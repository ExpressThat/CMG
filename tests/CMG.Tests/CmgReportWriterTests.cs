using CMG.Runner;
using System.Text.Json;

namespace CMG.Tests;

public sealed class CmgReportWriterTests
{
    [Fact]
    public void JsonReport_IncludesStepDiagnostics()
    {
        var report = CmgJsonReportWriter.Write([FailedTest()]);

        Assert.Contains("\"steps\"", report);
        Assert.Contains("\"status\": \"failed\"", report);
        Assert.Contains("\"lineNumber\": 12", report);
        Assert.Contains("No element matched", report);
    }

    [Fact]
    public void JsonReport_FiltersGeneratedEvaluateInternals()
    {
        var test = FailedTest() with
        {
            Output = [
                "PASS 001 line=1 action=evaluate \"(() => { return true; })()\"",
                "EVALUATE 001 line=1 action=evaluate true",
                "PASS 002 line=2 action=evaluate 1 + 1",
                "EVALUATE 002 line=2 action=evaluate 2"
            ],
            Steps = [
                new CmgStepResult(
                    1,
                    "evaluate",
                    true,
                    [
                        "PASS 001 line=1 action=evaluate \"(() => { return true; })()\"",
                        "EVALUATE 001 line=1 action=evaluate true"
                    ],
                    null,
                    null,
                    1,
                    "",
                    "evaluate"),
                new CmgStepResult(
                    2,
                    "evaluate",
                    true,
                    [
                        "PASS 002 line=2 action=evaluate 1 + 1",
                        "EVALUATE 002 line=2 action=evaluate 2"
                    ],
                    null,
                    null,
                    2,
                    "",
                    "evaluate")
            ]
        };

        var report = CmgJsonReportWriter.Write([test]);
        using var document = JsonDocument.Parse(report);
        var output = document.RootElement[0].GetProperty("output").EnumerateArray().Select(line => line.GetString()).ToArray();
        var stepOutput = document.RootElement[0].GetProperty("steps")[0].GetProperty("output").EnumerateArray().Select(line => line.GetString()).ToArray();

        Assert.DoesNotContain("PASS 001 line=1 action=evaluate \"(() => { return true; })()\"", output);
        Assert.DoesNotContain("EVALUATE 001 line=1 action=evaluate true", output);
        Assert.Contains("PASS 001 line=2 action=evaluate 1 + 1", output);
        Assert.Contains("EVALUATE 001 line=2 action=evaluate 2", output);
        Assert.DoesNotContain("PASS 001 line=1 action=evaluate \"(() => { return true; })()\"", stepOutput);
        Assert.DoesNotContain("EVALUATE 001 line=1 action=evaluate true", stepOutput);
        Assert.Contains("PASS 001 line=2 action=evaluate 1 + 1", stepOutput);
        Assert.Contains("EVALUATE 001 line=2 action=evaluate 2", stepOutput);
    }

    [Fact]
    public void JsonReport_IncludesStructuredStepFields()
    {
        var report = CmgJsonReportWriter.Write([FailedTest() with
        {
            Steps = [new CmgStepResult(42, "click", true, ["PASS 007 line=42 context=\"macro login > repeat[2/3]\" action=click #save"], null, null, 7, "macro login > repeat[2/3]", "click")]
        }]);

        using var document = JsonDocument.Parse(report);
        var step = document.RootElement[0].GetProperty("steps")[0];
        Assert.Equal(1, step.GetProperty("sequence").GetInt32());
        Assert.Equal(42, step.GetProperty("lineNumber").GetInt32());
        Assert.Equal("macro login > repeat[2/3]", step.GetProperty("context").GetString());
        Assert.Equal("click", step.GetProperty("action").GetString());
    }

    [Fact]
    public void HtmlReport_FiltersGeneratedInternalsAndShowsStepTable()
    {
        var report = CmgHtmlReportWriter.Write([FailedTest() with
        {
            Steps = [new CmgStepResult(42, "evaluate", true, [
                "PASS 001 line=42 action=evaluate new Promise((resolve, reject) => {})",
                "EVALUATE 001 line=42 action=evaluate true",
                "PASS 003 line=43 action=evaluate 1 + 1",
                "EVALUATE 003 line=43 action=evaluate 2"
            ], null, null, 1, "", "evaluate"),
            new CmgStepResult(43, "evaluate", true, [
                "PASS 003 line=43 action=evaluate 1 + 1",
                "EVALUATE 003 line=43 action=evaluate 2"
            ], null, null, 3, "", "evaluate")]
        }]);

        Assert.Contains("<table>", report);
        Assert.DoesNotContain("new Promise((resolve, reject)", report);
        Assert.Contains("EVALUATE 001 line=43 action=evaluate 2", report);
    }

    [Fact]
    public void JsonReport_PublicStepsOmitInternalsAndRenumberContiguously()
    {
        var report = CmgJsonReportWriter.Write([FailedTest() with
        {
            Output = [
                "PASS 005 line=20 action=evaluate new Promise((resolve, reject) => {})",
                "EVALUATE 005 line=20 action=evaluate true",
                "PASS 007 line=21 action=click #save",
                "PASS 009 line=22 action=screenshot #taskList",
                "SCREENSHOT 022 artifacts/task-list.png"
            ],
            Steps = [
                new CmgStepResult(20, "evaluate", true, [
                    "PASS 005 line=20 action=evaluate new Promise((resolve, reject) => {})",
                    "EVALUATE 005 line=20 action=evaluate true"
                ], null, null, 5, "", "evaluate"),
                new CmgStepResult(21, "click", true, [
                    "PASS 007 line=21 action=click #save"
                ], null, null, 7, "", "click"),
                new CmgStepResult(22, "screenshot", true, [
                    "PASS 009 line=22 action=screenshot #taskList",
                    "SCREENSHOT 022 artifacts/task-list.png"
                ], null, null, 9, "", "screenshot")
            ]
        }]);

        using var document = JsonDocument.Parse(report);
        var steps = document.RootElement[0].GetProperty("steps").EnumerateArray().ToArray();
        Assert.Equal(2, steps.Length);
        Assert.Equal([1, 2], steps.Select(step => step.GetProperty("sequence").GetInt32()).ToArray());
        Assert.Equal(["click", "screenshot"], steps.Select(step => step.GetProperty("action").GetString() ?? string.Empty).ToArray());
        var screenshotOutput = steps[1].GetProperty("output").EnumerateArray().Select(line => line.GetString()).ToArray();
        Assert.Contains("PASS 002 line=22 action=screenshot #taskList", screenshotOutput);
        Assert.Contains("SCREENSHOT 002 artifacts/task-list.png", screenshotOutput);
        Assert.DoesNotContain("new Promise((resolve, reject)", report);
    }

    [Fact]
    public void JsonReport_OmitsSequenceZeroPlannedPlaceholders()
    {
        var report = CmgJsonReportWriter.Write([FailedTest() with
        {
            Steps = [
                new CmgStepResult(20, "click", true, [], null, null),
                new CmgStepResult(20, "click", true, ["PASS 004 line=20 action=click #save"], null, null, 4, "", "click")
            ]
        }]);

        using var document = JsonDocument.Parse(report);
        var steps = document.RootElement[0].GetProperty("steps").EnumerateArray().ToArray();
        Assert.Single(steps);
        Assert.Equal(1, steps[0].GetProperty("sequence").GetInt32());
    }

    [Fact]
    public void HtmlReport_IncludesFailureReason()
    {
        var report = CmgHtmlReportWriter.Write([FailedTest()]);

        Assert.Contains("No element matched", report);
        Assert.Contains("checkout", report);
    }

    [Fact]
    public void JUnitReport_IncludesFailureNode()
    {
        var report = CmgJUnitReportWriter.Write([FailedTest()]);

        Assert.Contains("<failure", report);
        Assert.Contains("No element matched", report);
    }

    [Fact]
    public void Reports_IncludeSkippedStatus()
    {
        var skipped = FailedTest() with { Success = true, Error = "Not relevant" };
        skipped = skipped with { Output = [] };
        skipped = skipped with { Steps = [] };
        skipped = skipped with { GifPath = null };
        skipped = skipped with { Status = "skipped" };

        Assert.Contains("\"status\": \"skipped\"", CmgJsonReportWriter.Write([skipped]));
        Assert.Contains("checkout - skipped", CmgHtmlReportWriter.Write([skipped]));
        var junit = CmgJUnitReportWriter.Write([skipped]);
        Assert.Contains("skipped=\"1\"", junit);
        Assert.Contains("<skipped", junit);
    }

    [Fact]
    public void Reports_IncludeAnnotations()
    {
        var annotated = FailedTest() with
        {
            Annotations = [
                new CmgAnnotation("owner", "qa"),
                new CmgAnnotation("issue", "BUG-7")
            ]
        };

        Assert.Contains("\"annotations\"", CmgJsonReportWriter.Write([annotated]));
        Assert.Contains("\"type\": \"owner\"", CmgJsonReportWriter.Write([annotated]));
        Assert.Contains("owner: qa", CmgHtmlReportWriter.Write([annotated]));
        Assert.Contains("cmg.annotation.issue", CmgJUnitReportWriter.Write([annotated]));
    }

    [Fact]
    public void Reports_IncludeProject()
    {
        var projected = FailedTest() with { Project = "firefox-smoke" };

        Assert.Contains("\"project\": \"firefox-smoke\"", CmgJsonReportWriter.Write([projected]));
        Assert.Contains("Project: firefox-smoke", CmgHtmlReportWriter.Write([projected]));
        Assert.Contains("cmg.project", CmgJUnitReportWriter.Write([projected]));
    }

    private static CmgTestResult FailedTest() =>
        new(
            "checkout",
            "checkout.cmgscript",
            false,
            ["PASS 001 navigate"],
            "No element matched selector '#pay'",
            "checkout.gif",
            [new CmgStepResult(12, "click", false, [], "No element matched selector '#pay'", "checkout.gif")])
        {
            Tags = "smoke"
        };
}
