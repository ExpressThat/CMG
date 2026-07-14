using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgHtmlReportSummaryTests
{
    [Fact]
    public void Write_GroupsFailuresAndBuildsRunFilmstrip()
    {
        var first = FailedTest("checkout", "artifacts/checkout.gif");
        var second = FailedTest("payment", "artifacts/payment.gif");

        var report = CmgHtmlReportWriter.Write([first, second]);

        Assert.Contains("Run summary", report);
        Assert.Contains("Total: 2 | Passed: 0 | Failed: 2 | Skipped: 0", report);
        Assert.Contains("Failures by reason", report);
        Assert.Contains("No element matched selector &#39;#pay&#39; (2)", report);
        Assert.Contains("Run filmstrip", report);
        Assert.Contains("Visual evidence for checkout", report);
        Assert.Contains("Visual evidence for payment", report);
    }

    private static CmgTestResult FailedTest(string name, string gifPath) =>
        new(name, "checkout.cmgscript", false, [], "No element matched selector '#pay'", gifPath, []);
}
