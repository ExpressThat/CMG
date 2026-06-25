using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerNavigationActionTests
{
    [Fact]
    public void RunText_ReloadNavigatesCurrentUrl()
    {
        var result = Runner().RunText("reload", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("RELOADED 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_GoBackUsesHistoryScript()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("goBack timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("history.back()", client.LastExpression);
        Assert.Contains("BACK 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForUrlUsesPollingScript()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForUrl \"/checkout\" timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("URL did not match", client.LastExpression);
        Assert.Contains("URL 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_ExpectTitleReturnsTitleOutput()
    {
        var result = Runner().RunText("expectTitle \"Dashboard\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("TITLE 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForLoadStateSupportsNetworkIdle()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForLoadState networkidle", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("networkidle", client.LastExpression);
        Assert.Contains("LOAD_STATE 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForLoadStateValidatesState()
    {
        var result = Runner().RunText("waitForLoadState paint", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("loading, interactive, complete, load, or networkidle", result.Error);
    }

    [Fact]
    public void RunText_WaitForNavigationUsesPollingScript()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForNavigation \"/checkout\" waitUntil=domcontentloaded timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("Navigation did not reach", client.LastExpression);
        Assert.Contains("domcontentloaded", client.LastExpression);
        Assert.Contains("NAVIGATION 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForNavigationValidatesWaitUntil()
    {
        var result = Runner().RunText("waitForNavigation waitUntil=paint", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("load, domcontentloaded, networkidle, or commit", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
