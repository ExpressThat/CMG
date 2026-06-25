using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserNetworkEnvironmentScriptsTests
{
    [Fact]
    public void ExtraHeadersPatchesFetchAndXhr()
    {
        var script = BrowserNetworkEnvironmentScripts.ExtraHeaders(new Dictionary<string, string> { ["X-CMG"] = "yes" });

        Assert.Contains("__cmgExtraHeaders", script);
        Assert.Contains("window.fetch", script);
        Assert.Contains("setRequestHeader", script);
        Assert.Contains("X-CMG", script);
    }

    [Fact]
    public void OfflinePatchesNavigatorFetchAndXhr()
    {
        var script = BrowserNetworkEnvironmentScripts.Offline(offline: true);

        Assert.Contains("__cmgOffline = true", script);
        Assert.Contains("navigator.onLine", script);
        Assert.Contains("CMG offline mode is enabled", script);
    }
}
