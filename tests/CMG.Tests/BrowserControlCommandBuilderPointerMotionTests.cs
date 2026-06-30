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
        public ClickPulseStyle ClickPulse { get; private set; }
        public int HoldAfterActionMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds;
        public int HoldOnFailureMilliseconds { get; private set; } = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds;
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
            ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
            int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds)
        {
            File = file;
            PointerMotion = pointerMotion;
            ClickPulse = clickPulse;
            HoldAfterActionMilliseconds = holdAfterActionMilliseconds;
            HoldOnFailureMilliseconds = holdOnFailureMilliseconds;
            return 0;
        }
    }
}
