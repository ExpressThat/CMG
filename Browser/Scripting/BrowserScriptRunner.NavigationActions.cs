namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteNavigationAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "reload" => Reload(remoteDebuggingUrl, automationClient, action),
            "goback" => MoveHistory(remoteDebuggingUrl, automationClient, action, "back", "BACK"),
            "goforward" => MoveHistory(remoteDebuggingUrl, automationClient, action, "forward", "FORWARD"),
            "waitforurl" => WaitForUrl(remoteDebuggingUrl, automationClient, action),
            "expecturl" => ExpectUrl(remoteDebuggingUrl, automationClient, action),
            "expecttitle" => ExpectTitle(remoteDebuggingUrl, automationClient, action),
            "waitforloadstate" => WaitForLoadState(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown navigation action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> Reload(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var currentUrl = automationClient.Evaluate(remoteDebuggingUrl, "location.href");
        automationClient.Evaluate(remoteDebuggingUrl, "location.reload(); location.href");
        Thread.Sleep(100);
        return [$"RELOADED {action.LineNumber:000} {currentUrl}"];
    }

    private static IReadOnlyList<string> MoveHistory(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string direction,
        string output)
    {
        RequireArgumentCount(action, 0, 0);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var url = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.History(direction, timeout));
        return [$"{output} {action.LineNumber:000} {url}"];
    }

    private static IReadOnlyList<string> WaitForUrl(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var url = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.WaitForUrl(action.Arguments[0], timeout));
        return [$"URL {action.LineNumber:000} {url}"];
    }

    private static IReadOnlyList<string> ExpectUrl(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var url = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.ExpectUrl(action.Arguments[0]));
        return [$"URL {action.LineNumber:000} {url}"];
    }

    private static IReadOnlyList<string> ExpectTitle(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var title = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.ExpectTitle(action.Arguments[0]));
        return [$"TITLE {action.LineNumber:000} {title}"];
    }

    private static IReadOnlyList<string> WaitForLoadState(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        if (action.Arguments.Count > 1)
        {
            throw new ScriptExecutionException("waitForLoadState expects at most one state argument.");
        }

        var state = action.Arguments.Count > 0 ? action.Arguments[0] : "load";
        if (state is not ("loading" or "interactive" or "complete" or "load"))
        {
            throw new ScriptExecutionException("waitForLoadState expects loading, interactive, complete, or load.");
        }

        var timeout = GetIntOption(action, "timeout", 5_000);
        var actual = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.WaitForLoadState(state, timeout));
        return [$"LOAD_STATE {action.LineNumber:000} {actual}"];
    }
}
