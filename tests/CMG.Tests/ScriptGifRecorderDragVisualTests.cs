using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderDragVisualTests
{
    [Fact]
    public void DragDelay_ShowsPressedPointerAndOptionalTrail()
    {
        var client = new FakeAutomationClient();
        using var recorder = Recorder(client);
        recorder.Start("debug");

        recorder.BeginDrag(Action("dragAndDrop", ["#source"], new()
        {
            ["dragTrail"] = "true",
            ["dragBreadcrumbs"] = "true"
        }));
        recorder.DragDelay(Action("delay", ["100"], new()
        {
            ["dragTrail"] = "true",
            ["dragBreadcrumbs"] = "true"
        }), 100);

        Assert.Contains(client.CursorStates, state => state.Pressed && state.Trail && state.Breadcrumb);
    }

    [Fact]
    public void DropDrag_ReleasesPressedPointerBeforeDropPulse()
    {
        var client = new FakeAutomationClient();
        using var recorder = Recorder(client);
        recorder.Start("debug");

        recorder.BeginDrag(Action("dragAndDrop", ["#source"]));
        recorder.DropDrag(Action("drop", ["#target"]));

        Assert.False(client.LastCursorPressed);
    }

    [Fact]
    public void DragVisualOptionsValidateBooleanValues()
    {
        var client = new FakeAutomationClient();
        using var recorder = Recorder(client);
        recorder.Start("debug");

        var error = Assert.Throws<ScriptExecutionException>(() =>
            recorder.BeginDrag(Action("dragAndDrop", ["#source"], new()
            {
                ["dragTrail"] = "maybe"
            })));

        Assert.Contains("dragTrail= must be true or false", error.Message);
    }

    private static ScriptGifRecorder Recorder(FakeAutomationClient client) =>
        new(client, new ScriptRecordingOptions(
            Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"),
            PointerMotion: new ScriptPointerMotionOptions(PointerDurationMilliseconds: 0),
            PostClickHoldMilliseconds: 0));

    private static BrowserScriptAction Action(
        string name,
        IReadOnlyList<string> arguments,
        Dictionary<string, string>? options = null) =>
        new(1, name, name, arguments, options ?? [], []);
}
