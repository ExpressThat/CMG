
namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public void RecordDragAndDrop(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var sourceSelector = action.Arguments[0];
        var targetSelector = action.Arguments[1];
        MoveToSelector(action with { Options = SourceMoveOptions(action) });
        CaptureHoldFrame(action, "preDragHold");

        devToolsClient.BeginPageDrag(remoteDebuggingUrl, sourceSelector, pointer.Position);
        MoveDragToSelector(targetSelector, action, "targetPointerDuration", "dragEasing");
        CaptureHoldFrame(action, "dragHold");
        devToolsClient.EndPageDrag(remoteDebuggingUrl, pointer.Position);
        CapturePulseFrame(action);
        CaptureHoldFrame(action, "postDropHold");
    }

    public void RecordDragAndDrop(BrowserScriptAction action, ElementPoint start, ElementPoint end)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var sourceSelector = action.Arguments[0];
        MovePointerTo(start, dragging: false, action with { Options = SourceMoveOptions(action) });
        CaptureHoldFrame(action, "preDragHold");

        devToolsClient.BeginPageDrag(remoteDebuggingUrl, sourceSelector, pointer.Position);
        MovePointerTo(end, dragging: true, action, "targetPointerDuration", "dragEasing");
        CaptureHoldFrame(action, "dragHold");
        devToolsClient.EndPageDrag(remoteDebuggingUrl, pointer.Position);
        CapturePulseFrame(action);
        CaptureHoldFrame(action, "postDropHold");
    }

    public void Finish(GifRecordingOutcome outcome = GifRecordingOutcome.Passed)
    {
        CaptureConfiguredOutro(outcome);
        if (remoteDebuggingUrl is not null)
        {
            CleanupDomEvidence();
        }

        frameSink.Save(OutputPath);
        var narrationPath = options.EffectiveReview.ResolveNarrationPath(OutputPath);
        if (frameSink.FrameCount > 0 && narrationPath is not null)
        {
            NarrationPath = GifNarrationWriter.Write(
                narrationPath, OutputPath, options.EffectiveReview, frameSink, timelineSteps, outcome);
        }
        var timelinePath = options.TimelinePath ?? (options.EffectiveEncoding.Format is GifArtifactFormat.Mp4
            ? GifArtifactPaths.Timeline(OutputPath) : null);
        if (frameSink.FrameCount > 0 && !string.IsNullOrWhiteSpace(timelinePath))
        {
            TimelinePath = GifTimelineWriter.Write(timelinePath, OutputPath, options, frameSink, checkpoints, timelineSteps, redactionAudit, debugFrames, outcome);
        }
        if (frameSink.FrameCount > 0 && debugFrames.Count > 0)
        {
            DebugPath = GifTimelineWriter.Write(GifArtifactPaths.Debug(OutputPath), OutputPath, options,
                frameSink, checkpoints, timelineSteps, redactionAudit, debugFrames, outcome);
        }
        RestoreRecordingViewport();
    }

    public string? TimelinePath { get; private set; }

    public string? DebugPath { get; private set; }

    public string? NarrationPath { get; private set; }

    public void Dispose()
    {
        RestoreRecordingViewport();
        frameSink.Dispose();
    }
}
