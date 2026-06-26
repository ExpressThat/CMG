using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderNavigationTabCaptureAliasTests
{
    [Theory]
    [InlineData("navigate https://example.test --wait-until load --timeout 250", "navigate \"https://example.test\" waitUntil=\"load\" timeout=\"250\"")]
    [InlineData("goto https://example.test", "goto \"https://example.test\"")]
    [InlineData("goto https://example.test --wait-until domcontentloaded", "goto \"https://example.test\" waitUntil=\"domcontentloaded\"")]
    [InlineData("visit https://example.test", "visit \"https://example.test\"")]
    [InlineData("waitForNetworkIdle", "waitForNetworkIdle timeout=\"5000\"")]
    [InlineData("waitForNetworkIdle --timeout 250", "waitForNetworkIdle timeout=\"250\"")]
    [InlineData("networkIdle", "networkIdle timeout=\"5000\"")]
    [InlineData("networkIdle --timeout 250", "networkIdle timeout=\"250\"")]
    public void NavigationAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control navigation {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Theory]
    [InlineData("listTabs", "listTabs")]
    [InlineData("openTab about:blank", "openTab \"about:blank\"")]
    [InlineData("waitForTab --count 2 --timeout 1000", "waitForTab count=\"2\" timeout=\"1000\"")]
    [InlineData("waitForPopup --count 2 --timeout 1000", "waitForPopup count=\"2\" timeout=\"1000\"")]
    [InlineData("activateTab --index 1", "activateTab index=\"1\"")]
    [InlineData("closeTab --index 1", "closeTab index=\"1\"")]
    public void TabAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control tabs {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Fact]
    public void PdfAliasCommand_MapsToScriptAction()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse("control capture pdf --path C:\\temp\\page.pdf --landscape true").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("pdf path=\"C:\\\\temp\\\\page.pdf\" landscape=\"true\"", handler.ScriptLine);
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
