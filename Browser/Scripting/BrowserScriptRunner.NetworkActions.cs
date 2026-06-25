using CMG.Runner;
using System.Text.Json;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteRoute(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        ValidateRouteOptions(action);
        automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.Route(ToNode(action)));
        return [$"ROUTE {action.LineNumber:000} {action.Arguments[0]}"];
    }

    private static void ValidateRouteOptions(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("times", out var times) &&
            (!int.TryParse(times, out var parsed) || parsed <= 0))
        {
            throw new ScriptExecutionException($"{action.Name} option times= must be a positive integer.");
        }
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
        return [$"RESPONSE {action.LineNumber:000} {ParseNetworkWaitResult(result)}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForRequest(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var result = automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.WaitForRequest(ToNode(action)));
        return [$"REQUEST {action.LineNumber:000} {ParseNetworkWaitResult(result)}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForRequestFailed(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var result = automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.WaitForRequestFailed(ToNode(action)));
        return [$"REQUEST_FAILED {action.LineNumber:000} {ParseNetworkWaitResult(result)}"];
    }

    private static string ParseNetworkWaitResult(string result)
    {
        using var document = JsonDocument.Parse(result);
        var root = document.RootElement;
        if (root.TryGetProperty("success", out var success) && success.GetBoolean())
        {
            return root.TryGetProperty("value", out var value) ? value.GetRawText() : "{}";
        }

        var error = root.TryGetProperty("error", out var reason)
            ? reason.GetString() ?? "Network wait failed."
            : "Network wait failed.";
        throw new ScriptExecutionException(error);
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
