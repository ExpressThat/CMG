using System.Text.Json;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteWaitAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "waitforselector" => WaitForSelector(remoteDebuggingUrl, automationClient, action),
            "waitforfunction" => WaitForFunction(remoteDebuggingUrl, automationClient, action),
            "waitfortimeout" => WaitForTimeout(action),
            _ => throw new ScriptExecutionException($"Unknown wait action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> WaitForSelector(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        ExecuteWaitForElement(remoteDebuggingUrl, automationClient, action);
        return [$"SELECTOR {action.LineNumber:000} {action.Arguments[0]}"];
    }

    private static IReadOnlyList<string> WaitForFunction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var result = automationClient.Evaluate(remoteDebuggingUrl, BrowserWaitScripts.Function(action.Arguments[0], timeout));
        using var document = JsonDocument.Parse(result);
        var root = document.RootElement;
        if (root.TryGetProperty("success", out var success) && success.GetBoolean())
        {
            var value = root.TryGetProperty("value", out var payload) ? payload.GetString() ?? string.Empty : string.Empty;
            return [$"FUNCTION {action.LineNumber:000} {value}"];
        }

        var error = root.TryGetProperty("error", out var reason)
            ? reason.GetString() ?? "waitForFunction failed."
            : "waitForFunction failed.";
        throw new ScriptExecutionException(error);
    }

    private static IReadOnlyList<string> WaitForTimeout(BrowserScriptAction action)
    {
        ExecuteDelay(action);
        return [$"WAIT_TIMEOUT {action.LineNumber:000} {action.Arguments[0]}"];
    }
}
