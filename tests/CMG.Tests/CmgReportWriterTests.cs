using CMG.Runner;

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
