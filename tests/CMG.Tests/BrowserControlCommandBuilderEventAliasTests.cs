using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderEventAliasTests
{
    [Theory]
    [InlineData("console captureConsole", "captureConsole")]
    [InlineData("console waitForConsole Ready --level error", "waitForConsole \"Ready\" level=\"error\"")]
    [InlineData("dialogs captureDialogs --prompt-text ok", "captureDialogs promptText=\"ok\"")]
    [InlineData("dialogs setDialogBehavior dismiss", "setDialogBehavior \"dismiss\"")]
    [InlineData("dialogs onDialog accept --prompt-text yes", "onDialog \"accept\" promptText=\"yes\"")]
    [InlineData("dialogs handleDialog dismiss", "handleDialog \"dismiss\"")]
    [InlineData("dialogs dialogBehavior accept", "dialogBehavior \"accept\"")]
    [InlineData("dialogs waitForDialog Confirm", "waitForDialog \"Confirm\"")]
    [InlineData("pageErrors capturePageErrors", "capturePageErrors")]
    [InlineData("pageErrors waitForPageError Boom", "waitForPageError \"Boom\"")]
    [InlineData("waitForEvent response /api --status 200", "waitForEvent \"response\" \"/api\" status=\"200\"")]
    public void EventAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control events {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
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
