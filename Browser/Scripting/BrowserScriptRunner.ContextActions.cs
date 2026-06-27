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
        try
        {
            automationClient.Evaluate(remoteDebuggingUrl, BrowserContextScripts.Clear(reset));
        }
        catch (ChromeDevToolsException exception) when (reset && IsExpectedResetNavigation(exception))
        {
        }

        return [$"{(reset ? "CONTEXT_RESET" : "CONTEXT_CLEARED")} {action.LineNumber:000}"];
    }

    private static bool IsExpectedResetNavigation(ChromeDevToolsException exception) =>
        exception.Message.Contains("Inspected target navigated", StringComparison.OrdinalIgnoreCase) ||
        exception.Message.Contains("Execution context was destroyed", StringComparison.OrdinalIgnoreCase);
}
