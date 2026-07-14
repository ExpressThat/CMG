namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private void CleanupDomEvidence()
    {
        if (remoteDebuggingUrl is null) return;
        TryRemoveDomCursor();
        RemovePointerEvidence();
        RemoveAccessibilityEvidence();
        RemoveDebugEvidence();
        RemoveRedactionOverlays();
        try { devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveMessageBar()); }
        catch (ChromeDevToolsException) { }
    }
}
