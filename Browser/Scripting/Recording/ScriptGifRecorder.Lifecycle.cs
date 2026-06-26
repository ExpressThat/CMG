
namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public void RecordDragAndDrop(string sourceSelector, string targetSelector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        MoveToSelector(sourceSelector);
        CaptureHoldFrame();

        devToolsClient.BeginPageDrag(remoteDebuggingUrl, sourceSelector, pointer.Position);
        MoveDragToSelector(targetSelector);
        devToolsClient.EndPageDrag(remoteDebuggingUrl, pointer.Position);
        CapturePulseFrame();
        CaptureHoldFrame();
    }

    public void RecordDragAndDrop(BrowserScriptAction action, ElementPoint start, ElementPoint end)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var sourceSelector = action.Arguments[0];
        MovePointerTo(start, dragging: false);
        CaptureHoldFrame();

        devToolsClient.BeginPageDrag(remoteDebuggingUrl, sourceSelector, pointer.Position);
        MovePointerTo(end, dragging: true);
        devToolsClient.EndPageDrag(remoteDebuggingUrl, pointer.Position);
        CapturePulseFrame();
        CaptureHoldFrame();
    }

    public void Finish()
    {
        if (remoteDebuggingUrl is not null)
        {
            TryRemoveDomCursor();
        }

        frameSink.Save(OutputPath);
    }

    public void Dispose()
    {
        frameSink.Dispose();
    }
}
