namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public int LongWaitCount { get; private set; }
    public int LongWaitMillisecondsSaved { get; private set; }

    private void CaptureWaitAction(BrowserScriptAction action, string name)
    {
        if (action.Arguments.Count == 0 || name is not ("delay" or "waitfortimeout" or "wait")) return;
        if (!int.TryParse(action.Arguments[0], out var milliseconds) || milliseconds <= 0) return;
        CaptureLongWait(action, milliseconds);
    }

    private bool CaptureLongWait(BrowserScriptAction action, int milliseconds)
    {
        var threshold = WaitDurationOption(action, "longWaitThreshold", 2000);
        if (milliseconds < threshold) return false;
        var compress = BoolOption(action, "compressLongWaits", true);
        var encoded = compress ? Math.Min(milliseconds, WaitDurationOption(action, "longWaitDuration", 1200)) : milliseconds;
        var progress = BoolOption(action, "waitProgress", true);
        LongWaitCount++;
        LongWaitMillisecondsSaved += Math.Max(0, milliseconds - encoded);
        try
        {
            for (var phase = 1; phase <= (progress ? 3 : 1); phase++)
            {
                if (progress) devToolsClient.Evaluate(remoteDebuggingUrl!, BrowserDomScripts.ShowGifWaitProgress(milliseconds * phase / 3, milliseconds));
                CaptureFrame(Math.Max(1, (encoded + 9) / 10 / (progress ? 3 : 1)), action: action);
            }
        }
        finally { devToolsClient.Evaluate(remoteDebuggingUrl!, BrowserDomScripts.RemoveGifWaitProgress()); }
        return true;
    }

    private static int WaitDurationOption(BrowserScriptAction action, string name, int fallback) =>
        action.Options.TryGetValue(name, out var value)
            ? ScriptPointerMotionOptions.ParseDuration(value, $"{action.Name} option {name}=") : fallback;
}
