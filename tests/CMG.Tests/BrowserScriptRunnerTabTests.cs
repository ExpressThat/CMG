using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerTabTests
{
    [Fact]
    public void RunText_OpenTabEvaluatesWindowOpen()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("openTab \"https://example.com\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("window.open", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("TAB_OPENED", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForTabPollsUntilCountMatches()
    {
        var client = new FakeAutomationClient();
        client.TabResponses.Enqueue([new ChromePageTab("1", "one", "about:blank")]);
        client.TabResponses.Enqueue([
            new ChromePageTab("1", "one", "about:blank"),
            new ChromePageTab("2", "two", "https://example.com")
        ]);

        var result = Runner().RunText("waitForTab count=2 timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("TAB_COUNT", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForTabReportsTimeout()
    {
        var client = new FakeAutomationClient();
        client.TabResponses.Enqueue([new ChromePageTab("1", "one", "about:blank")]);

        var result = Runner().RunText("waitForTab count=2 timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Expected at least 2 tab", result.Error);
    }

    [Fact]
    public void RunText_WaitForPopupAliasesWaitForTab()
    {
        var client = new FakeAutomationClient();
        client.TabResponses.Enqueue([new ChromePageTab("1", "one", "about:blank")]);

        var result = Runner().RunText("waitForPopup count=1 timeout=1", "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("TAB_COUNT", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
