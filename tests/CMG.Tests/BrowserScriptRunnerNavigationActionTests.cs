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
    public void RunText_WaitForUrlReturnsMatchedUrl()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("https://example.test/checkout");
        var result = Runner().RunText("waitForUrl \"/checkout\" timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("location.href", client.LastExpression);
        Assert.Contains("URL 001 https://example.test/checkout", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForUrlReportsTimeoutReason()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("https://example.test/cart");
        var result = Runner().RunText("waitForUrl \"/checkout\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("URL did not match /checkout within 1ms", result.Error);
    }

    [Fact]
    public void RunText_ExpectTitleReturnsTitleOutput()
    {
        var result = Runner().RunText("expectTitle \"Dashboard\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("TITLE 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForTitleReturnsTitleOutput()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Dashboard Home");
        var result = Runner().RunText("waitForTitle \"Dashboard\" timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("document.title", client.LastExpression);
        Assert.Contains("TITLE 001 Dashboard Home", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForTitleReportsTimeoutReason()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Dashboard Home");
        var result = Runner().RunText("waitForTitle \"Missing\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Title did not match Missing within 1ms", result.Error);
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
