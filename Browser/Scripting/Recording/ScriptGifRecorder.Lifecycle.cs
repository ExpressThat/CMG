
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
            TryRemoveDomCursor();
        }

        frameSink.Save(OutputPath);
        if (frameSink.FrameCount > 0 && !string.IsNullOrWhiteSpace(options.TimelinePath))
        {
            TimelinePath = GifTimelineWriter.Write(options.TimelinePath, OutputPath, options, frameSink, checkpoints, timelineSteps, redactionAudit, debugFrames);
        }
        if (frameSink.FrameCount > 0 && debugFrames.Count > 0)
        {
            DebugPath = GifTimelineWriter.Write(Path.ChangeExtension(OutputPath, ".debug.json"), OutputPath, options,
                frameSink, checkpoints, timelineSteps, redactionAudit, debugFrames);
        }
        RestoreRecordingViewport();
    }

    public string? TimelinePath { get; private set; }

    public string? DebugPath { get; private set; }

    public void Dispose()
    {
        RestoreRecordingViewport();
        frameSink.Dispose();
    }
}
