using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderPointerTargetTests
{
    [Fact]
    public void BeforeAction_UsesElementOffsetForPointerTarget()
    {
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(10, 20, 100, 40));
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, "hover", "hover", ["#save"], new Dictionary<string, string>
        {
            ["x"] = "4",
            ["y"] = "8"
        }, []));

        Assert.Equal(new ElementPoint(14, 28), client.LastMouseMove);
    }

    [Fact]
    public void BeforeAction_UsesPointerDurationForFrameCount()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, "hover", "hover", ["#save"], new Dictionary<string, string>
        {
            ["pointerDuration"] = "300"
        }, []));

        Assert.Equal(3, client.MouseMoveCount);
        Assert.Equal(3, client.PageScreenshotCount);
    }

    [Fact]
    public void BeforeAction_UsesGifBlockPointerDefaults()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var motion = new ScriptPointerMotionOptions(PointerDurationMilliseconds: 200);
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path, PointerMotion: motion));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, "hover", "hover", ["#save"], new Dictionary<string, string>(), []));

        Assert.Equal(2, client.MouseMoveCount);
    }

    [Fact]
    public void BeforeAction_HiddenPointerSuppressesDomCursor()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path, ShowPointer: PointerVisibility.Hidden));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, "hover", "hover", ["#save"], new Dictionary<string, string>(), []));

        Assert.Empty(client.CursorStates);
        Assert.True(client.RemoveDomCursorCalled);
        Assert.Equal(ScriptRecordingOptions.MovementFrameCount, client.PageScreenshotCount);
    }

    [Fact]
    public void BeforeAction_ActionShowPointerOverridesHiddenDefault()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path, ShowPointer: PointerVisibility.Hidden));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, "hover", "hover", ["#save"], new Dictionary<string, string>
        {
            ["showPointer"] = "true"
        }, []));

        Assert.NotEmpty(client.CursorStates);
    }

    [Fact]
    public void MoveMouse_HoldAfterMoveCapturesSettleFrame()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var motion = new ScriptPointerMotionOptions(PointerDurationMilliseconds: 0);
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path, PointerMotion: motion));
        recorder.Start("debug");

        recorder.MoveMouse(new BrowserScriptAction(1, "moveMouse", "moveMouse", ["center"], new Dictionary<string, string>
        {
            ["holdAfterMove"] = "500"
        }, []), dragging: false);

        Assert.Equal(2, client.PageScreenshotCount);
    }

    [Fact]
    public void BeforeAction_InvalidPointerPathFailsClearly()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path));
        recorder.Start("debug");

        var error = Assert.Throws<ScriptExecutionException>(() =>
            recorder.BeforeAction(new BrowserScriptAction(1, "hover", "hover", ["#save"], new Dictionary<string, string>
            {
                ["pointerPath"] = "wobble"
            }, [])));

        Assert.Contains("pointerPath= must be one of", error.Message);
    }

    [Fact]
    public void AfterAction_UsesConfiguredClickPulse()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(
            client,
            new ScriptRecordingOptions(path, ClickPulse: ClickPulseStyle.Ripple));
        recorder.Start("debug");

        recorder.AfterAction(new BrowserScriptAction(1, "click", "click", ["#save"], new Dictionary<string, string>(), []));

        Assert.Contains(ClickPulseStyle.Ripple, client.CursorPulseStyles);
    }

    [Fact]
    public void AfterAction_ActionClickPulseOverridesRecordingDefault()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(
            client,
            new ScriptRecordingOptions(path, ClickPulse: ClickPulseStyle.Ring));
        recorder.Start("debug");

        recorder.AfterAction(new BrowserScriptAction(1, "click", "click", ["#save"], new Dictionary<string, string>
        {
            ["clickPulse"] = "dot"
        }, []));

        Assert.Contains(ClickPulseStyle.Dot, client.CursorPulseStyles);
    }

    [Fact]
    public void AfterAction_ActionHoldZeroSkipsPostActionHold()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path));
        recorder.Start("debug");

        recorder.AfterAction(new BrowserScriptAction(1, "click", "click", ["#save"], new Dictionary<string, string>
        {
            ["holdAfterAction"] = "0"
        }, []));

        Assert.Equal(1, client.PageScreenshotCount);
    }

    [Fact]
    public void BeforeAction_ClickPreHoldCapturesSettleFrame()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var motion = new ScriptPointerMotionOptions(PointerDurationMilliseconds: 0);
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path, PointerMotion: motion, PreClickHoldMilliseconds: 500));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, "click", "click", ["#save"], new Dictionary<string, string>(), []));

        Assert.Equal(2, client.PageScreenshotCount);
    }

    [Fact]
    public void AfterAction_ClickPostHoldUsesSpecificDefault()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path, PostClickHoldMilliseconds: 0));
        recorder.Start("debug");

        recorder.AfterAction(new BrowserScriptAction(1, "click", "click", ["#save"], new Dictionary<string, string>(), []));

        Assert.Equal(1, client.PageScreenshotCount);
    }

    [Fact]
    public void AfterAction_NavigationAndAssertionUseSpecificHolds()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(
            path,
            HoldAfterNavigationMilliseconds: 0,
            HoldAfterAssertionMilliseconds: 0));
        recorder.Start("debug");

        recorder.AfterAction(new BrowserScriptAction(1, "navigate", "navigate", ["/"], new Dictionary<string, string>(), []));
        recorder.AfterAction(new BrowserScriptAction(2, "expectText", "expectText", ["#status", "Ready"], new Dictionary<string, string>(), []));

        Assert.Equal(0, client.PageScreenshotCount);
    }

    [Fact]
    public void PauseGif_CapturesOneRecordingFrame()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path));
        recorder.Start("debug");

        recorder.Pause(new BrowserScriptAction(1, "pauseGif", "pauseGif", ["500"], new Dictionary<string, string>(), []));

        Assert.Equal(1, client.PageScreenshotCount);
    }

    [Fact]
    public void CaptureFailureHold_CapturesConfiguredFrame()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(
            client,
            new ScriptRecordingOptions(path, HoldOnFailureMilliseconds: 900));
        recorder.Start("debug");

        recorder.CaptureFailureHold();

        Assert.Equal(1, client.PageScreenshotCount);
    }
}
