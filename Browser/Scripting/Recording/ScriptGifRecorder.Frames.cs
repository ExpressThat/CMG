
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

    private void CapturePulseFrame()
    {
        CapturePulseFrame(null);
    }

    private void CapturePulseFrame(BrowserScriptAction? action)
    {
        CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds, PulseStyleFor(action));
    }

    private void CaptureFrame(int delayCentiseconds, ClickPulseStyle? pulseStyle = null)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        devToolsClient.PromoteMessageBar(remoteDebuggingUrl);
        devToolsClient.MoveDomCursor(remoteDebuggingUrl, pointer.Position, pulseStyle);
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

        return options.ClickPulse;
    }

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
