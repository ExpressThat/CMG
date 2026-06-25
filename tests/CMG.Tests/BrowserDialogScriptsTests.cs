using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserDialogScriptsTests
{
    [Fact]
    public void InstallOverridesDialogFunctionsAndStoresBehavior()
    {
        var script = BrowserDialogScripts.Install("dismiss", "typed");

        Assert.Contains("__cmgDialogs", script);
        Assert.Contains("window.confirm", script);
        Assert.Contains("window.prompt", script);
        Assert.Contains("dismiss", script);
    }

    [Fact]
    public void WaitForDialogReturnsEnvelopeInsteadOfRejecting()
    {
        var script = BrowserDialogScripts.WaitForDialog("Save?", 500);

        Assert.Contains("success: true", script);
        Assert.Contains("Timed out waiting for dialog", script);
        Assert.Contains("Save?", script);
    }
}
