using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderClickPulseTests
{
    [Fact]
    public void AfterAction_RightClickUsesCrosshairPulseByDefault()
    {
        var client = new FakeAutomationClient();
        using var recorder = Recorder(client);
        recorder.Start("debug");

        recorder.AfterAction(Action("rightClick", "#menu"));

        Assert.Contains(ClickPulseStyle.Crosshair, client.CursorPulseStyles);
    }

    [Fact]
    public void AfterAction_MiddleClickUsesDotPulseByDefault()
    {
        var client = new FakeAutomationClient();
        using var recorder = Recorder(client);
        recorder.Start("debug");

        recorder.AfterAction(Action("click", "#target", new() { ["button"] = "middle" }));

        Assert.Contains(ClickPulseStyle.Dot, client.CursorPulseStyles);
    }

    [Fact]
    public void AfterAction_DoubleClickCapturesTwoPulseFrames()
    {
        var client = new FakeAutomationClient();
        using var recorder = Recorder(client, postClickHoldMilliseconds: 0);
        recorder.Start("debug");

        recorder.AfterAction(Action("dblclick", "#target"));

        Assert.Equal(2, client.PageScreenshotCount);
        Assert.Equal(2, client.CursorPulseStyles.Count(style => style is ClickPulseStyle.Ring));
    }

    [Fact]
    public void AfterAction_ActionClickPulseOverridesVariantDefault()
    {
        var client = new FakeAutomationClient();
        using var recorder = Recorder(client);
        recorder.Start("debug");

        recorder.AfterAction(Action("rightClick", "#menu", new() { ["clickPulse"] = "ripple" }));

        Assert.Contains(ClickPulseStyle.Ripple, client.CursorPulseStyles);
        Assert.DoesNotContain(ClickPulseStyle.Crosshair, client.CursorPulseStyles);
    }

    private static ScriptGifRecorder Recorder(FakeAutomationClient client, int postClickHoldMilliseconds = 350) =>
        new(client, new ScriptRecordingOptions(
            Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"),
            PostClickHoldMilliseconds: postClickHoldMilliseconds));

    private static BrowserScriptAction Action(
        string name,
        string selector,
        Dictionary<string, string>? options = null) =>
        new(1, name, name, [selector], options ?? [], []);
}
