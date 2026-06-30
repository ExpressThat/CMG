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
    public void RunCommand_MapsPointerVisualOptions()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "run flows --gif artifacts --pointer-theme hand --pointer-color #16a34a --pointer-size 40 --pointer-shadow light").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(PointerTheme.Hand, handler.PointerVisual?.Theme);
        Assert.Equal("#16a34a", handler.PointerVisual?.Color);
        Assert.Equal(40, handler.PointerVisual?.SizePixels);
        Assert.Equal(PointerShadow.Light, handler.PointerVisual?.Shadow);
    }

    [Fact]
    public void RunCommand_MapsShowPointerOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --show-pointer false").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(PointerVisibility.Hidden, handler.ShowPointer);
    }

    [Fact]
    public void RunCommand_MapsCaptionOptions()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "run flows --gif artifacts --caption-style teaching --caption-position right --caption-severity warning").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(CaptionStyle.Teaching, handler.CaptionOptions?.Style);
        Assert.Equal(CaptionPosition.Right, handler.CaptionOptions?.Position);
        Assert.Equal(CaptionSeverity.Warning, handler.CaptionOptions?.Severity);
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
    public void RunCommand_MapsSpecificGifHoldOptions()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "run flows --gif artifacts --pointer-pre-click-hold 100 --pointer-post-click-hold 200 --gif-hold-after-navigation 300 --gif-hold-after-assertion 400").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(100, handler.PreClickHoldMilliseconds);
        Assert.Equal(200, handler.PostClickHoldMilliseconds);
        Assert.Equal(300, handler.HoldAfterNavigationMilliseconds);
        Assert.Equal(400, handler.HoldAfterAssertionMilliseconds);
    }

    [Fact]
    public void RunCommand_MapsGifTimelineOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-timeline timelines").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("timelines", handler.GifTimelinePath);
    }

    [Fact]
    public void RunCommand_MapsGifFrameDelayOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-fps 20 --gif-frame-delay 80").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(80, handler.FrameDelayMilliseconds);
    }

    [Fact]
    public void RunCommand_MapsGifFpsOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse("run flows --gif artifacts --gif-fps 25").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(40, handler.FrameDelayMilliseconds);
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
        public PointerVisualOptions? PointerVisual { get; private set; }
        public PointerVisibility ShowPointer { get; private set; } = PointerVisibility.Auto;
        public BrowserCaptionOptions? CaptionOptions { get; private set; }
        public ClickPulseStyle ClickPulse { get; private set; }
        public int HoldAfterActionMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds;
        public int HoldOnFailureMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds;
        public int PreClickHoldMilliseconds { get; private set; }
        public int PostClickHoldMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds;
        public int HoldAfterNavigationMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds;
        public int HoldAfterAssertionMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds;
        public string? GifTimelinePath { get; private set; }
        public int FrameDelayMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultFrameDelayMilliseconds;

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
            PointerVisibility showPointer = PointerVisibility.Auto,
            BrowserCaptionOptions? captionOptions = null,
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
            PointerMotion = pointerMotion;
            PointerVisual = pointerVisual;
            ShowPointer = showPointer;
            CaptionOptions = captionOptions;
            ClickPulse = clickPulse;
            HoldAfterActionMilliseconds = holdAfterActionMilliseconds;
            HoldOnFailureMilliseconds = holdOnFailureMilliseconds;
            PreClickHoldMilliseconds = preClickHoldMilliseconds;
            PostClickHoldMilliseconds = postClickHoldMilliseconds;
            HoldAfterNavigationMilliseconds = holdAfterNavigationMilliseconds;
            HoldAfterAssertionMilliseconds = holdAfterAssertionMilliseconds;
            GifTimelinePath = gifTimelinePath;
            FrameDelayMilliseconds = frameDelayMilliseconds;
            return 0;
        }
    }
}
