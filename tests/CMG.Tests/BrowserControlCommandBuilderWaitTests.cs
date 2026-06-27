using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderWaitTests
{
    [Theory]
    [InlineData("element #ready", "waitForElement \"#ready\"")]
    [InlineData("element #ready --timeout 1000", "waitForElement \"#ready\" timeout=\"1000\"")]
    [InlineData("selector text=Ready --timeout 250", "waitForSelector \"text=Ready\" timeout=\"250\"")]
    [InlineData("selector #ready --state visible --timeout 250", "waitForSelector \"#ready\" timeout=\"250\" state=\"visible\"")]
    [InlineData("function window.appReady", "waitForFunction \"window.appReady\"")]
    [InlineData("function window.appReady --timeout 5000", "waitForFunction \"window.appReady\" timeout=\"5000\"")]
    [InlineData("timeout 25", "waitForTimeout \"25\"")]
    [InlineData("waitForElement #ready --timeout 1000", "waitForElement \"#ready\" timeout=\"1000\"")]
    [InlineData("waitForSelector text=Ready --timeout 250", "waitForSelector \"text=Ready\" timeout=\"250\"")]
    [InlineData("waitForSelector #toast --state hidden", "waitForSelector \"#toast\" state=\"hidden\"")]
    [InlineData("waitForFunction window.appReady --timeout 5000", "waitForFunction \"window.appReady\" timeout=\"5000\"")]
    [InlineData("waitForTimeout 25", "waitForTimeout \"25\"")]
    [InlineData("auto 25", "wait \"25\"")]
    [InlineData("auto #ready --timeout 1000", "wait \"#ready\" timeout=\"1000\"")]
    public void WaitCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control wait {commandTail}").Invoke();

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
