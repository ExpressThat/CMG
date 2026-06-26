using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderScriptTests
{
    [Fact]
    public void ScriptCommand_MapsGifAndTraceOptions()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --trace C:\\temp\\flow.trace.json").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("flow.cmgscript", handler.File);
        Assert.Equal("C:\\temp\\flow.gif", handler.Gif?.FullName);
        Assert.Equal("C:\\temp\\flow.trace.json", handler.Trace?.FullName);
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
        public string? File { get; private set; }

        public FileInfo? Gif { get; private set; }

        public FileInfo? Trace { get; private set; }

        public int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output) => 0;

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif)
        {
            File = file;
            Gif = gif;
            return 0;
        }

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace)
        {
            File = file;
            Gif = gif;
            Trace = trace;
            return 0;
        }

        public int RunScriptAction(BrowserKind browserKind, string scriptLine) => 0;

        public int ValidateScript(string file) => 0;
    }
}
