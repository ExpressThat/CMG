
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

        var resolved = ResolveLocator(selector, lineNumber: 0);
        devToolsClient.ScrollElementIntoView(remoteDebuggingUrl, resolved);
        StabilizeTarget(resolved);
        var bounds = devToolsClient.GetElementBox(remoteDebuggingUrl, resolved);
        var target = Center(bounds);

        MovePointerTo(target, dragging: false, targetBounds: bounds);
    }

    private void MoveToSelector(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var selector = ResolveLocator(action.Arguments[0], action.LineNumber);
        InspectTarget(action, selector);
        devToolsClient.ScrollElementIntoView(remoteDebuggingUrl, selector);
        StabilizeTarget(selector);
        var bounds = devToolsClient.GetElementBox(remoteDebuggingUrl, selector);
        var target = action.Options.ContainsKey("x") || action.Options.ContainsKey("y")
            ? ResolveElementOffsetTarget(action, bounds)
            : devToolsClient.GetElementCenter(remoteDebuggingUrl, selector);

        MovePointerTo(target, dragging: false, action, targetBounds: bounds);
    }

    private static ElementPoint ResolveElementOffsetTarget(BrowserScriptAction action, ElementBox box)
    {
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

        StabilizeTarget(selector);
        var bounds = devToolsClient.GetElementBox(remoteDebuggingUrl, selector);
        var target = Center(bounds);

        MovePointerTo(target, dragging: true, action, durationOption, easingOption, targetBounds: bounds);
    }

    private void StabilizeTarget(string selector)
    {
        var framing = options.EffectiveFraming;
        devToolsClient.Evaluate(remoteDebuggingUrl!, BrowserDomScripts.StabilizeGifTarget(
            selector, framing.SafeArea, framing.LayoutStabilityMilliseconds));
    }

    private void MovePointerTo(
        ElementPoint target,
        bool dragging,
        BrowserScriptAction? action = null,
        string? durationOption = null,
        string? easingOption = null,
        bool pressed = false,
        ElementBox? targetBounds = null)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        var frameDelay = FrameDelayMillisecondsFor(action);
        var frameDelayCentiseconds = Math.Max(1, (frameDelay + 9) / 10);
        var motion = MotionFor(action, durationOption, easingOption);
        var path = dragging ? motion.DragPath ?? motion.PointerPath : motion.PointerPath;
        SetCursorState(action, dragging || pressed);
        var source = pointer.Position;
        var actionName = action?.Name ?? "recording";
        var points = pointer.MoveTo(target, motion.FrameCount(actionName, frameDelay), motion.PointerEasing, path, targetBounds).ToArray();
        var evidence = PointerEvidenceFor(action);
        teleportOrigin = evidence.TeleportMarker && motion.DurationMilliseconds(actionName) is 0 && source != target ? source : null;
        try
        {
            for (var index = 0; index < points.Length; index++)
            {
                var point = points[index];
                pointer.Set(point);
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

    private static ElementPoint Center(ElementBox box) =>
        new(box.X + box.Width / 2, box.Y + box.Height / 2);
}
