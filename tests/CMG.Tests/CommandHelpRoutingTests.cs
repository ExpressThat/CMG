using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CommandHelpRoutingTests
{
    [Fact]
    public void DocumentedLeafCommandHelp_DoesNotInvokeHandlers()
    {
        var root = BuildRoot();
        foreach (var command in DocumentedLeafCommands())
        {
            var exitCode = root.Parse([.. command, "--help"]).Invoke();
            Assert.Equal(0, exitCode);
        }
    }

    private static RootCommand BuildRoot()
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var browserOptions = new BrowserSelectionOptions(chrome, edge, firefox);
        var root = new RootCommand();
        root.Options.Add(chrome);
        root.Options.Add(edge);
        root.Options.Add(firefox);
        var control = new BrowserControlCommandBuilder(new ThrowingBrowserControlCommandHandler());
        root.Subcommands.Add(new BrowserCommandBuilder(new ThrowingBrowserCommandHandler(), control).Build(browserOptions));
        root.Subcommands.Add(new RunCommandBuilder(new ThrowingRunCommandHandler()).Build(browserOptions));
        root.Subcommands.Add(new ApiCommandBuilder(new CmgApiRequestRunner()).Build());
        root.Subcommands.Add(new FilesCommandBuilder().Build());
        return root;
    }

    private static IEnumerable<string[]> DocumentedLeafCommands()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var commandDocs = Path.Combine(root, "docs", "commands");
        foreach (var file in Directory.EnumerateFiles(commandDocs, "*.md", SearchOption.AllDirectories))
        {
            if (!Path.GetFileName(file).Equals("index.md", StringComparison.OrdinalIgnoreCase))
            {
                yield return Path.GetRelativePath(commandDocs, Path.ChangeExtension(file, null))
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }
    }

    private sealed class ThrowingBrowserCommandHandler : IBrowserCommandHandler
    {
        public int Launch(BrowserKind kind, IReadOnlyList<string> args, bool headless, string? url) => throw new InvalidOperationException();
        public int LaunchApp(BrowserKind kind, FileInfo file, string app, BrowserAppDebugOptions options, IReadOnlyList<string> args) => throw new InvalidOperationException();
        public int AttachApp(BrowserKind kind, BrowserAppDebugOptions options, int processId) => throw new InvalidOperationException();
        public int Close(BrowserKind kind, IReadOnlyList<string> args) => throw new InvalidOperationException();
    }

    private sealed class ThrowingBrowserControlCommandHandler : IBrowserControlCommandHandler
    {
        public int GetElement(BrowserKind kind, string selector, bool html, bool screenshot, FileInfo? output) => throw new InvalidOperationException();
        public int RunScript(BrowserKind kind, string file, FileInfo? gif) => throw new InvalidOperationException();
        public int RunScriptAction(BrowserKind kind, string line) => throw new InvalidOperationException();
        public int ValidateScript(string file) => throw new InvalidOperationException();
    }

    private sealed class ThrowingRunCommandHandler : ICmgRunCommandHandler
    {
        public int Run(
            BrowserKind browserKind,
            string path,
            DirectoryInfo? artifacts,
            FileInfo? jsonReport,
            FileInfo? htmlReport,
            FileInfo? junitReport,
            DirectoryInfo? traceDirectory,
            string? grep,
            string? tag,
            int retries,
            int maxFailures,
            int repeatEach,
            bool listOnly,
            string? shard,
            int? timeout,
            int? navigationTimeout,
            int? assertionTimeout,
            string? baseUrl,
            IReadOnlyDictionary<string, string> variables,
            string projectName = "",
            int? browserPort = null,
            bool autoLaunch = false,
            bool autoLaunchHeadless = false,
            GifQuality gifQuality = GifQuality.Highest,
            ScriptPointerMotionOptions? pointerMotion = null) => throw new InvalidOperationException();
    }
}
