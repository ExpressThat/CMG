namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteEmulate(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        ApplyViewport(remoteDebuggingUrl, automationClient, action);
        var script = BrowserEmulationScript.Build(action.Options);
        if (!string.IsNullOrWhiteSpace(script))
        {
            automationClient.Evaluate(remoteDebuggingUrl, script);
        }

        return [$"EMULATE {action.LineNumber:000} {string.Join(' ', action.Options.Keys)}".TrimEnd()];
    }

    private static void ApplyViewport(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        var hasWidth = action.Options.ContainsKey("width");
        var hasHeight = action.Options.ContainsKey("height");
        if (hasWidth != hasHeight)
        {
            throw new ScriptExecutionException("emulate requires both width and height when overriding viewport.");
        }

        if (hasWidth)
        {
            automationClient.SetViewport(
                remoteDebuggingUrl,
                GetIntOption(action, "width", required: true),
                GetIntOption(action, "height", required: true));
        }
    }
}
