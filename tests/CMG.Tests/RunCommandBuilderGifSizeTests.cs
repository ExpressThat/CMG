using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using CMG.Runner;

namespace CMG.Tests;

public sealed class RunCommandBuilderGifSizeTests
{
    [Fact]
    public void RunCommand_MapsGifWarnSize()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-warn-size 2MB").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(2 * 1024 * 1024, handler.GifWarnSizeBytes);
    }

    [Fact]
    public void RunCommand_RejectsInvalidGifWarnSize()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-warn-size huge").Invoke();

        Assert.Equal(1, exitCode);
        Assert.Null(handler.Path);
    }

    [Fact]
    public void RunCommand_MapsGifMaxSize()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-max-size 500KB").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(500 * 1024, handler.GifMaxSizeBytes);
    }

    [Fact]
    public void RunCommand_RejectsInvalidGifMaxSize()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-max-size huge").Invoke();

        Assert.Equal(1, exitCode);
        Assert.Null(handler.Path);
    }

    [Fact]
    public void RunCommand_MapsGifMaxDuration()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-max-duration 2s").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(2000, handler.GifMaxDurationMilliseconds);
    }

    [Fact]
    public void RunCommand_RejectsInvalidGifMaxDuration()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-max-duration forever").Invoke();

        Assert.Equal(1, exitCode);
        Assert.Null(handler.Path);
    }

    private static RootCommand BuildRoot(CapturingHandler handler)
    {
        var chrome = new Option<bool>("--chrome");
        var root = new RootCommand();
        root.Options.Add(chrome);
        root.Subcommands.Add(new RunCommandBuilder(handler).Build(new BrowserSelectionOptions(chrome, new Option<bool>("--edge"), new Option<bool>("--firefox"))));
        return root;
    }

    private sealed class CapturingHandler : ICmgRunCommandHandler
    {
        public string? Path { get; private set; }
        public long? GifWarnSizeBytes { get; private set; }
        public long? GifMaxSizeBytes { get; private set; }
        public int? GifMaxDurationMilliseconds { get; private set; }

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
            ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
            int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
            string? gifTimelinePath = null,
            int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
            long? gifWarnSizeBytes = null,
            long? gifMaxSizeBytes = null,
            int? gifMaxDurationMilliseconds = null)
        {
            Path = path;
            GifWarnSizeBytes = gifWarnSizeBytes;
            GifMaxSizeBytes = gifMaxSizeBytes;
            GifMaxDurationMilliseconds = gifMaxDurationMilliseconds;
            return 0;
        }
    }
}
