using System.Text;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteNetworkEnvironmentAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "setextrahttpheaders" or "setheaders" => SetExtraHttpHeaders(remoteDebuggingUrl, automationClient, action),
            "clearextrahttpheaders" or "clearheaders" => ClearExtraHttpHeaders(remoteDebuggingUrl, automationClient, action),
            "sethttpcredentials" or "httpcredentials" or "authenticate" => SetHttpCredentials(remoteDebuggingUrl, automationClient, action),
            "clearhttpcredentials" => ClearHttpCredentials(remoteDebuggingUrl, automationClient, action),
            "setproxy" or "proxy" => SetProxy(remoteDebuggingUrl, automationClient, action),
            "clearproxy" => ClearProxy(remoteDebuggingUrl, automationClient, action),
            "setoffline" => SetOffline(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown network environment action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> SetExtraHttpHeaders(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        if (action.Arguments.Count < 2 || action.Arguments.Count % 2 is not 0)
        {
            throw new ScriptExecutionException($"{action.Name} requires one or more <name> <value> header pairs.");
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < action.Arguments.Count; index += 2)
        {
            var name = action.Arguments[index];
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ScriptExecutionException($"{action.Name} header names cannot be empty.");
            }

            headers[name] = action.Arguments[index + 1];
        }

        var script = BrowserNetworkEnvironmentScripts.ExtraHeaders(headers);
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
        return [$"HEADERS_SET {action.LineNumber:000} {headers.Count}"];
    }

    private static IReadOnlyList<string> ClearExtraHttpHeaders(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var script = BrowserNetworkEnvironmentScripts.ClearExtraHeaders();
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
        return [$"HEADERS_CLEARED {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> SetHttpCredentials(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        if (string.IsNullOrWhiteSpace(action.Arguments[0]))
        {
            throw new ScriptExecutionException($"{action.Name} username cannot be empty.");
        }

        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{action.Arguments[0]}:{action.Arguments[1]}"));
        var script = BrowserNetworkEnvironmentScripts.HttpCredentials($"Basic {token}");
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
        return [$"HTTP_CREDENTIALS_SET {action.LineNumber:000} {action.Arguments[0]}"];
    }

    private static IReadOnlyList<string> ClearHttpCredentials(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var script = BrowserNetworkEnvironmentScripts.ClearHttpCredentials();
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
        return [$"HTTP_CREDENTIALS_CLEARED {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> SetProxy(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        if (string.IsNullOrWhiteSpace(action.Arguments[0]))
        {
            throw new ScriptExecutionException($"{action.Name} proxy prefix cannot be empty.");
        }

        var script = BrowserNetworkEnvironmentScripts.Proxy(action.Arguments[0]);
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
        return [$"PROXY_SET {action.LineNumber:000} {action.Arguments[0]}"];
    }

    private static IReadOnlyList<string> ClearProxy(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var script = BrowserNetworkEnvironmentScripts.ClearProxy();
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
        return [$"PROXY_CLEARED {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> SetOffline(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        if (!bool.TryParse(action.Arguments[0], out var offline))
        {
            throw new ScriptExecutionException("setOffline expects true or false.");
        }

        var script = BrowserNetworkEnvironmentScripts.Offline(offline);
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
        return [$"OFFLINE {action.LineNumber:000} {offline.ToString().ToLowerInvariant()}"];
    }
}
