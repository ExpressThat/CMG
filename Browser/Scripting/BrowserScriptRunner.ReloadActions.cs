namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static string ReloadOutput(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string currentUrl)
    {
        if (!TryGetReloadWaitUntil(action, out var waitUntil))
        {
            return $"RELOADED {action.LineNumber:000} {currentUrl}";
        }

        var timeout = GetIntOption(action, "timeout", 5_000);
        if (waitUntil is "commit")
        {
            return $"RELOADED {action.LineNumber:000} {currentUrl} waitUntil=commit";
        }

        var state = waitUntil is "domcontentloaded" ? "interactive" : waitUntil;
        var actual = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.WaitForLoadState(state, timeout));
        return $"RELOADED {action.LineNumber:000} {currentUrl} waitUntil={waitUntil} state={actual}";
    }

    private static bool TryGetReloadWaitUntil(BrowserScriptAction action, out string waitUntil)
    {
        waitUntil = action.Options.GetValueOrDefault("waitUntil") ??
            action.Options.GetValueOrDefault("state") ??
            string.Empty;
        if (string.IsNullOrWhiteSpace(waitUntil))
        {
            return false;
        }

        waitUntil = waitUntil.ToLowerInvariant();
        if (waitUntil is "load" or "domcontentloaded" or "networkidle" or "commit")
        {
            return true;
        }

        throw new ScriptExecutionException("reload waitUntil= expects load, domcontentloaded, networkidle, or commit.");
    }
}
