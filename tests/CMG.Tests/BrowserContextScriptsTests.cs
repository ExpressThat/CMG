using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserContextScriptsTests
{
    [Fact]
    public void Clear_RemovesStorageCookiesDatabasesCachesAndWorkers()
    {
        var script = BrowserContextScripts.Clear(navigateBlank: false);

        Assert.Contains("localStorage.clear", script);
        Assert.Contains("indexedDB.databases", script);
        Assert.Contains("caches.keys", script);
        Assert.Contains("serviceWorker.getRegistrations", script);
    }

    [Fact]
    public void Clear_CanNavigateToBlankPage()
    {
        var script = BrowserContextScripts.Clear(navigateBlank: true);

        Assert.Contains("about:blank", script);
        Assert.Contains("location.href", script);
    }
}
