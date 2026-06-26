using System.CommandLine;
using CMG.Browser;
using CMG.Commands;
using CMG.Runner;

namespace CMG.Tests;

public sealed class RunCommandBuilderTests
{
    [Fact]
    public void RunCommand_MapsTimeoutOptions()
    {
        var handler = new CapturingRunCommandHandler();
        var exitCode = BuildRoot(handler).Parse(
            "run flows --timeout 700 --navigation-timeout 800 --assertion-timeout 900").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("flows", handler.Path);
        Assert.Equal(700, handler.Timeout);
        Assert.Equal(800, handler.NavigationTimeout);
        Assert.Equal(900, handler.AssertionTimeout);
    }

    private static RootCommand BuildRoot(CapturingRunCommandHandler handler)
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var root = new RootCommand();
        root.Options.Add(chrome);
        root.Options.Add(edge);
        root.Options.Add(firefox);
        root.Subcommands.Add(new RunCommandBuilder(handler).Build(new BrowserSelectionOptions(chrome, edge, firefox)));
        return root;
    }

    private sealed class CapturingRunCommandHandler : ICmgRunCommandHandler
    {
        public string? Path { get; private set; }

        public int? Timeout { get; private set; }

        public int? NavigationTimeout { get; private set; }

        public int? AssertionTimeout { get; private set; }

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
            int? assertionTimeout)
        {
            Path = path;
            Timeout = timeout;
            NavigationTimeout = navigationTimeout;
            AssertionTimeout = assertionTimeout;
            return 0;
        }
    }
}
