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
                "PASS 001 evaluate \"(() => { return true; })()\"",
                "EVALUATE 001 true",
                "PASS 002 evaluate 1 + 1",
                "EVALUATE 002 2"
            ],
            Steps = [
                new CmgStepResult(
                    1,
                    "evaluate",
                    true,
                    [
                        "PASS 001 evaluate \"(() => { return true; })()\"",
                        "EVALUATE 001 true",
                        "PASS 002 evaluate 1 + 1",
                        "EVALUATE 002 2"
                    ],
                    null,
                    null)
            ]
        };

        var report = CmgJsonReportWriter.Write([test]);
        using var document = JsonDocument.Parse(report);
        var output = document.RootElement[0].GetProperty("output").EnumerateArray().Select(line => line.GetString()).ToArray();
        var stepOutput = document.RootElement[0].GetProperty("steps")[0].GetProperty("output").EnumerateArray().Select(line => line.GetString()).ToArray();

        Assert.DoesNotContain("PASS 001 evaluate \"(() => { return true; })()\"", output);
        Assert.DoesNotContain("EVALUATE 001 true", output);
        Assert.Contains("PASS 002 evaluate 1 + 1", output);
        Assert.Contains("EVALUATE 002 2", output);
        Assert.DoesNotContain("PASS 001 evaluate \"(() => { return true; })()\"", stepOutput);
        Assert.DoesNotContain("EVALUATE 001 true", stepOutput);
        Assert.Contains("PASS 002 evaluate 1 + 1", stepOutput);
        Assert.Contains("EVALUATE 002 2", stepOutput);
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
