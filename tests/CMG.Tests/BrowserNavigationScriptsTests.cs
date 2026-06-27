using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserNavigationScriptsTests
{
    [Fact]
    public void ExpectUrlReportsCurrentUrlOnMismatch()
    {
        var script = BrowserNavigationScripts.ExpectUrl("/checkout");

        Assert.Contains("Expected URL to contain /checkout", script);
        Assert.Contains("location.href", script);
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
