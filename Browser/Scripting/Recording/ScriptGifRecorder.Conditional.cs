namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public string VisualSignature()
    {
        if (remoteDebuggingUrl is null) return string.Empty;
        CleanupDomEvidence();
        var screenshot = devToolsClient.GetPageScreenshot(remoteDebuggingUrl, promoteMessageBar: true);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(screenshot));
    }

    public void Snapshot(BrowserScriptAction action, string name)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended) return;
        RecordCheckpoint(action, name);
        var duration = action.Options.TryGetValue("duration", out var value)
            ? ScriptPointerMotionOptions.ParseDuration(value, $"{action.Name} option duration=")
            : options.HoldAfterActionMilliseconds;
        CaptureHoldFrame(duration, action);
    }

    public void Discard()
    {
        if (remoteDebuggingUrl is not null)
        {
            CleanupDomEvidence();
        }
        RestoreRecordingViewport();
    }
}
