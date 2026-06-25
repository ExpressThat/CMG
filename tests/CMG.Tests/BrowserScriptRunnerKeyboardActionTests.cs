using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerKeyboardActionTests
{
    [Fact]
    public void RunText_KeyDownAndKeyUpUseClientPrimitives()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("keyDown Shift\nkeyUp Shift", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("Shift", client.LastKeyDown);
        Assert.Equal("Shift", client.LastKeyUp);
        Assert.Contains("KEY_DOWN 001 Shift", result.StdoutLines);
        Assert.Contains("KEY_UP 002 Shift", result.StdoutLines);
    }

    [Fact]
    public void RunText_InsertTextUsesClientPrimitive()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("insertText \"hello\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("hello", client.LastInsertedText);
        Assert.Contains("TEXT_INSERTED 001 5", result.StdoutLines);
    }

    [Fact]
    public void RunText_KeyDownRequiresKey()
    {
        var result = Runner().RunText("keyDown", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Expected 1 positional", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
