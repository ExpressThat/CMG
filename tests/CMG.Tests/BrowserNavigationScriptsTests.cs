using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserNavigationScriptsTests
{
    [Fact]
    public void WaitForUrlPollsWithClearTimeoutFailure()
    {
        var script = BrowserNavigationScripts.WaitForUrl("/checkout", 250);

        Assert.Contains("new Promise", script);
        Assert.Contains("within 250ms", script);
        Assert.Contains("Last URL", script);
    }

    [Fact]
    public void HistoryReportsDirectionAndTimeout()
    {
        var script = BrowserNavigationScripts.History("back", 500);

        Assert.Contains("history.back()", script);
        Assert.Contains("History back did not change URL within 500ms", script);
    }

    [Fact]
    public void WaitForLoadStateAcceptsLoadAlias()
    {
        var script = BrowserNavigationScripts.WaitForLoadState("load", 1000);

        Assert.Contains("document.readyState === 'complete'", script);
        Assert.Contains("Load state", script);
    }
}
