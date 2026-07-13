using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderPointerMotionTests
{
    [Fact]
    public void ScriptCommand_MapsPointerMotionOptions()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --pointer-duration 450 --pointer-speed 1.5x --pointer-easing linear").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(450, handler.PointerMotion?.PointerDurationMilliseconds);
        Assert.Equal("1.5x", handler.PointerMotion?.PointerSpeed);
        Assert.Equal(ScriptPointerEasing.Linear, handler.PointerMotion?.PointerEasing);
    }

    [Fact]
    public void ScriptCommand_MapsClickPulseOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --click-pulse crosshair").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(ClickPulseStyle.Crosshair, handler.ClickPulse);
    }

    [Fact]
    public void ScriptCommand_MapsPointerVisualOptions()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --pointer-theme ring --pointer-color #dc2626 --pointer-size 44 --pointer-shadow strong").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(PointerTheme.Ring, handler.PointerVisual?.Theme);
        Assert.Equal("#dc2626", handler.PointerVisual?.Color);
        Assert.Equal(44, handler.PointerVisual?.SizePixels);
        Assert.Equal(PointerShadow.Strong, handler.PointerVisual?.Shadow);
    }

    [Fact]
    public void ScriptCommand_MapsAccessibleGifPresetsAndExplicitOverrides()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --gif-reduced-motion --pointer-duration 120 --gif-high-contrast-pointer --pointer-size 48").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(120, handler.PointerMotion?.PointerDurationMilliseconds);
        Assert.Equal(ScriptPointerEasing.Linear, handler.PointerMotion?.PointerEasing);
        Assert.Equal(PointerTheme.Ring, handler.PointerVisual?.Theme);
        Assert.Equal("#ffea00", handler.PointerVisual?.Color);
        Assert.Equal(48, handler.PointerVisual?.SizePixels);
    }

    [Fact]
    public void ScriptCommand_MapsShowPointerOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --show-pointer false").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(PointerVisibility.Hidden, handler.ShowPointer);
    }

    [Fact]
    public void ScriptCommand_MapsCaptionOptions()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --caption-style qa --caption-position bottom --caption-severity success --caption-size large --auto-captions --caption-template \"{step}: {action}\"").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(CaptionStyle.Qa, handler.CaptionOptions?.Style);
        Assert.Equal(CaptionPosition.Bottom, handler.CaptionOptions?.Position);
        Assert.Equal(CaptionSeverity.Success, handler.CaptionOptions?.Severity);
        Assert.Equal(CaptionSize.Large, handler.CaptionOptions?.Size);
        Assert.True(handler.CaptionOptions?.AutoCaptions);
        Assert.Equal("{step}: {action}", handler.CaptionOptions?.CaptionTemplate);
    }

    [Fact]
    public void ScriptCommand_MapsGifHoldAfterActionOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --gif-hold-after-action 900").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(900, handler.HoldAfterActionMilliseconds);
    }

    [Fact]
    public void ScriptCommand_MapsGifHoldOnFailureOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --gif-hold-on-failure 1800").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(1800, handler.HoldOnFailureMilliseconds);
    }

    [Fact]
    public void ScriptCommand_MapsSpecificGifHoldOptions()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --pointer-pre-click-hold 100 --pointer-post-click-hold 200 --gif-hold-after-navigation 300 --gif-hold-after-assertion 400").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(100, handler.PreClickHoldMilliseconds);
        Assert.Equal(200, handler.PostClickHoldMilliseconds);
        Assert.Equal(300, handler.HoldAfterNavigationMilliseconds);
        Assert.Equal(400, handler.HoldAfterAssertionMilliseconds);
    }

    [Fact]
    public void ScriptCommand_MapsGifTimelineOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --gif-timeline C:\\temp\\flow.timeline.json").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("C:\\temp\\flow.timeline.json", handler.GifTimelinePath);
    }

    [Fact]
    public void ScriptCommand_MapsGifFrameDelayOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --gif-fps 20 --gif-frame-delay 80").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(80, handler.FrameDelayMilliseconds);
    }

    [Fact]
    public void ScriptCommand_MapsGifFpsOption()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --gif-fps 20").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(50, handler.FrameDelayMilliseconds);
    }

    [Fact]
    public void ScriptCommand_RejectsInvalidPointerEasing()
    {
        var handler = new CapturingHandler();
        var exitCode = BuildRoot(handler).Parse(
            "control script --file flow.cmgscript --gif C:\\temp\\flow.gif --pointer-easing wobbly").Invoke();

        Assert.Equal(1, exitCode);
        Assert.Null(handler.File);
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
        root.Subcommands.Add(new BrowserControlCommandBuilder(handler).Build(new BrowserSelectionOptions(chrome, edge, firefox)));
        return root;
    }

    private sealed class CapturingHandler : IBrowserControlCommandHandler
    {
        public string? File { get; private set; }
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
        public int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output) => 0;
        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif) => 0;
        public int RunScriptAction(BrowserKind browserKind, string scriptLine) => 0;
        public int ValidateScript(string file) => 0;

        public int RunScript(
            BrowserKind browserKind,
            int? port,
            string file,
            FileInfo? gif,
            FileInfo? trace,
            ScriptTimeoutOptions? timeouts,
            string? baseUrl,
            IReadOnlyDictionary<string, string> variables,
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
            int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds)
        {
            File = file;
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
