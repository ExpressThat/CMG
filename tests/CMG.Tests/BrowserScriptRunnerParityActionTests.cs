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

    [Fact]
    public void RunText_ExpectTextAliasesAssertText()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Saved");
        var result = Runner().RunText("expectText \"#status\" Saved", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#status", client.LastElementTextSelector);
    }

    [Fact]
    public void RunText_AssertVisibleAliasesWaitForElement()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("assertVisible \"#save\" timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#save", client.LastWaitSelector);
    }

    [Fact]
    public void RunText_WaitAliasesElementWait()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("wait \"#ready\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#ready", client.LastWaitSelector);
    }

    [Fact]
    public void RunText_CaptionAliasesShowMessageBar()
    {
        var result = Runner().RunText("caption Saved", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
