
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

    private void MoveToSelector(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var selector = action.Arguments[0];
        var target = action.Options.ContainsKey("x") || action.Options.ContainsKey("y")
            ? ResolveElementOffsetTarget(action, selector)
            : devToolsClient.GetElementCenter(remoteDebuggingUrl, selector);

        MovePointerTo(target, dragging: false);
    }

    private ElementPoint ResolveElementOffsetTarget(BrowserScriptAction action, string selector)
    {
        var box = devToolsClient.GetElementBox(remoteDebuggingUrl!, selector);
        var x = action.Options.TryGetValue("x", out var rawX)
            ? ParseElementOffset(rawX, action.Name, "x")
            : box.Width / 2;
        var y = action.Options.TryGetValue("y", out var rawY)
            ? ParseElementOffset(rawY, action.Name, "y")
            : box.Height / 2;

        return new ElementPoint(box.X + x, box.Y + y);
    }

    private static double ParseElementOffset(string value, string actionName, string optionName) =>
        double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) && number >= 0
            ? number
            : throw new ScriptExecutionException($"{actionName} option {optionName}= must be zero or greater.");

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
