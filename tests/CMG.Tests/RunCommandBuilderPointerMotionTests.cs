using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using CMG.Runner;

namespace CMG.Tests;

public sealed class RunCommandBuilderPointerMotionTests
{
    [Fact]
    public void RunCommand_MapsPointerMotionOptions()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "run flows --gif artifacts --pointer-duration 600 --pointer-speed slow --pointer-easing spring").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(600, handler.PointerMotion?.PointerDurationMilliseconds);
        Assert.Equal("slow", handler.PointerMotion?.PointerSpeed);
        Assert.Equal(ScriptPointerEasing.Spring, handler.PointerMotion?.PointerEasing);
    }

    [Fact]
    public void RunCommand_MapsClickPulseOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --click-pulse dot").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(ClickPulseStyle.Dot, handler.ClickPulse);
    }

    [Fact]
    public void RunCommand_MapsGifHoldAfterActionOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-hold-after-action 750").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(750, handler.HoldAfterActionMilliseconds);
    }

    [Fact]
    public void RunCommand_MapsGifHoldOnFailureOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-hold-on-failure 1800").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(1800, handler.HoldOnFailureMilliseconds);
    }

    [Fact]
    public void RunCommand_MapsGifTimelineOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-timeline timelines").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("timelines", handler.GifTimelinePath);
    }

    private static RootCommand BuildRoot(CapturingHandler handler)
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

    private sealed class CapturingHandler : ICmgRunCommandHandler
    {
        public ScriptPointerMotionOptions? PointerMotion { get; private set; }
        public ClickPulseStyle ClickPulse { get; private set; }
        public int HoldAfterActionMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds;
        public int HoldOnFailureMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds;
        public string? GifTimelinePath { get; private set; }

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
            long? gifWarnSizeBytes = null,
            long? gifMaxSizeBytes = null,
            int? gifMaxDurationMilliseconds = null)
        {
            PointerMotion = pointerMotion;
            ClickPulse = clickPulse;
            HoldAfterActionMilliseconds = holdAfterActionMilliseconds;
            HoldOnFailureMilliseconds = holdOnFailureMilliseconds;
            GifTimelinePath = gifTimelinePath;
            return 0;
        }
    }
}
