using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgReportWriterTests
{
    [Fact]
    public void JsonReport_IncludesStepDiagnostics()
    {
        var report = CmgJsonReportWriter.Write([FailedTest()]);

        Assert.Contains("\"steps\"", report);
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
