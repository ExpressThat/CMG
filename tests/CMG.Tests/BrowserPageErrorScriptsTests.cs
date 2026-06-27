using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserPageErrorScriptsTests
{
    [Fact]
    public void InstallPageErrorsCapturesErrorAndRejection()
    {
        var script = BrowserConsoleScripts.InstallPageErrors();

        Assert.Contains("__cmgPageErrors", script);
        Assert.Contains("unhandledrejection", script);
        Assert.Contains("addEventListener('error'", script);
    }

    [Fact]
    public void WaitForPageErrorReportsExpectedTextAndTimeout()
    {
        var script = BrowserConsoleScripts.WaitForPageError("boom", 1000);

        Assert.Contains("boom", script);
        Assert.Contains("Page error", script);
        Assert.Contains("1000", script);
    }
}
