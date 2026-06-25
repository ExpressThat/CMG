using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerParityActionTests
{
    [Fact]
    public void RunText_ApiRequestUsesRunnerDiagnostics()
    {
        var result = Runner().RunText("apiRequest \"GET\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("apiRequest requires method and URL", result.Error);
    }

    [Fact]
    public void RunText_StorageStateUsesRunnerDiagnostics()
    {
        var result = Runner().RunText("storageState", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("storageState requires save or load", result.Error);
    }

    [Fact]
    public void RunText_UploadFilesIsAvailableInBrowserScripts()
    {
        var result = Runner().RunText("uploadFiles \"#file\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("uploadFiles requires a selector", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
