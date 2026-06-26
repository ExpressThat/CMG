using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderScriptTests
{
    [Fact]
    public void Set_IsNotExposedAsAControlCommand()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var result = BuildRoot(handler).Parse("control set title value");

        Assert.True(result.Errors.Count > 0);
        Assert.Null(handler.ScriptLine);
    }

    [Fact]
    public void ScriptCommand_MapsGifAndTraceOptions()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --trace C:\\temp\\flow.trace.json --timeout 700 --navigation-timeout 800 --assertion-timeout 900 --var user=Ada --env mode=demo").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("flow.cmgscript", handler.File);
        Assert.Equal("C:\\temp\\flow.gif", handler.Gif?.FullName);
        Assert.Equal("C:\\temp\\flow.trace.json", handler.Trace?.FullName);
        Assert.Equal(new ScriptTimeoutOptions(700, 800, 900), handler.Timeouts);
        Assert.Equal("Ada", handler.Variables["user"]);
        Assert.Equal("demo", handler.Variables["mode"]);
    }

    [Fact]
    public void ScriptCommand_RejectsMalformedVariable()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --var broken").Invoke();

        Assert.Equal(1, exitCode);
        Assert.Null(handler.File);
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

        public string? ScriptLine { get; private set; }

        public FileInfo? Gif { get; private set; }

        public FileInfo? Trace { get; private set; }

        public ScriptTimeoutOptions? Timeouts { get; private set; }

        public IReadOnlyDictionary<string, string> Variables { get; private set; } =
            new Dictionary<string, string>();

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

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts)
        {
            RunScript(browserKind, file, gif, trace);
            Timeouts = timeouts;
            return 0;
        }

        public int RunScript(
            BrowserKind browserKind,
            string file,
            FileInfo? gif,
            FileInfo? trace,
            ScriptTimeoutOptions? timeouts,
            IReadOnlyDictionary<string, string> variables)
        {
            RunScript(browserKind, file, gif, trace, timeouts);
            Variables = variables;
            return 0;
        }

        public int RunScriptAction(BrowserKind browserKind, string scriptLine)
        {
            ScriptLine = scriptLine;
            return 0;
        }

        public int ValidateScript(string file) => 0;
    }
}
