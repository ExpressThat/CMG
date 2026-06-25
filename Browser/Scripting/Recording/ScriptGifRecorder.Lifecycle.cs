
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
