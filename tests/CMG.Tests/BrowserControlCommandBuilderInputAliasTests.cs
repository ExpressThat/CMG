using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderInputAliasTests
{
    [Theory]
    [InlineData("dblclick #save", "dblclick \"#save\"")]
    [InlineData("contextClick #save", "contextClick \"#save\"")]
    [InlineData("touchTap #save", "touchTap \"#save\"")]
    [InlineData("pressSequentially #name CMG", "pressSequentially \"#name\" \"CMG\"")]
    [InlineData("type #name CMG --delay 25", "type \"#name\" \"CMG\" delay=\"25\"")]
    [InlineData("pressSequentially #name CMG --delay 25", "pressSequentially \"#name\" \"CMG\" delay=\"25\"")]
    [InlineData("shortcut Control+S", "keyboardShortcut \"Control+S\"")]
    [InlineData("hotkey Control+Shift+P", "keyboardShortcut \"Control+Shift+P\"")]
    [InlineData("keyboardShortcut Meta+K", "keyboardShortcut \"Meta+K\"")]
    [InlineData("selectOption #plan pro", "selectOption \"#plan\" \"pro\"")]
    [InlineData("click #save --button middle --click-count 2 --delay 10 --modifiers Control+Shift --x 4 --y 8", "click \"#save\" button=\"middle\" clickCount=\"2\" delay=\"10\" modifiers=\"Control+Shift\" x=\"4\" y=\"8\"")]
    public void InputAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control input {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Theory]
    [InlineData("setInputFiles #file C:\\temp\\avatar.png", "setInputFiles \"#file\" \"C:\\\\temp\\\\avatar.png\"")]
    [InlineData("selectFile #file C:\\temp\\avatar.png", "selectFile \"#file\" \"C:\\\\temp\\\\avatar.png\"")]
    public void UploadAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control input {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    private static RootCommand BuildRoot(CapturingBrowserControlCommandHandler handler)
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var root = new RootCommand();
        root.Options.Add(chrome);
        root.Options.Add(edge);
        root.Options.Add(firefox);
        root.Subcommands.Add(new BrowserControlCommandBuilder(handler).Build(new BrowserSelectionOptions(chrome, edge, firefox)));
        return root;
    }

    private sealed class CapturingBrowserControlCommandHandler : IBrowserControlCommandHandler
    {
        public BrowserKind BrowserKind { get; private set; }

        public string? ScriptLine { get; private set; }

        public int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output) => 0;

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif) => 0;

        public int ValidateScript(string file) => 0;

        public int RunScriptAction(BrowserKind browserKind, string scriptLine)
        {
            BrowserKind = browserKind;
            ScriptLine = scriptLine;
            return 0;
        }
    }
}
