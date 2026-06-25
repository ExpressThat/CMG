
using CMG.Browser.Scripting;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private void MoveToFrameSelector(string frameSelector, string selector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var json = devToolsClient.Evaluate(remoteDebuggingUrl, BrowserFrameScripts.TargetCenter(frameSelector, selector));
        using var document = System.Text.Json.JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty("x", out var x) || !root.TryGetProperty("y", out var y))
        {
            throw new ScriptExecutionException($"Could not resolve frame selector '{selector}'.");
        }

        MovePointerTo(new ElementPoint(x.GetDouble(), y.GetDouble()), dragging: false);
    }

    private void MoveToSelector(string selector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var target = devToolsClient.GetElementCenter(remoteDebuggingUrl, selector);

        MovePointerTo(target, dragging: false);
    }

    private void MoveDragToSelector(string selector)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var target = devToolsClient.GetElementCenter(remoteDebuggingUrl, selector);

        MovePointerTo(target, dragging: true);
    }

    private void MovePointerTo(ElementPoint target, bool dragging)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        foreach (var point in pointer.MoveTo(target, ScriptRecordingOptions.MovementFrameCount))
        {
            devToolsClient.MoveMouse(remoteDebuggingUrl, point, dragging ? 1 : 0);
            if (dragging)
            {
                devToolsClient.MovePageDrag(remoteDebuggingUrl, point);
            }

            devToolsClient.MoveDomCursor(remoteDebuggingUrl, point);
            CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds);
        }
    }
}
