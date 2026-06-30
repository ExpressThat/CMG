using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderPointerVisualTests
{
    [Fact]
    public void BeforeAction_UsesRecordingPointerVisualDefault()
    {
        var client = new FakeAutomationClient();
        using var recorder = new ScriptGifRecorder(
            client,
            Options(new PointerVisualOptions(PointerTheme.Ring, "#dc2626", 44, PointerShadow.Strong)));
        recorder.Start("debug");

        recorder.BeforeAction(Action("click", "#target"));

        Assert.Contains(client.CursorStates, state =>
            state.Visual?.Theme is PointerTheme.Ring &&
            state.Visual.Color == "#dc2626" &&
            state.Visual.SizePixels == 44 &&
            state.Visual.Shadow is PointerShadow.Strong);
    }

    [Fact]
    public void BeforeAction_ActionPointerVisualOverridesRecordingDefault()
    {
        var client = new FakeAutomationClient();
        using var recorder = new ScriptGifRecorder(
            client,
            Options(new PointerVisualOptions(PointerTheme.Ring, "#dc2626", 44, PointerShadow.Strong)));
        recorder.Start("debug");

        recorder.BeforeAction(Action("click", "#target", new()
        {
            ["pointerTheme"] = "dot",
            ["pointerColor"] = "#16a34a",
            ["pointerSize"] = "30",
            ["pointerShadow"] = "none"
        }));

        Assert.Contains(client.CursorStates, state =>
            state.Visual?.Theme is PointerTheme.Dot &&
            state.Visual.Color == "#16a34a" &&
            state.Visual.SizePixels == 30 &&
            state.Visual.Shadow is PointerShadow.None);
    }

    [Fact]
    public void BeforeAction_TapUsesTouchOnlyWhenThemeIsDefaultArrow()
    {
        var client = new FakeAutomationClient();
        using var recorder = new ScriptGifRecorder(
            client,
            Options(new PointerVisualOptions(PointerTheme.Hand, "#2563eb", 36, PointerShadow.Light)));
        recorder.Start("debug");

        recorder.BeforeAction(Action("tap", "#target"));

        Assert.Contains(client.CursorStates, state => state.Visual?.Theme is PointerTheme.Hand);
        Assert.DoesNotContain(client.CursorStates, state => state.Visual?.Theme is PointerTheme.Touch);
    }

    private static ScriptRecordingOptions Options(PointerVisualOptions visual) =>
        new(
            Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"),
            PointerMotion: new ScriptPointerMotionOptions(PointerDurationMilliseconds: 0),
            PointerVisual: visual);

    private static BrowserScriptAction Action(
        string name,
        string selector,
        Dictionary<string, string>? options = null) =>
        new(1, name, name, [selector], options ?? [], []);
}
