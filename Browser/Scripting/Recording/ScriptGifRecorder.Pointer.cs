
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

        var target = devToolsClient.GetElementCenter(remoteDebuggingUrl, ResolveLocator(selector, lineNumber: 0));

        MovePointerTo(target, dragging: false);
    }

    private void MoveToSelector(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var selector = ResolveLocator(action.Arguments[0], action.LineNumber);
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
        string? easingOption = null,
        bool pressed = false)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var frameDelay = FrameDelayMillisecondsFor(action);
        var frameDelayCentiseconds = Math.Max(1, (frameDelay + 9) / 10);
        var motion = MotionFor(action, durationOption, easingOption);
        var path = PathFor(action, dragging);
        SetCursorState(action, dragging || pressed);
        var source = pointer.Position;
        var actionName = action?.Name ?? "recording";
        var points = pointer.MoveTo(target, motion.FrameCount(actionName, frameDelay), motion.PointerEasing, path).ToArray();
        var evidence = PointerEvidenceFor(action);
        teleportOrigin = evidence.TeleportMarker && motion.DurationMilliseconds(actionName) is 0 && source != target ? source : null;
        try
        {
            for (var index = 0; index < points.Length; index++)
            {
                var point = points[index];
                devToolsClient.MoveMouse(remoteDebuggingUrl, point, dragging || pressed ? 1 : 0);
                if (dragging)
                {
                    devToolsClient.MovePageDrag(remoteDebuggingUrl, point);
                }

                CaptureFrame(frameDelayCentiseconds, action: action, sampleEligible: index < points.Length - 1);
            }
        }
        finally
        {
            teleportOrigin = null;
        }
    }

    private void SetCursorState(BrowserScriptAction? action, bool dragging)
    {
        cursorPressed = dragging && BoolOption(action, "pressedPointer", true);
        cursorTrail = dragging && BoolOption(action, "dragTrail", false);
        cursorBreadcrumb = dragging && BoolOption(action, "dragBreadcrumbs", false);
        var touch = action?.Name.Equals("tap", StringComparison.OrdinalIgnoreCase) is true ||
            action?.Name.Equals("touchTap", StringComparison.OrdinalIgnoreCase) is true;
        cursorVisual = action is null
            ? options.EffectivePointerVisual
            : options.EffectivePointerVisual.WithAction(action, touch);
    }

    private static bool BoolOption(BrowserScriptAction? action, string name, bool fallback)
    {
        if (action?.Options.TryGetValue(name, out var value) is not true)
        {
            return fallback;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{action.Name} option {name}= must be true or false.")
        };
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

    private static ScriptPointerPath PathFor(BrowserScriptAction? action, bool dragging)
    {
        if (action is null)
        {
            return ScriptPointerPath.Direct;
        }

        if (dragging && action.Options.TryGetValue("dragPath", out var dragPath))
        {
            return ParsePath(action, dragPath, "dragPath");
        }

        return action.Options.TryGetValue("pointerPath", out var pointerPath)
            ? ParsePath(action, pointerPath, "pointerPath")
            : ScriptPointerPath.Direct;
    }

    private static ScriptPointerPath ParsePath(BrowserScriptAction action, string value, string optionName) =>
        ScriptPointerPathParser.TryParse(value, out var path)
            ? path
            : throw new ScriptExecutionException($"{action.Name} option {optionName}= must be one of: {ScriptPointerPathParser.Values}.");
}
