using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderContextTests
{
    [Theory]
    [InlineData("setJavaScriptEnabled false", "setJavaScriptEnabled \"false\"")]
    [InlineData("javaScriptEnabled true", "javaScriptEnabled \"true\"")]
    [InlineData("serviceWorkers block", "serviceWorkers \"block\"")]
    [InlineData("setServiceWorkers allow", "setServiceWorkers \"allow\"")]
    [InlineData("clearContext", "clearContext")]
    [InlineData("resetContext", "resetContext")]
    public void ContextAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control context {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Theory]
    [InlineData("newContext --url about:blank", "newContext url=\"about:blank\"")]
    [InlineData("useContext ctx-1", "useContext \"ctx-1\"")]
    [InlineData("listContexts", "listContexts")]
    [InlineData("closeContext ctx-1", "closeContext \"ctx-1\"")]
    public void BrowserContextAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control context browserContexts {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Fact]
    public void EmulateCommand_MapsDeviceOptionToScriptAction()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse("control context emulate --device \"Pixel 7\" --locale en-GB").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("emulate device=\"Pixel 7\" locale=\"en-GB\"", handler.ScriptLine);
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
