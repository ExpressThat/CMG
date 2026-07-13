namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private ViewportSize? previousViewport;
    private bool recordingViewportApplied;

    private void ApplyRecordingViewport()
    {
        if (remoteDebuggingUrl is null) return;
        var framing = options.EffectiveFraming;
        if (framing.ViewportWidth is null && framing.PixelRatio == 1d) return;
        previousViewport = devToolsClient.GetViewportSize(remoteDebuggingUrl);
        var width = framing.ViewportWidth ?? (int)Math.Round(previousViewport.Width);
        var height = framing.ViewportHeight ?? (int)Math.Round(previousViewport.Height);
        devToolsClient.SetViewport(remoteDebuggingUrl, new ViewportOptions(width, height, framing.PixelRatio));
        recordingViewportApplied = true;
    }

    private void RestoreRecordingViewport()
    {
        if (!recordingViewportApplied || remoteDebuggingUrl is null || previousViewport is null) return;
        devToolsClient.SetViewport(remoteDebuggingUrl, new ViewportOptions(
            (int)Math.Round(previousViewport.Width),
            (int)Math.Round(previousViewport.Height)));
        recordingViewportApplied = false;
    }
}
