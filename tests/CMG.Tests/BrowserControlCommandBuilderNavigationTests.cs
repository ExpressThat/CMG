using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderNavigationTests
{
    [Theory]
    [InlineData("reload", "reload")]
    [InlineData("reload --wait-until networkidle --timeout 250", "reload waitUntil=\"networkidle\" timeout=\"250\"")]
    [InlineData("expectUrl checkout", "expectUrl \"checkout\"")]
    [InlineData("expectUrl checkout --match exact --ignore-case", "expectUrl \"checkout\" match=\"exact\" ignoreCase=\"true\"")]
    [InlineData("expectTitle Dashboard", "expectTitle \"Dashboard\"")]
    [InlineData("expectTitle Dashboard --match regex", "expectTitle \"Dashboard\" match=\"regex\"")]
    [InlineData("waitForUrl checkout --match regex --ignore-case", "waitForUrl \"checkout\" timeout=\"5000\" match=\"regex\" ignoreCase=\"true\"")]
    [InlineData("waitForTitle Dashboard", "waitForTitle \"Dashboard\" timeout=\"5000\"")]
    [InlineData("waitForTitle Dashboard --timeout 250 --match exact --ignore-case", "waitForTitle \"Dashboard\" timeout=\"250\" match=\"exact\" ignoreCase=\"true\"")]
    [InlineData("toHaveURL checkout", "toHaveURL \"checkout\"")]
    [InlineData("toHaveTitle Dashboard", "toHaveTitle \"Dashboard\"")]
    [InlineData("waitForNavigation", "waitForNavigation")]
    [InlineData("waitForNavigation checkout", "waitForNavigation \"checkout\"")]
    [InlineData("waitForNavigation checkout --wait-until domcontentloaded --timeout 250", "waitForNavigation \"checkout\" waitUntil=\"domcontentloaded\" timeout=\"250\"")]
    public void NavigationCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control navigation {commandTail}").Invoke();

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
