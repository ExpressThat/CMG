namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteAccessibilitySnapshot(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 1);
        var selector = action.Arguments.Count is 1 ? action.Arguments[0] : null;
        var json = automationClient.Evaluate(remoteDebuggingUrl, BrowserAccessibilityScripts.Snapshot(selector));
        if (action.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output))
        {
            var path = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory());
            File.WriteAllText(path, json);
            return [$"ACCESSIBILITY {action.LineNumber:000} {path}"];
        }

        return [$"ACCESSIBILITY {action.LineNumber:000} {json}"];
    }

    private static IReadOnlyList<string> ExecuteExpectAccessible(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        if (!action.Options.TryGetValue("role", out var role) || string.IsNullOrWhiteSpace(role))
        {
            throw new ScriptExecutionException("expectAccessible requires role=<role>.");
        }

        var name = action.Options.TryGetValue("name", out var nameValue) ? nameValue : string.Empty;
        automationClient.Evaluate(remoteDebuggingUrl, BrowserAccessibilityScripts.Expect(role, name));
        return [$"ACCESSIBLE {action.LineNumber:000} role={role} name=\"{name}\""];
    }
}
