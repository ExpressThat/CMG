using CMG.Runner;
using System.Text.RegularExpressions;
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

        if (action.Options.TryGetValue("delay", out var delay) &&
            (!int.TryParse(delay, out var parsedDelay) || parsedDelay < 0))
        {
            throw new ScriptExecutionException($"{action.Name} option delay= must be a non-negative integer.");
        }

        ValidateNetworkUrlMatchOptions(action);
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
        ValidateNetworkWaitOptions(action);
        var result = automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.WaitForResponse(ToNode(action)));
        return [$"RESPONSE {action.LineNumber:000} {ParseNetworkWaitResult(result)}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForRequest(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        ValidateNetworkWaitOptions(action);
        var result = automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.WaitForRequest(ToNode(action)));
        return [$"REQUEST {action.LineNumber:000} {ParseNetworkWaitResult(result)}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForRequestFinished(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        ValidateNetworkWaitOptions(action);
        var result = automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.WaitForRequestFinished(ToNode(action)));
        return [$"REQUEST_FINISHED {action.LineNumber:000} {ParseNetworkWaitResult(result)}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForRequestFailed(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        ValidateNetworkWaitOptions(action);
        var result = automationClient.Evaluate(remoteDebuggingUrl, CmgNetworkScripts.WaitForRequestFailed(ToNode(action)));
        return [$"REQUEST_FAILED {action.LineNumber:000} {ParseNetworkWaitResult(result)}"];
    }

    private static void ValidateNetworkWaitOptions(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("timeout", out var timeout) &&
            (!int.TryParse(timeout, out var parsedTimeout) || parsedTimeout < 0))
        {
            throw new ScriptExecutionException($"{action.Name} option timeout= must be a non-negative integer.");
        }

        if (action.Options.TryGetValue("status", out var status) &&
            (!int.TryParse(status, out var parsedStatus) || parsedStatus < 100 || parsedStatus > 999))
        {
            throw new ScriptExecutionException($"{action.Name} option status= must be a numeric HTTP status.");
        }

        if (action.Options.TryGetValue("mocked", out var mocked) &&
            !bool.TryParse(mocked, out _))
        {
            throw new ScriptExecutionException($"{action.Name} option mocked= must be true or false.");
        }

        if (action.Options.TryGetValue("headerValue", out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue) &&
            !action.Options.ContainsKey("header") &&
            !action.Options.ContainsKey("headerName"))
        {
            throw new ScriptExecutionException($"{action.Name} option headerValue= requires header= or headerName=.");
        }

        ValidateNetworkUrlMatchOptions(action);
    }

    private static void ValidateNetworkUrlMatchOptions(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("ignoreCase", out var ignoreCase) &&
            !bool.TryParse(ignoreCase, out _))
        {
            throw new ScriptExecutionException($"{action.Name} option ignoreCase= must be true or false.");
        }

        var match = action.Options.GetValueOrDefault("match") ?? action.Options.GetValueOrDefault("mode");
        if (string.IsNullOrWhiteSpace(match))
        {
            return;
        }

        if (!IsNetworkMatchMode(match))
        {
            throw new ScriptExecutionException($"{action.Name} option match= must be contains, exact, or regex.");
        }

        if (match.Equals("regex", StringComparison.OrdinalIgnoreCase))
        {
            ValidateNetworkRegex(action);
        }
    }

    private static bool IsNetworkMatchMode(string match) =>
        match.Equals("contains", StringComparison.OrdinalIgnoreCase) ||
        match.Equals("exact", StringComparison.OrdinalIgnoreCase) ||
        match.Equals("regex", StringComparison.OrdinalIgnoreCase);

    private static void ValidateNetworkRegex(BrowserScriptAction action)
    {
        try
        {
            _ = new Regex(action.Arguments[0]);
        }
        catch (ArgumentException ex)
        {
            throw new ScriptExecutionException($"Invalid network regex '{action.Arguments[0]}': {ex.Message}");
        }
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
