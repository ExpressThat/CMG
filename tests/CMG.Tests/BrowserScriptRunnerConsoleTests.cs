using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerConsoleTests
{
    [Fact]
    public void RunText_CaptureConsoleInstallsHook()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("captureConsole", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgConsole", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("CONSOLE_CAPTURE", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForConsoleUsesExpectedTextAndLevel()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForConsole \"saved\" level=info timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("saved", client.LastExpression);
        Assert.Contains("info", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("CONSOLE", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
