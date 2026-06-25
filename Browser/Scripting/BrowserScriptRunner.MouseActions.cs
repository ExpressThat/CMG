using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteMouseAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptGifRecorder? recorder)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "mousemove" => MouseMove(remoteDebuggingUrl, automationClient, action, recorder),
            "mousedown" => MouseButton(remoteDebuggingUrl, automationClient, action, recorder, down: true),
            "mouseup" => MouseButton(remoteDebuggingUrl, automationClient, action, recorder, down: false),
            _ => throw new ScriptExecutionException($"Unknown mouse action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> MouseMove(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptGifRecorder? recorder)
    {
        var target = BrowserMouseTargetResolver.Resolve(remoteDebuggingUrl, automationClient, action);
        if (recorder is null)
        {
            automationClient.MoveMouse(remoteDebuggingUrl, target, buttons: 0);
        }
        else
        {
            recorder.MoveMouse(action, dragging: false);
        }

        return [$"MOUSE_MOVED {action.LineNumber:000} {FormatPoint(target)}"];
    }

    private static IReadOnlyList<string> MouseButton(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptGifRecorder? recorder,
        bool down)
    {
        var target = BrowserMouseTargetResolver.Resolve(remoteDebuggingUrl, automationClient, action);
        recorder?.MoveMouse(action, dragging: down);
        if (down)
        {
            automationClient.MouseDown(remoteDebuggingUrl, target);
            return [$"MOUSE_DOWN {action.LineNumber:000} {FormatPoint(target)}"];
        }

        automationClient.MouseUp(remoteDebuggingUrl, target);
        return [$"MOUSE_UP {action.LineNumber:000} {FormatPoint(target)}"];
    }

    private static string FormatPoint(ElementPoint point) =>
        $"{point.X.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)},{point.Y.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}";
}
