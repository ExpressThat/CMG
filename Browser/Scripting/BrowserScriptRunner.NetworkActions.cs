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

    private static IReadOnlyList<string> ExecuteExportHar(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var path = RequiredPath(action);
        var json = automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.ExportHar());
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory());
        File.WriteAllText(path, json);
        return [$"HAR_EXPORTED {action.LineNumber:000} {path}"];
    }

    private static IReadOnlyList<string> ExecuteReplayHar(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var path = RequiredPath(action);
        if (!File.Exists(path))
        {
            throw new ScriptExecutionException($"HAR file '{path}' was not found.");
        }

        var count = automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.ReplayHar(File.ReadAllText(path)));
        return [$"HAR_REPLAY {action.LineNumber:000} routes={count} {path}"];
    }

    private static string RequiredPath(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("path", out var path) || string.IsNullOrWhiteSpace(path))
        {
            throw new ScriptExecutionException($"{action.Name} requires path=<file>.");
        }

        return Path.GetFullPath(path);
    }
}
