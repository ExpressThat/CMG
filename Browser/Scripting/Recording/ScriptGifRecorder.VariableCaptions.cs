namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public void CaptureVariable(BrowserScriptAction action, string message)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended) return;
        devToolsClient.ShowMessageBar(remoteDebuggingUrl, message, BrowserCaptionOptions.FromOptions(action.Options, action.Name));
        CaptureHoldFrame(CaptionDuration(action), action);
        devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveMessageBar());
    }
}
