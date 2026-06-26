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

    [Fact]
    public void RunText_KeyboardShortcutPressesModifiersThenKeyAndReleasesReverse()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("keyboardShortcut \"Control+Shift+P\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(["down:Control", "down:Shift", "press:P", "up:Shift", "up:Control"], client.KeyEvents);
        Assert.Contains("KEYBOARD_SHORTCUT 001 Control+Shift+P", result.StdoutLines);
    }

    [Fact]
    public void RunText_PressSupportsShortcutChord()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("press \"Ctrl+A\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(["down:Control", "press:A", "up:Control"], client.KeyEvents);
        Assert.Contains("KEYBOARD_SHORTCUT 001 Ctrl+A", result.StdoutLines);
    }

    [Fact]
    public void RunText_PressDelayHoldsSingleKey()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("press Enter delay=1", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(["down:Enter", "up:Enter"], client.KeyEvents);
    }

    [Fact]
    public void RunText_PressDelayHoldsFinalChordKey()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("press \"Ctrl+A\" delay=1", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(["down:Control", "down:A", "up:A", "up:Control"], client.KeyEvents);
        Assert.Contains("KEYBOARD_SHORTCUT 001 Ctrl+A", result.StdoutLines);
    }

    [Fact]
    public void RunText_PressDelayValidatesInput()
    {
        var result = Runner().RunText("press Enter delay=-1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("delay= must be zero or greater", result.Error);
    }

    [Theory]
    [InlineData("shortcut")]
    [InlineData("hotkey")]
    public void RunText_KeyboardShortcutAliasesWork(string action)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText($"{action} \"Cmd+K\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(["down:Meta", "press:K", "up:Meta"], client.KeyEvents);
    }

    [Fact]
    public void RunText_KeyboardShortcutRejectsSingleKey()
    {
        var result = Runner().RunText("keyboardShortcut Enter", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("expects a key chord", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
