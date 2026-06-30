
using CMG.Browser.Scripting;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private void MoveToFrameSelector(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var frameSelector = action.Arguments[0];
        var selector = action.Arguments[1];
        var json = devToolsClient.Evaluate(remoteDebuggingUrl, BrowserFrameScripts.TargetCenter(frameSelector, selector));
        using var document = System.Text.Json.JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty("x", out var x) || !root.TryGetProperty("y", out var y))
        {
            throw new ScriptExecutionException($"Could not resolve frame selector '{selector}'.");
        }

        MovePointerTo(new ElementPoint(x.GetDouble(), y.GetDouble()), dragging: false, action);
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

        MovePointerTo(target, dragging: false, action);
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

    private void MoveDragToSelector(string selector, BrowserScriptAction? action = null, string? durationOption = null, string? easingOption = null)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var target = devToolsClient.GetElementCenter(remoteDebuggingUrl, selector);

        MovePointerTo(target, dragging: true, action, durationOption, easingOption);
    }

    private void MovePointerTo(
        ElementPoint target,
        bool dragging,
        BrowserScriptAction? action = null,
        string? durationOption = null,
        string? easingOption = null)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var motion = MotionFor(action, durationOption, easingOption);
        foreach (var point in pointer.MoveTo(target, motion.FrameCount(action?.Name ?? "recording"), motion.PointerEasing))
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

    private ScriptPointerMotionOptions MotionFor(BrowserScriptAction? action, string? durationOption, string? easingOption)
    {
        var motion = options.EffectivePointerMotion;
        if (action is null)
        {
            return motion;
        }

        var isMoveMouse = action.Name.Equals("moveMouse", StringComparison.OrdinalIgnoreCase);
        motion = motion.WithAction(action, isMoveMouse ? "duration" : null, isMoveMouse ? "easing" : null);
        if (durationOption is not null)
        {
            motion = motion.WithDurationOption(action, durationOption);
        }

        return easingOption is null ? motion : motion.WithEasingOption(action, easingOption);
    }
}
