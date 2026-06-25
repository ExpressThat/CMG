
namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public void BeginDrag(string sourceSelector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MoveToSelector(sourceSelector);
        CaptureHoldFrame();
        devToolsClient.BeginPageDrag(remoteDebuggingUrl, sourceSelector, pointer.Position);
    }

    public void DragHover(string selector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MoveDragToSelector(selector);
    }

    public void DragDelay(int milliseconds)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var frameDelayMilliseconds = ScriptRecordingOptions.FrameDelayCentiseconds * 10;
        var frameCount = Math.Max(1, milliseconds / Math.Max(1, frameDelayMilliseconds));

        for (var index = 0; index < frameCount; index++)
        {
            devToolsClient.MoveMouse(remoteDebuggingUrl, pointer.Position, buttons: 1);
            devToolsClient.MovePageDrag(remoteDebuggingUrl, pointer.Position);
            CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds);
        }
    }

    public void DropDrag(string targetSelector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MoveDragToSelector(targetSelector);
        devToolsClient.EndPageDrag(remoteDebuggingUrl, pointer.Position);
        CapturePulseFrame();
        CaptureHoldFrame();
    }

    public void MoveMouse(BrowserScriptAction action, bool dragging)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MovePointerTo(ResolveMoveMouseTarget(action), dragging);
    }
}
