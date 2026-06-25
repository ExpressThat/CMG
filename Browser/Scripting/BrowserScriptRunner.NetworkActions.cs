using CMG.Runner;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteRoute(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.Route(ToNode(action)));
        return [$"ROUTE {action.LineNumber:000} {action.Arguments[0]}"];
    }

    private static IReadOnlyList<string> ExecuteClearRoutes(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.ClearRoutes());
        return [$"ROUTES_CLEARED {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForResponse(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var result = automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.WaitForResponse(ToNode(action)));
        return [$"RESPONSE {action.LineNumber:000} {result}"];
    }
}
