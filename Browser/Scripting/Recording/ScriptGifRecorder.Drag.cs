
namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public void BeginDrag(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var sourceSelector = action.Arguments[0];
        MoveToSelector(action with { Options = SourceMoveOptions(action) });
        CaptureHoldFrame(action, "preDragHold");
        devToolsClient.BeginPageDrag(remoteDebuggingUrl, sourceSelector, pointer.Position);
    }

    public void DragHover(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MoveDragToSelector(action.Arguments[0], action);
    }

    public void DragDelay(int milliseconds)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var frameDelayMilliseconds = options.FrameDelayMilliseconds;
        var frameCount = Math.Max(1, milliseconds / Math.Max(1, frameDelayMilliseconds));

        for (var index = 0; index < frameCount; index++)
        {
            devToolsClient.MoveMouse(remoteDebuggingUrl, pointer.Position, buttons: 1);
            devToolsClient.MovePageDrag(remoteDebuggingUrl, pointer.Position);
            CaptureFrame(options.FrameDelayCentiseconds);
        }
    }

    public void DropDrag(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MoveDragToSelector(action.Arguments[0], action, "dropPointerDuration");
        CaptureHoldFrame(action, "dragHold");
        devToolsClient.EndPageDrag(remoteDebuggingUrl, pointer.Position);
        CapturePulseFrame(action);
        CaptureHoldFrame(action, "postDropHold");
    }

    public void MoveMouse(BrowserScriptAction action, bool dragging)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MovePointerTo(ResolveMoveMouseTarget(action), dragging, action, "duration", "easing");
        CaptureOptionalHoldFrame(action, "holdAfterMove");
    }

    private static IReadOnlyDictionary<string, string> SourceMoveOptions(BrowserScriptAction action)
    {
        var options = new Dictionary<string, string>(action.Options, StringComparer.OrdinalIgnoreCase);
        options.Remove("pointerDuration");
        options.Remove("targetPointerDuration");
        options.Remove("dropPointerDuration");
        options.Remove("dragEasing");
        options.Remove("dragHold");
        options.Remove("postDropHold");
        if (!action.Options.TryGetValue("sourcePointerDuration", out var duration))
        {
            return options;
        }

        options["pointerDuration"] = duration;
        return options;
    }
}
