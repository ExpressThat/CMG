using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using CMG.Runner;

namespace CMG.Tests;

public sealed class RunCommandBuilderGifRetentionTests
{
    [Fact]
    public void RunCommand_MapsGifRetentionOptions()
    {
        var handler = new Handler();

        var exitCode = Root(handler).Parse(
            "run flows --gif artifacts --gif-on-retry --gif-sample-rate 7 --gif-clean-passed").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(CmgGifRetentionMode.OnRetry, handler.Mode);
        Assert.Equal(7, handler.SampleRate);
        Assert.True(handler.CleanPassed);
    }

    [Theory]
    [InlineData("--gif-on-failure --gif-on-retry")]
    [InlineData("--gif-retention always --gif-on-retry")]
    [InlineData("--gif-retention sometimes")]
    [InlineData("--gif-retention days:0")]
    [InlineData("--gif-sample-rate 0")]
    public void RunCommand_RejectsInvalidGifRetentionOptions(string options)
    {
        var handler = new Handler();

        var exitCode = Root(handler).Parse($"run flows {options}").Invoke();

        Assert.Equal(1, exitCode);
        Assert.False(handler.Called);
    }

    [Fact]
    public void RunCommand_MapsGifRetentionConfigAndCliOverride()
    {
        using var directory = new TempDirectory();
        var config = directory.Write("""
        { "gifRetention": "onFailure", "gifSampleRate": 5, "gifCleanPassed": true }
        """);
        var handler = new Handler();

        var exitCode = Root(handler).Parse(
            $"run flows --config \"{config}\" --gif-on-retry --gif-sample-rate 2").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(CmgGifRetentionMode.OnRetry, handler.Mode);
        Assert.Equal(2, handler.SampleRate);
        Assert.True(handler.CleanPassed);
    }

    [Fact]
    public void RunCommand_DaysRetentionDeletesOnlyExpiredGifFamilies()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var expired = Path.Combine(directory.FullName, "expired.gif");
            var current = Path.Combine(directory.FullName, "current.gif");
            File.WriteAllText(expired, "gif");
            File.WriteAllText(Path.ChangeExtension(expired, ".timeline.json"), "{}");
            File.WriteAllText(Path.ChangeExtension(expired, ".narration.txt"), "narration");
            File.WriteAllText(current, "gif");
            File.SetLastWriteTimeUtc(expired, DateTime.UtcNow.AddDays(-8));
            var handler = new Handler();

            var exitCode = Root(handler).Parse(
                $"run flows --gif \"{directory.FullName}\" --gif-retention days:7").Invoke();

            Assert.Equal(0, exitCode);
            Assert.True(handler.Called);
            Assert.False(File.Exists(expired));
            Assert.False(File.Exists(Path.ChangeExtension(expired, ".timeline.json")));
            Assert.False(File.Exists(Path.ChangeExtension(expired, ".narration.txt")));
            Assert.True(File.Exists(current));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void RunCommand_MapsGifBudgetFallbackOptions()
    {
        var handler = new Handler();

        var exitCode = Root(handler).Parse(
            "run flows --gif artifacts --gif-budget 750KB --no-gif-budget-downscale").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(750 * 1024, handler.Encoding?.SizeBudget?.Bytes);
        Assert.True(handler.Encoding?.SizeBudget?.QualityFallback);
        Assert.False(handler.Encoding?.SizeBudget?.DownscaleFallback);
    }

    [Fact]
    public void RunCommand_MapsGifReviewOptions()
    {
        var handler = new Handler();

        var exitCode = Root(handler).Parse(
            "run flows --gif artifacts --gif-narration true --gif-still-pdf true --gif-alt-text \"{name}: {outcome}\" --gif-description \"Release evidence\"").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("auto", handler.Encoding?.Review?.NarrationSidecar);
        Assert.Equal("auto", handler.Encoding?.Review?.StillPdf);
        Assert.Equal("{name}: {outcome}", handler.Encoding?.Review?.AltText);
        Assert.Equal("Release evidence", handler.Encoding?.Review?.Description);
    }

    private static RootCommand Root(Handler handler)
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var root = new RootCommand();
        root.Options.Add(chrome); root.Options.Add(edge); root.Options.Add(firefox);
        root.Subcommands.Add(new RunCommandBuilder(handler).Build(new(chrome, edge, firefox)));
        return root;
    }

    private sealed class Handler : ICmgRunCommandHandler
    {
        public bool Called { get; private set; }
        public CmgGifRetentionMode Mode { get; private set; }
        public int SampleRate { get; private set; }
        public bool CleanPassed { get; private set; }
        public GifEncodingOptions? Encoding { get; private set; }

        public int Run(
            BrowserKind browserKind, string path, DirectoryInfo? artifacts, FileInfo? jsonReport, FileInfo? htmlReport,
            FileInfo? junitReport, DirectoryInfo? traceDirectory, string? grep, string? tag, int retries, int maxFailures,
            int repeatEach, bool listOnly, string? shard, int? timeout, int? navigationTimeout, int? assertionTimeout,
            string? baseUrl, IReadOnlyDictionary<string, string> variables, string projectName = "", int? browserPort = null,
            bool autoLaunch = false, bool autoLaunchHeadless = false, GifQuality gifQuality = GifQuality.Highest,
            ScriptPointerMotionOptions? pointerMotion = null, PointerVisualOptions? pointerVisual = null,
            PointerVisibility showPointer = PointerVisibility.Auto, BrowserCaptionOptions? captionOptions = null,
            ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
            int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
            int preClickHoldMilliseconds = 0,
            int postClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            int holdAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            int holdAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            string? gifTimelinePath = null, int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
            long? gifWarnSizeBytes = null, long? gifMaxSizeBytes = null, int? gifMaxDurationMilliseconds = null) => 0;

        public int RunWithGifRetention(
            BrowserKind browserKind, string path, DirectoryInfo? artifacts, FileInfo? jsonReport, FileInfo? htmlReport,
            FileInfo? junitReport, DirectoryInfo? traceDirectory, string? grep, string? tag, int retries, int maxFailures,
            int repeatEach, bool listOnly, string? shard, int? timeout, int? navigationTimeout, int? assertionTimeout,
            string? baseUrl, IReadOnlyDictionary<string, string> variables, string projectName, int? browserPort,
            bool autoLaunch, bool autoLaunchHeadless, GifQuality gifQuality, ScriptPointerMotionOptions? pointerMotion,
            PointerVisualOptions? pointerVisual, PointerVisibility showPointer, BrowserCaptionOptions? captionOptions,
            ClickPulseStyle clickPulse, int holdAfterActionMilliseconds, int holdOnFailureMilliseconds,
            int preClickHoldMilliseconds, int postClickHoldMilliseconds, int holdAfterNavigationMilliseconds,
            int holdAfterAssertionMilliseconds, string? gifTimelinePath, int frameDelayMilliseconds,
            long? gifWarnSizeBytes, long? gifMaxSizeBytes, int? gifMaxDurationMilliseconds, GifEncodingOptions? gifEncoding,
            int? browserIdleTimeoutMilliseconds, bool noBrowserIdleCleanup, CmgGifRetentionMode gifRetentionMode,
            int gifRetentionSampleRate, bool gifCleanPassed)
        {
            Called = true;
            Mode = gifRetentionMode;
            SampleRate = gifRetentionSampleRate;
            CleanPassed = gifCleanPassed;
            Encoding = gifEncoding;
            return 0;
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        public string Write(string content)
        {
            Directory.CreateDirectory(root);
            var path = Path.Combine(root, "cmg.run.json");
            File.WriteAllText(path, content);
            return path;
        }
        public void Dispose() { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
