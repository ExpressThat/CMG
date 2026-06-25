using CMG.Runner;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteWebSocketAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "routewebsocket" => RouteWebSocket(remoteDebuggingUrl, automationClient, action),
            "clearwebsocketroutes" => ClearWebSocketRoutes(remoteDebuggingUrl, automationClient, action),
            "waitforwebsocket" => WaitForWebSocket(remoteDebuggingUrl, automationClient, action),
            "waitforwebsocketmessage" => WaitForWebSocketMessage(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown WebSocket action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> RouteWebSocket(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        ValidateWebSocketOptions(action);
        automationClient.Evaluate(remoteDebuggingUrl, CmgWebSocketScripts.Route(ToNode(action)));
        return [$"WEBSOCKET_ROUTE {action.LineNumber:000} {action.Arguments[0]}"];
    }

    private static IReadOnlyList<string> ClearWebSocketRoutes(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.Evaluate(remoteDebuggingUrl, CmgWebSocketScripts.ClearRoutes());
        return [$"WEBSOCKET_ROUTES_CLEARED {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> WaitForWebSocket(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var result = automationClient.Evaluate(remoteDebuggingUrl, CmgWebSocketScripts.WaitForSocket(ToNode(action)));
        return [$"WEBSOCKET {action.LineNumber:000} {ParseNetworkWaitResult(result)}"];
    }

    private static IReadOnlyList<string> WaitForWebSocketMessage(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var result = automationClient.Evaluate(remoteDebuggingUrl, CmgWebSocketScripts.WaitForMessage(ToNode(action)));
        return [$"WEBSOCKET_MESSAGE {action.LineNumber:000} {ParseNetworkWaitResult(result)}"];
    }

    private static void ValidateWebSocketOptions(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("close", out var close) && !bool.TryParse(close, out _))
        {
            throw new ScriptExecutionException("routeWebSocket option close= must be true or false.");
        }

        if (action.Options.TryGetValue("code", out var code) && (!int.TryParse(code, out var parsed) || parsed < 1000))
        {
            throw new ScriptExecutionException("routeWebSocket option code= must be a WebSocket close code.");
        }
    }
}
