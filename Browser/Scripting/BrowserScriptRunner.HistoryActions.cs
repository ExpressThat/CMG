namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static string? GetHistoryWaitUntil(BrowserScriptAction action)
    {
        var waitUntil = action.Options.GetValueOrDefault("waitUntil") ??
            action.Options.GetValueOrDefault("state");
        if (string.IsNullOrWhiteSpace(waitUntil))
        {
            return null;
        }

        waitUntil = waitUntil.ToLowerInvariant();
        if (waitUntil is "load" or "domcontentloaded" or "networkidle" or "commit")
        {
            return waitUntil;
        }

        throw new ScriptExecutionException($"{action.Name} waitUntil= expects load, domcontentloaded, networkidle, or commit.");
    }

    private static string HistoryOutput(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string output,
        string url,
        string? waitUntil,
        int timeout)
    {
        if (string.IsNullOrWhiteSpace(waitUntil))
        {
            return $"{output} {action.LineNumber:000} {url}";
        }

        if (waitUntil is "commit")
        {
            return $"{output} {action.LineNumber:000} {url} waitUntil=commit";
        }

        var state = waitUntil is "domcontentloaded" ? "interactive" : waitUntil;
        var actual = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.WaitForLoadState(state, timeout));
        return $"{output} {action.LineNumber:000} {url} waitUntil={waitUntil} state={actual}";
    }
}
