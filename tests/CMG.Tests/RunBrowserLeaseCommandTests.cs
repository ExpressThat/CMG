using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using CMG.Runner;

namespace CMG.Tests;

public sealed class RunBrowserLeaseCommandTests
{
    [Fact]
    public void Run_MapsBrowserIdleLeaseOptions()
    {
        var handler = new Handler();

        var exit = Root(handler).Parse("run flows --auto-launch --headless --browser-idle-timeout 90m").Invoke();

        Assert.Equal(0, exit);
        Assert.Equal(5_400_000, handler.IdleTimeout);
        Assert.False(handler.Disable);
    }

    [Fact]
    public void Run_ReadsConservativeLeaseConfig()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """{ "browserIdleTimeout": "2h", "noBrowserIdleCleanup": true }""");
        try
        {
            var handler = new Handler();
            var exit = Root(handler).Parse($"run flows --config \"{path}\"").Invoke();

            Assert.Equal(0, exit);
            Assert.Equal(7_200_000, handler.IdleTimeout);
            Assert.True(handler.Disable);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("{ \"noBrowserIdleCleanup\": true }", "--browser-idle-timeout 45m", 2700000, false)]
    [InlineData("{ \"browserIdleTimeout\": \"45m\" }", "--no-browser-idle-cleanup", null, true)]
    public void ExplicitCliLeaseControl_OverridesOppositeConfig(
        string json,
        string arguments,
        int? expectedTimeout,
        bool expectedDisable)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        try
        {
            var handler = new Handler();
            Assert.Equal(0, Root(handler).Parse($"run flows --config \"{path}\" {arguments}").Invoke());
            Assert.Equal(expectedTimeout, handler.IdleTimeout);
            Assert.Equal(expectedDisable, handler.Disable);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static RootCommand Root(Handler handler)
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var root = new RootCommand();
        root.Options.Add(chrome); root.Options.Add(edge); root.Options.Add(firefox);
        root.Subcommands.Add(new RunCommandBuilder(handler).Build(new BrowserSelectionOptions(chrome, edge, firefox)));
        return root;
    }

    private sealed class Handler : ICmgRunCommandHandler
    {
        public int? IdleTimeout { get; private set; }
        public bool Disable { get; private set; }

        public int Run(
            BrowserKind browserKind, string path, DirectoryInfo? artifacts, FileInfo? json, FileInfo? html,
            FileInfo? junit, DirectoryInfo? trace, string? grep, string? tag, int retries, int maxFailures,
            int repeatEach, bool listOnly, string? shard, int? timeout, int? navigationTimeout, int? assertionTimeout,
            string? baseUrl, IReadOnlyDictionary<string, string> variables, string projectName = "", int? browserPort = null,
            bool autoLaunch = false, bool autoLaunchHeadless = false, GifQuality gifQuality = GifQuality.Highest,
            ScriptPointerMotionOptions? motion = null, PointerVisualOptions? visual = null,
            PointerVisibility showPointer = PointerVisibility.Auto, BrowserCaptionOptions? captions = null,
            ClickPulseStyle pulse = ClickPulseStyle.Ring, int hold = 350, int failureHold = 1200, int preHold = 0,
            int postHold = 350, int navigationHold = 350, int assertionHold = 350,
            string? timeline = null, int frameDelay = 100, long? warnSize = null, long? maxSize = null,
            int? maxDuration = null) => 0;

        public int Run(
            BrowserKind browserKind, string path, DirectoryInfo? artifacts, FileInfo? json, FileInfo? html,
            FileInfo? junit, DirectoryInfo? trace, string? grep, string? tag, int retries, int maxFailures,
            int repeatEach, bool listOnly, string? shard, int? timeout, int? navigationTimeout, int? assertionTimeout,
            string? baseUrl, IReadOnlyDictionary<string, string> variables, string projectName, int? browserPort,
            bool autoLaunch, bool autoLaunchHeadless, GifQuality quality, ScriptPointerMotionOptions? motion,
            PointerVisualOptions? visual, PointerVisibility showPointer, BrowserCaptionOptions? captions,
            ClickPulseStyle pulse, int hold, int failureHold, int preHold, int postHold, int navigationHold,
            int assertionHold, string? timeline, int frameDelay, long? warnSize, long? maxSize, int? maxDuration,
            GifEncodingOptions? encoding, int? browserIdleTimeoutMilliseconds = null, bool noBrowserIdleCleanup = false)
        {
            IdleTimeout = browserIdleTimeoutMilliseconds;
            Disable = noBrowserIdleCleanup;
            return 0;
        }
    }
}
