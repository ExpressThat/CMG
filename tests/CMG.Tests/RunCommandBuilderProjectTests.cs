using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using CMG.Runner;

namespace CMG.Tests;

public sealed class RunCommandBuilderProjectTests
{
    [Fact]
    public void RunCommand_AppliesNamedProjectConfig()
    {
        using var directory = new TempRunDirectory();
        var config = directory.Write("cmg.run.json", """
        {
          "baseUrl": "https://global.test/",
          "variables": { "tenant": "global" },
          "projects": [
            {
              "name": "firefox-smoke",
              "browser": "firefox",
              "baseUrl": "https://firefox.test/",
              "tag": "smoke",
              "retries": 2,
              "variables": { "tenant": "firefox" }
            }
          ]
        }
        """);
        var handler = new CapturingRunCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"run flows --config \"{config}\" --project firefox-smoke").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Firefox, handler.BrowserKind);
        Assert.Equal("firefox-smoke", handler.ProjectName);
        Assert.Equal("https://firefox.test/", handler.BaseUrl);
        Assert.Equal("smoke", handler.Tag);
        Assert.Equal(2, handler.Retries);
        Assert.Equal("firefox", handler.Variables["tenant"]);
    }

    [Fact]
    public void RunCommand_RejectsMissingProject()
    {
        using var directory = new TempRunDirectory();
        var config = directory.Write("cmg.run.json", """{ "projects": [] }""");
        var handler = new CapturingRunCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"run flows --config \"{config}\" --project missing").Invoke();

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
        public BrowserKind BrowserKind { get; private set; }
        public string? Tag { get; private set; }
        public int Retries { get; private set; }
        public string? BaseUrl { get; private set; }
        public string ProjectName { get; private set; } = string.Empty;
        public IReadOnlyDictionary<string, string> Variables { get; private set; } =
            new Dictionary<string, string>();

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
            ScriptPointerMotionOptions? pointerMotion = null,
            PointerVisualOptions? pointerVisual = null,
            ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
            int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
            int preClickHoldMilliseconds = 0,
            int postClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            int holdAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            int holdAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            string? gifTimelinePath = null,
            int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
            long? gifWarnSizeBytes = null,
            long? gifMaxSizeBytes = null,
            int? gifMaxDurationMilliseconds = null)
        {
            BrowserKind = browserKind;
            Path = path;
            Tag = tag;
            Retries = retries;
            BaseUrl = baseUrl;
            Variables = variables;
            ProjectName = projectName;
            return 0;
        }
    }

    private sealed class TempRunDirectory : IDisposable
    {
        public string Root { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public string Write(string relativePath, string content)
        {
            var fullPath = System.IO.Path.Combine(Root, relativePath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)!);
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
