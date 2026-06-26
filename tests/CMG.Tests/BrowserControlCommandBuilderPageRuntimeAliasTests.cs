using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderPageRuntimeAliasTests
{
    [Theory]
    [InlineData("runtime evalOnSelector #title \"el => el.textContent\"", "evalOnSelector \"#title\" \"el => el.textContent\"")]
    [InlineData("runtime evalAll .row \"els => els.length\"", "evalAll \".row\" \"els => els.length\"")]
    [InlineData("runtime evaluateOnNewDocument \"window.ready = true\"", "evaluateOnNewDocument \"window.ready = true\"")]
    [InlineData("runtime count .row", "count \".row\"")]
    [InlineData("runtime locatorCount .row", "locatorCount \".row\"")]
    [InlineData("runtime boundingBox #card", "boundingBox \"#card\"")]
    [InlineData("runtime allTextContents .item", "allTextContents \".item\"")]
    [InlineData("runtime allInnerTexts .item", "allInnerTexts \".item\"")]
    public void RuntimeAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control page {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Theory]
    [InlineData("setViewport --width 1024 --height 768", "setViewport width=\"1024\" height=\"768\"")]
    [InlineData("viewport --width 390 --height 844 --mobile --touch", "setViewport width=\"390\" height=\"844\" isMobile=\"true\" hasTouch=\"true\"")]
    [InlineData("setViewportSize --width 1280 --height 720 --device-scale-factor 2", "setViewport width=\"1280\" height=\"720\" deviceScaleFactor=\"2\"")]
    public void ViewportAliasCommands_MapToSetViewportAction(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control page {commandTail}").Invoke();

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
