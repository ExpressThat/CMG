
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
            CaptureHoldFrame(options.HoldAfterActionMilliseconds, action);
            return;
        }

        CaptureHoldFrame(ParseHoldMilliseconds(action, value, "holdAfterAction"), action);
    }

    private void CaptureHoldFrame(BrowserScriptAction action, string optionName)
    {
        if (!action.Options.TryGetValue(optionName, out var value))
        {
            CaptureHoldFrame(options.HoldAfterActionMilliseconds, action);
            return;
        }

        CaptureHoldFrame(ParseHoldMilliseconds(action, value, optionName), action);
    }

    private void CaptureOptionalHoldFrame(BrowserScriptAction action, string optionName)
    {
        if (action.Options.TryGetValue(optionName, out var value))
        {
            CaptureHoldFrame(ParseHoldMilliseconds(action, value, optionName), action);
        }
    }

    private void CapturePreClickHoldFrame(BrowserScriptAction action)
    {
        CaptureHoldFrame(HoldMillisecondsFor(action, "preClickHold", options.PreClickHoldMilliseconds), action);
    }

    private void CapturePostClickHoldFrame(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("postClickHold", out var value))
        {
            CaptureHoldFrame(ParseHoldMilliseconds(action, value, "postClickHold"), action);
            return;
        }

        CaptureHoldFrame(action.Options.ContainsKey("holdAfterAction")
            ? HoldMillisecondsFor(action, "holdAfterAction", options.PostClickHoldMilliseconds)
            : options.PostClickHoldMilliseconds, action);
    }

    private void CaptureNavigationHoldFrame(BrowserScriptAction action)
    {
        CaptureHoldFrame(HoldMillisecondsFor(action, "holdAfterNavigation", options.HoldAfterNavigationMilliseconds), action);
    }

    private void CaptureAssertionHoldFrame(BrowserScriptAction action)
    {
        CaptureHoldFrame(HoldMillisecondsFor(action, "holdAfterAssertion", options.HoldAfterAssertionMilliseconds), action);
    }

    public void Pause(BrowserScriptAction action)
    {
        if (action.Arguments.Count is not 1)
        {
            throw new ScriptExecutionException("pauseGif requires milliseconds.");
        }

        CaptureHoldFrame(ParseHoldMilliseconds(action, action.Arguments[0], "milliseconds"), action);
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

    private void CaptureHoldFrame(int milliseconds, BrowserScriptAction? action = null)
    {
        if (milliseconds <= 0)
        {
            return;
        }

        var delay = Math.Max(1, (milliseconds + 9) / 10);
        var evidence = PointerEvidenceFor(action);
        if (evidence.Idle is PointerIdleMode.None || milliseconds < evidence.IdleThresholdMilliseconds || delay < 3)
        {
            CaptureFrame(delay, action: action);
            return;
        }

        try
        {
            for (var phase = 1; phase <= 3; phase++)
            {
                pointerIdlePhase = phase;
                CaptureFrame(delay / 3 + (phase <= delay % 3 ? 1 : 0), action: action);
            }
        }
        finally
        {
            pointerIdlePhase = 0;
        }
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

        frameAction = action;
        try
        {
            var screenshot = CapturePage(promoteMessageBar: false);
            AddCapturedFrame(screenshot, delay, pulseStyle is null ? "frame" : "click-evidence", action);
        }
        finally
        {
            frameAction = null;
        }
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
