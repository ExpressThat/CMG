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
            "run flows --timeout 700 --navigation-timeout 800 --assertion-timeout 900 --base-url https://example.test/app/ --var user=Ada --env mode=demo").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("flows", handler.Path);
        Assert.Equal(700, handler.Timeout);
        Assert.Equal(800, handler.NavigationTimeout);
        Assert.Equal(900, handler.AssertionTimeout);
        Assert.Equal("https://example.test/app/", handler.BaseUrl);
        Assert.Equal("Ada", handler.Variables["user"]);
        Assert.Equal("demo", handler.Variables["mode"]);
    }

    [Fact]
    public void RunCommand_RejectsMalformedVariable()
    {
        var handler = new CapturingRunCommandHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --env broken").Invoke();

        Assert.Equal(1, exitCode);
        Assert.Null(handler.Path);
    }

    [Fact]
    public void RunCommand_MergesConfigWithCliOverrides()
    {
        using var directory = new TempRunDirectory();
        var config = directory.Write("cmg.run.json", """
        {
          "gif": "artifacts/gifs",
          "reportJson": "artifacts/report.json",
          "trace": "artifacts/traces",
          "grep": "checkout",
          "tag": "smoke",
          "retries": 1,
          "maxFailures": 2,
          "repeatEach": 3,
          "shard": "1/2",
          "timeout": 1000,
          "navigationTimeout": 2000,
          "assertionTimeout": 3000,
          "baseUrl": "https://config.test/app/",
          "variables": { "tenant": "demo", "mode": "config" }
        }
        """);
        var handler = new CapturingRunCommandHandler();
        var exitCode = BuildRoot(handler).Parse(
            $"run flows --config \"{config}\" --retries 4 --base-url https://cli.test/ --var mode=cli").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(Path.Combine(directory.Root, "artifacts", "gifs"), handler.Artifacts?.FullName);
        Assert.Equal(Path.Combine(directory.Root, "artifacts", "report.json"), handler.JsonReport?.FullName);
        Assert.Equal(Path.Combine(directory.Root, "artifacts", "traces"), handler.TraceDirectory?.FullName);
        Assert.Equal("checkout", handler.Grep);
        Assert.Equal("smoke", handler.Tag);
        Assert.Equal(4, handler.Retries);
        Assert.Equal(2, handler.MaxFailures);
        Assert.Equal(3, handler.RepeatEach);
        Assert.Equal("1/2", handler.Shard);
        Assert.Equal(1000, handler.Timeout);
        Assert.Equal(2000, handler.NavigationTimeout);
        Assert.Equal(3000, handler.AssertionTimeout);
        Assert.Equal("https://cli.test/", handler.BaseUrl);
        Assert.Equal("demo", handler.Variables["tenant"]);
        Assert.Equal("cli", handler.Variables["mode"]);
    }

    [Fact]
    public void RunCommand_RejectsInvalidConfig()
    {
        using var directory = new TempRunDirectory();
        var config = directory.Write("cmg.run.json", "{ nope");
        var handler = new CapturingRunCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"run flows --config \"{config}\"").Invoke();

        Assert.Equal(1, exitCode);
        Assert.Null(handler.Path);
    }

    [Fact]
    public void RunCommand_RejectsInvalidConfigFieldType()
    {
        using var directory = new TempRunDirectory();
        var config = directory.Write("cmg.run.json", """{ "retries": "many" }""");
        var handler = new CapturingRunCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"run flows --config \"{config}\"").Invoke();

        Assert.Equal(1, exitCode);
        Assert.Null(handler.Path);
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

        public string? BaseUrl { get; private set; }

        public IReadOnlyDictionary<string, string> Variables { get; private set; } =
            new Dictionary<string, string>();

        public DirectoryInfo? Artifacts { get; private set; }

        public FileInfo? JsonReport { get; private set; }

        public DirectoryInfo? TraceDirectory { get; private set; }

        public string? Grep { get; private set; }

        public string? Tag { get; private set; }

        public int Retries { get; private set; }

        public int MaxFailures { get; private set; }

        public int RepeatEach { get; private set; }

        public string? Shard { get; private set; }

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
            IReadOnlyDictionary<string, string> variables)
        {
            Path = path;
            Artifacts = artifacts;
            JsonReport = jsonReport;
            TraceDirectory = traceDirectory;
            Grep = grep;
            Tag = tag;
            Retries = retries;
            MaxFailures = maxFailures;
            RepeatEach = repeatEach;
            Shard = shard;
            Timeout = timeout;
            NavigationTimeout = navigationTimeout;
            AssertionTimeout = assertionTimeout;
            BaseUrl = baseUrl;
            Variables = variables;
            return 0;
        }
    }

    private sealed class TempRunDirectory : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public string Write(string relativePath, string content)
        {
            var fullPath = Path.Combine(Root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, content);
            return fullPath;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
