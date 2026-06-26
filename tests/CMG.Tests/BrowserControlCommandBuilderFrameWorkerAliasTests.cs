using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderFrameWorkerAliasTests
{
    [Theory]
    [InlineData("frameClick iframe #save", "frameClick \"iframe\" \"#save\"")]
    [InlineData("frameHover iframe #save", "frameHover \"iframe\" \"#save\"")]
    [InlineData("frameType iframe #name CMG", "frameType \"iframe\" \"#name\" \"CMG\"")]
    [InlineData("frameFill iframe #name CMG", "frameFill \"iframe\" \"#name\" \"CMG\"")]
    [InlineData("frameAssertText iframe #status Ready", "frameAssertText \"iframe\" \"#status\" \"Ready\"")]
    [InlineData("frameWaitForElement iframe #ready --timeout 1000", "frameWaitForElement \"iframe\" \"#ready\" timeout=\"1000\"")]
    [InlineData("frameEvaluate iframe document.title", "frameEvaluate \"iframe\" \"document.title\"")]
    public void FrameAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control frames {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Theory]
    [InlineData("listWorkers", "listWorkers")]
    [InlineData("waitForWorker worker.js --timeout 1000", "waitForWorker \"worker.js\" timeout=\"1000\"")]
    [InlineData("waitForWorker worker-\\d+\\.js --match regex --ignore-case", "waitForWorker \"worker-\\d+\\.js\" match=\"regex\" ignoreCase=\"true\"")]
    [InlineData("workerEvaluate self.location.href --target worker.js", "workerEvaluate \"self.location.href\" target=\"worker.js\"")]
    [InlineData("workerIntercept /api --status 201 --body ok", "workerIntercept \"/api\" status=\"201\" body=\"ok\"")]
    [InlineData("workerIntercept /api --body-file C:\\temp\\worker.json", "workerIntercept \"/api\" bodyFile=\"C:\\\\temp\\\\worker.json\"")]
    [InlineData("workerIntercept /api/.+ --match regex --ignore-case --header \"X-Test: yes\"", "workerIntercept \"/api/.+\" header=\"X-Test: yes\" match=\"regex\" ignoreCase=\"true\"")]
    [InlineData("workerIntercept /api --header-name X-Test --header-value yes", "workerIntercept \"/api\" headerName=\"X-Test\" headerValue=\"yes\"")]
    public void WorkerAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control workers {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Theory]
    [InlineData("startCoverage --js true --css false", "startCoverage js=\"true\" css=\"false\"")]
    [InlineData("stopCoverage --path C:\\temp\\coverage.json", "stopCoverage path=\"C:\\\\temp\\\\coverage.json\"")]
    public void CoverageAliasCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control coverage {commandTail}").Invoke();

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
