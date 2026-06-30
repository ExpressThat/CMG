using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderTouchPointerTests
{
    [Theory]
    [InlineData("tap")]
    [InlineData("touchTap")]
    public void BeforeAction_TapUsesTouchPointer(string actionName)
    {
        var client = new FakeAutomationClient();
        using var recorder = new ScriptGifRecorder(
            client,
            new ScriptRecordingOptions(
                Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"),
                PointerMotion: new ScriptPointerMotionOptions(PointerDurationMilliseconds: 0)));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, actionName, actionName, ["#target"], new Dictionary<string, string>(), []));

        Assert.Contains(client.CursorStates, state => state.Touch);
    }

    [Fact]
    public void BeforeAction_ClickUsesArrowPointer()
    {
        var client = new FakeAutomationClient();
        using var recorder = new ScriptGifRecorder(
            client,
            new ScriptRecordingOptions(
                Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"),
                PointerMotion: new ScriptPointerMotionOptions(PointerDurationMilliseconds: 0)));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, "click", "click", ["#target"], new Dictionary<string, string>(), []));

        Assert.DoesNotContain(client.CursorStates, state => state.Touch);
    }
}
