
namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private void CaptureHoldFrame()
    {
        CaptureHoldFrame(options.HoldAfterActionMilliseconds);
    }

    private void CaptureHoldFrame(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("holdAfterAction", out var value))
        {
            CaptureHoldFrame();
            return;
        }

        CaptureHoldFrame(ParseHoldMilliseconds(action, value, "holdAfterAction"));
    }

    private void CaptureHoldFrame(BrowserScriptAction action, string optionName)
    {
        if (!action.Options.TryGetValue(optionName, out var value))
        {
            CaptureHoldFrame();
            return;
        }

        CaptureHoldFrame(ParseHoldMilliseconds(action, value, optionName));
    }

    private void CaptureOptionalHoldFrame(BrowserScriptAction action, string optionName)
    {
        if (action.Options.TryGetValue(optionName, out var value))
        {
            CaptureHoldFrame(ParseHoldMilliseconds(action, value, optionName));
        }
    }

    private void CapturePreClickHoldFrame(BrowserScriptAction action)
    {
        CaptureHoldFrame(HoldMillisecondsFor(action, "preClickHold", options.PreClickHoldMilliseconds));
    }

    private void CapturePostClickHoldFrame(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("postClickHold", out var value))
        {
            CaptureHoldFrame(ParseHoldMilliseconds(action, value, "postClickHold"));
            return;
        }

        CaptureHoldFrame(action.Options.ContainsKey("holdAfterAction")
            ? HoldMillisecondsFor(action, "holdAfterAction", options.PostClickHoldMilliseconds)
            : options.PostClickHoldMilliseconds);
    }

    private void CaptureNavigationHoldFrame(BrowserScriptAction action)
    {
        CaptureHoldFrame(HoldMillisecondsFor(action, "holdAfterNavigation", options.HoldAfterNavigationMilliseconds));
    }

    private void CaptureAssertionHoldFrame(BrowserScriptAction action)
    {
        CaptureHoldFrame(HoldMillisecondsFor(action, "holdAfterAssertion", options.HoldAfterAssertionMilliseconds));
    }

    public void Pause(BrowserScriptAction action)
    {
        if (action.Arguments.Count is not 1)
        {
            throw new ScriptExecutionException("pauseGif requires milliseconds.");
        }

        CaptureHoldFrame(ParseHoldMilliseconds(action, action.Arguments[0], "milliseconds"));
    }

    public void CaptureFailureHold()
    {
        CaptureHoldFrame(options.HoldOnFailureMilliseconds);
    }

    public void ShowPointer(BrowserScriptAction action)
    {
        CaptureFrame(FrameDelayCentisecondsFor(action));
    }

    public void HidePointer(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        devToolsClient.RemoveDomCursor(remoteDebuggingUrl);
        var screenshot = devToolsClient.GetPageScreenshot(remoteDebuggingUrl, promoteMessageBar: true);
        frameSink.AddFrame(screenshot, FrameDelayCentisecondsFor(action));
    }

    private void CaptureHoldFrame(int milliseconds)
    {
        if (milliseconds <= 0)
        {
            return;
        }

        CaptureFrame(Math.Max(1, (milliseconds + 9) / 10));
    }

    private static int ParseHoldMilliseconds(BrowserScriptAction action, string value, string optionName) =>
        ScriptPointerMotionOptions.ParseDuration(value, $"{action.Name} option {optionName}=");

    private static int HoldMillisecondsFor(BrowserScriptAction action, string optionName, int fallback)
    {
        if (!action.Options.TryGetValue(optionName, out var value))
        {
            return fallback;
        }

        return ParseHoldMilliseconds(action, value, optionName);
    }

    private void CapturePulseFrame()
    {
        CapturePulseFrame(null);
    }

    private void CapturePulseFrame(BrowserScriptAction? action)
    {
        CaptureFrame(FrameDelayCentisecondsFor(action), PulseStyleFor(action));
    }

    private void CaptureClickPulseFrames(BrowserScriptAction action)
    {
        var pulseStyle = PulseStyleFor(action);
        var pulseCount = PulseCountFor(action);
        for (var index = 0; index < pulseCount; index++)
        {
            CaptureFrame(FrameDelayCentisecondsFor(action), pulseStyle);
        }
    }

    private void CaptureFrame(int delayCentiseconds, ClickPulseStyle? pulseStyle = null)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        devToolsClient.PromoteMessageBar(remoteDebuggingUrl);
        devToolsClient.MoveDomCursor(remoteDebuggingUrl, pointer.Position, pulseStyle, cursorPressed, cursorTrail, cursorBreadcrumb, cursorVisual);
        var screenshot = devToolsClient.GetPageScreenshot(remoteDebuggingUrl, promoteMessageBar: false);
        frameSink.AddFrame(screenshot, delayCentiseconds);
    }

    private ClickPulseStyle PulseStyleFor(BrowserScriptAction? action)
    {
        if (action?.Options.TryGetValue("clickPulse", out var value) is true ||
            action?.Options.TryGetValue("pulse", out value) is true)
        {
            return ClickPulseStyleParser.TryParse(value, out var style)
                ? style
                : throw new ScriptExecutionException($"{action.Name} option clickPulse= must be one of: {ClickPulseStyleParser.Values}.");
        }

        if (action is not null)
        {
            var actionName = action.Name.ToLowerInvariant();
            if (actionName is "rightclick" or "contextclick" ||
                action.Options.GetValueOrDefault("button")?.Equals("right", StringComparison.OrdinalIgnoreCase) is true)
            {
                return ClickPulseStyle.Crosshair;
            }

            if (action.Options.GetValueOrDefault("button")?.Equals("middle", StringComparison.OrdinalIgnoreCase) is true)
            {
                return ClickPulseStyle.Dot;
            }
        }

        return options.ClickPulse;
    }

    private static int PulseCountFor(BrowserScriptAction action)
    {
        var name = action.Name.ToLowerInvariant();
        if (name is "dblclick" or "doubleclick")
        {
            return 2;
        }

        var countText = action.Options.GetValueOrDefault("clickCount") ?? action.Options.GetValueOrDefault("count");
        return int.TryParse(countText, out var count) && count > 1 ? Math.Min(count, 4) : 1;
    }

    private int FrameDelayMillisecondsFor(BrowserScriptAction? action) =>
        action is null
            ? options.FrameDelayMilliseconds
            : ScriptFrameTimingOptions.FromOptions(action.Options, action.Name, options.FrameDelayMilliseconds);

    private int FrameDelayCentisecondsFor(BrowserScriptAction? action) =>
        Math.Max(1, (FrameDelayMillisecondsFor(action) + 9) / 10);

    private void TryRemoveDomCursor()
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        try
        {
            devToolsClient.RemoveDomCursor(remoteDebuggingUrl);
            devToolsClient.RemoveDefaultDragGhost(remoteDebuggingUrl);
        }
        catch (ChromeDevToolsException)
        {
        }
    }
}
