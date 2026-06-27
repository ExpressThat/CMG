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

    [Fact]
    public void RunText_ProviderNavigationAssertionAliasesExecute()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("https://example.test/cart");
        client.EvaluateResponses.Enqueue("Cart");

        var result = Runner().RunText("""
        toHaveURL "/cart"
        toHaveTitle "Cart"
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("URL 001 https://example.test/cart", result.StdoutLines);
        Assert.Contains("TITLE 002 Cart", result.StdoutLines);
    }

    [Fact]
    public void RunText_ProviderInputAliasesUsePointerAwareActions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        pressSequentially "#name" "CMG"
        dragTo "#source" "#target"
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#name", client.LastTypedSelector);
        Assert.Equal("CMG", client.LastTypedText);
        Assert.Equal("#source", client.LastDragSource);
        Assert.Equal("#target", client.LastDragTarget);
    }

    [Fact]
    public void RunText_ToContainTextChecksBodyWithOneArgument()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Ready");

        var result = Runner().RunText("toContainText Ready", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("body", client.LastElementTextSelector);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
