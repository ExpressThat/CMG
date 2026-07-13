
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
        if (redactionCaptureBlocked)
        {
            return;
        }

        CaptureHoldFrame(options.HoldOnFailureMilliseconds);
        var index = timelineSteps.FindLastIndex(step => !step.Success);
        if (index >= 0 && frameSink.FrameCount > 0)
        {
            timelineSteps[index] = timelineSteps[index] with
            {
                EndFrameIndex = frameSink.FrameCount - 1,
                EndTimeMilliseconds = frameSink.DurationMilliseconds,
                FailureFrameIndex = frameSink.FrameCount - 1
            };
        }
    }

    public void ShowPointer(BrowserScriptAction action)
    {
        CaptureFrame(FrameDelayCentisecondsFor(action), forcePointer: true);
    }

    public void HidePointer(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended)
        {
            return;
        }

        devToolsClient.RemoveDomCursor(remoteDebuggingUrl);
        var screenshot = CapturePage(promoteMessageBar: true);
        var delay = ScaleDelay(FrameDelayCentisecondsFor(action));
        AddCapturedFrame(screenshot, delay, "pointer-hidden", action);
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

    private void CaptureFrame(
        int delayCentiseconds,
        ClickPulseStyle? pulseStyle = null,
        BrowserScriptAction? action = null,
        bool forcePointer = false,
        bool sampleEligible = false)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended)
        {
            return;
        }

        var delay = ScaleDelay(delayCentiseconds);
        if (TrySkipCapture(delay, action, sampleEligible)) return;

        devToolsClient.PromoteMessageBar(remoteDebuggingUrl);
        if (forcePointer || ShouldShowPointer(action))
        {
            devToolsClient.MoveDomCursor(remoteDebuggingUrl, pointer.Position, pulseStyle, cursorPressed, cursorTrail, cursorBreadcrumb, cursorVisual);
        }
        else
        {
            devToolsClient.RemoveDomCursor(remoteDebuggingUrl);
        }

        var screenshot = CapturePage(promoteMessageBar: false);
        AddCapturedFrame(screenshot, delay, pulseStyle is null ? "frame" : "click-evidence", action);
    }

    private bool ShouldShowPointer(BrowserScriptAction? action)
    {
        var visibility = options.ShowPointer;
        if (action?.Options.TryGetValue("showPointer", out var value) is true)
        {
            visibility = PointerVisibilityOptions.Parse(value, $"{action.Name} option");
        }

        return visibility is not PointerVisibility.Hidden;
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
