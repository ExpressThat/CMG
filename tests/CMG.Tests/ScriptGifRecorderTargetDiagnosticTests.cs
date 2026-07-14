using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderTargetDiagnosticTests
{
    [Fact]
    public void BeforeAction_ReportsAmbiguousTinyAndOffscreenTarget()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"count":3,"width":10,"height":12,"offscreen":true}""");
        using var recorder = Recorder(client);
        recorder.Start("debug");

        recorder.BeforeAction(Action("click", ["#save"]));

        var lines = recorder.CaptureDiagnosticLines();
        Assert.Contains(lines, line => line.StartsWith("GIF_WARN_MULTIPLE_TARGETS", StringComparison.Ordinal) && line.Contains("count=3", StringComparison.Ordinal));
        Assert.Contains(lines, line => line.StartsWith("GIF_WARN_TINY_TARGET", StringComparison.Ordinal) && line.Contains("width=10 height=12", StringComparison.Ordinal));
        Assert.Contains(lines, line => line.StartsWith("GIF_WARN_SCROLLED", StringComparison.Ordinal) && line.Contains("reason=offscreen-target", StringComparison.Ordinal));
    }

    [Fact]
    public void CompleteAction_ReportsIgnoredExplicitVisualOptions()
    {
        var client = new FakeAutomationClient();
        using var recorder = Recorder(client);
        recorder.Start("debug");
        var action = Action("recordCheckpoint", ["metadata-only"], new Dictionary<string, string> { ["pointerDuration"] = "200" });

        recorder.BeforeAction(action, sequence: 1);
        recorder.CompleteAction(1, success: true);

        Assert.Contains(recorder.CaptureDiagnosticLines(), line =>
            line == "GIF_WARN_NON_VISUAL line=4 action=recordCheckpoint options=pointerDuration");
    }

    [Fact]
    public void TargetDiagnosticParseFailure_DoesNotFailAction()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("not-json");
        using var recorder = Recorder(client);
        recorder.Start("debug");

        recorder.BeforeAction(Action("hover", ["#save"]));

        Assert.DoesNotContain(recorder.CaptureDiagnosticLines(), line => line.StartsWith("GIF_WARN_", StringComparison.Ordinal));
    }

    [Fact]
    public void PromotionFailure_EmitsOneParseableWarning()
    {
        using var recorder = Recorder(new FakeAutomationClient());

        recorder.RecordPromotionDiagnostic("{\"failed\":[\"__cmg_virtual_cursor\"]}");
        recorder.RecordPromotionDiagnostic("{\"failed\":[\"__cmg_virtual_cursor\"]}");

        Assert.Single(recorder.CaptureDiagnosticLines(), line =>
            line == "GIF_WARN_POINTER_PROMOTION overlays=__cmg_virtual_cursor reason=top-layer-promotion-failed");
    }

    private static ScriptGifRecorder Recorder(FakeAutomationClient client) =>
        new(client, new ScriptRecordingOptions(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"),
            PointerMotion: new ScriptPointerMotionOptions(PointerDurationMilliseconds: 0)));

    private static BrowserScriptAction Action(string name, IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string>? options = null) =>
        new(4, name, name, arguments, options ?? new Dictionary<string, string>(), []);
}
