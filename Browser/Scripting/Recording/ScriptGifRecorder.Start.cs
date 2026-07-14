namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public void Start(string remoteDebuggingUrl)
    {
        this.remoteDebuggingUrl = remoteDebuggingUrl;
        ApplyRecordingViewport();
        try
        {
            frameSink.SetGeometry(devToolsClient.GetGeometryMetrics(remoteDebuggingUrl));
        }
        catch (ChromeDevToolsException exception)
        {
            frameSink.SetGeometry(new BrowserGeometryMetrics());
            evidenceWarnings.Add($"GIF_WARN_GEOMETRY reason={Quote(exception.Message)} fallback=css-viewport");
        }
    }
}
