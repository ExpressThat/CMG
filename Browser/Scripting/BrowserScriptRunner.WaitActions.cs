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
        action = NormalizeSelectorArgument(action);
        if (action.Options.TryGetValue("state", out var state))
        {
            return WaitForSelectorState(remoteDebuggingUrl, automationClient, action, state);
        }

        ExecuteWaitForElement(remoteDebuggingUrl, automationClient, action);
        return [$"SELECTOR {action.LineNumber:000} {action.Arguments[0]}"];
    }

    private static IReadOnlyList<string> WaitForSelectorState(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string state)
    {
        RequireArgumentCount(action, 1, 1);
        if (state is not ("attached" or "detached" or "visible" or "hidden"))
        {
            throw new ScriptExecutionException("waitForSelector state= expects attached, detached, visible, or hidden.");
        }

        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        WaitForSelectorStateMatch(remoteDebuggingUrl, automationClient, selector, state, GetIntOption(action, "timeout", 5_000));
        return [$"SELECTOR {action.LineNumber:000} {action.Arguments[0]} state={state}"];
    }

    private static void WaitForSelectorStateMatch(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string selector,
        string state,
        int timeout)
    {
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        var snapshot = new SelectorStateSnapshot(false, false);
        do
        {
            snapshot = ReadSelectorState(automationClient.Evaluate(remoteDebuggingUrl, BrowserWaitScripts.SelectorState(selector)));
            if (SelectorStateMatches(snapshot, state))
            {
                return;
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException(
            $"Selector {selector} did not reach state {state} within {timeout}ms. Last state: {snapshot}.");
    }

    private static SelectorStateSnapshot ReadSelectorState(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        return new SelectorStateSnapshot(
            root.TryGetProperty("attached", out var attached) && attached.GetBoolean(),
            root.TryGetProperty("visible", out var visible) && visible.GetBoolean());
    }

    private static bool SelectorStateMatches(SelectorStateSnapshot snapshot, string state) =>
        state switch
        {
            "attached" => snapshot.Attached,
            "detached" => !snapshot.Attached,
            "visible" => snapshot.Attached && snapshot.Visible,
            "hidden" => !snapshot.Attached || !snapshot.Visible,
            _ => false
        };

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

    private sealed record SelectorStateSnapshot(bool Attached, bool Visible)
    {
        public override string ToString() => $"attached={Attached.ToString().ToLowerInvariant()}, visible={Visible.ToString().ToLowerInvariant()}";
    }
}
