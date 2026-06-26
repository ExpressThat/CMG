using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderNetworkAliasTests
{
    [Theory]
    [InlineData("route /api/profile --status 200", "route \"/api/profile\" status=\"200\"")]
    [InlineData("intercept /api/profile --method POST --times 1", "intercept \"/api/profile\" method=\"POST\" times=\"1\"")]
    [InlineData("mockResponse /api/profile --body ok --content-type text/plain", "mockResponse \"/api/profile\" body=\"ok\" contentType=\"text/plain\"")]
    public void RouteAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control network {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Theory]
    [InlineData("setHeaders X-Test yes", "setExtraHTTPHeaders \"X-Test\" \"yes\"")]
    [InlineData("setExtraHTTPHeaders X-Test yes", "setExtraHTTPHeaders \"X-Test\" \"yes\"")]
    [InlineData("clearExtraHTTPHeaders", "clearExtraHTTPHeaders")]
    [InlineData("setCredentials user pass", "setHttpCredentials \"user\" \"pass\"")]
    [InlineData("setHttpCredentials user pass", "setHttpCredentials \"user\" \"pass\"")]
    [InlineData("httpCredentials user pass", "setHttpCredentials \"user\" \"pass\"")]
    [InlineData("authenticate user pass", "setHttpCredentials \"user\" \"pass\"")]
    [InlineData("clearHttpCredentials", "clearHttpCredentials")]
    [InlineData("proxy https://proxy.local/", "setProxy \"https://proxy.local/\"")]
    public void EnvironmentAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control network {commandTail}").Invoke();

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
