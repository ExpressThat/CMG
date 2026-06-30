
namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private void CaptureHoldFrame()
    {
        CaptureFrame(ScriptRecordingOptions.HoldFrameDelayCentiseconds);
    }

    private void CaptureHoldFrame(BrowserScriptAction action, string optionName)
    {
        if (!action.Options.TryGetValue(optionName, out var value))
        {
            CaptureHoldFrame();
            return;
        }

        CaptureFrame(Math.Max(1, ScriptPointerMotionOptions.ParseDuration(value, $"{action.Name} option {optionName}=") / 10));
    }

    private void CapturePulseFrame()
    {
        CaptureFrame(ScriptRecordingOptions.FrameDelayCentiseconds, pulse: true);
    }

    private void CaptureFrame(int delayCentiseconds, bool pulse = false)
    {
        if (remoteDebuggingUrl is null)
        {
            return;
        }

        devToolsClient.PromoteMessageBar(remoteDebuggingUrl);
        devToolsClient.MoveDomCursor(remoteDebuggingUrl, pointer.Position);
        var screenshot = devToolsClient.GetPageScreenshot(remoteDebuggingUrl, promoteMessageBar: false);
        frameSink.AddFrame(screenshot, delayCentiseconds);
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
