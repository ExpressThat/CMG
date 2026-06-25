namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteContextAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var reset = action.Name.Equals("resetContext", StringComparison.OrdinalIgnoreCase);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserContextScripts.Clear(reset));
        return [$"{(reset ? "CONTEXT_RESET" : "CONTEXT_CLEARED")} {action.LineNumber:000}"];
    }
}
