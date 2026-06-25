namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteBrowserContextAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "newcontext" => NewBrowserContext(remoteDebuggingUrl, automationClient, action, context),
            "usecontext" => UseBrowserContext(remoteDebuggingUrl, automationClient, action),
            "listcontexts" => ListBrowserContexts(remoteDebuggingUrl, automationClient, action),
            "closecontext" => CloseBrowserContext(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown context action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> NewBrowserContext(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 0, 1);
        var initialUrl = action.Options.TryGetValue("url", out var url) ? NormalizeNavigationTarget(url) : "about:blank";
        var info = automationClient.NewBrowserContext(remoteDebuggingUrl, initialUrl);
        if (action.Arguments.Count is 1)
        {
            context.Variables[action.Arguments[0]] = info.Id;
        }

        return [$"CONTEXT_CREATED {action.LineNumber:000} id={info.Id} target={info.TargetId} url=\"{info.Url}\""];
    }

    private static IReadOnlyList<string> UseBrowserContext(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        automationClient.UseBrowserContext(remoteDebuggingUrl, action.Arguments[0]);
        return [$"CONTEXT_ACTIVE {action.LineNumber:000} {action.Arguments[0]}"];
    }

    private static IReadOnlyList<string> ListBrowserContexts(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        return automationClient
            .ListBrowserContexts(remoteDebuggingUrl)
            .Select((context, index) => $"CONTEXT {index} id={context.Id} target={context.TargetId} active={context.Active.ToString().ToLowerInvariant()} url=\"{context.Url}\"")
            .ToArray();
    }

    private static IReadOnlyList<string> CloseBrowserContext(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        automationClient.CloseBrowserContext(remoteDebuggingUrl, action.Arguments[0]);
        return [$"CONTEXT_CLOSED {action.LineNumber:000} {action.Arguments[0]}"];
    }
}
