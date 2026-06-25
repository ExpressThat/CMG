using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerPageErrorActionTests
{
    [Fact]
    public void RunText_CapturePageErrorsInstallsHook()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("capturePageErrors", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgPageErrors", client.LastExpression);
        Assert.Contains("PAGE_ERROR_CAPTURE 001", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForPageErrorEvaluatesMatcher()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForPageError \"boom\" timeout=10", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("Page error", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("PAGE_ERROR 001", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
