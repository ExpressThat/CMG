namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteWorkerAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "listworkers" => ListWorkers(remoteDebuggingUrl, automationClient, action),
            "waitforworker" => WaitForWorker(remoteDebuggingUrl, automationClient, action),
            "workerevaluate" => WorkerEvaluate(remoteDebuggingUrl, automationClient, action),
            "workerintercept" => WorkerIntercept(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown worker action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> ListWorkers(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        return automationClient.ListWorkers(remoteDebuggingUrl)
            .Select((worker, index) => $"WORKER {index} id={worker.Id} type={worker.Type} title=\"{worker.Title}\" url=\"{worker.Url}\"")
            .ToArray();
    }

    private static IReadOnlyList<string> WaitForWorker(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        ValidateNetworkUrlMatchOptions(action);
        var mode = (action.Options.GetValueOrDefault("match") ?? "contains").ToLowerInvariant();
        var ignoreCase = !action.Options.ContainsKey("ignoreCase") ||
            (bool.TryParse(action.Options.GetValueOrDefault("ignoreCase"), out var parsedIgnoreCase) && parsedIgnoreCase);
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        do
        {
            var match = automationClient.ListWorkers(remoteDebuggingUrl).FirstOrDefault(worker => WorkerUrlMatches(worker.Url, action.Arguments[0], mode, ignoreCase));
            if (match is not null)
            {
                return [$"WORKER_READY {action.LineNumber:000} id={match.Id} url=\"{match.Url}\""];
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException($"Worker '{action.Arguments[0]}' was not available within {timeout}ms.");
    }

    private static bool WorkerUrlMatches(string url, string pattern, string mode, bool ignoreCase)
    {
        var actual = ignoreCase ? url.ToLowerInvariant() : url;
        var expected = ignoreCase ? pattern.ToLowerInvariant() : pattern;
        return mode switch
        {
            "exact" => actual == expected,
            "regex" => System.Text.RegularExpressions.Regex.IsMatch(
                url,
                pattern,
                ignoreCase ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : System.Text.RegularExpressions.RegexOptions.None),
            _ => actual.Contains(expected, StringComparison.Ordinal)
        };
    }

    private static IReadOnlyList<string> WorkerEvaluate(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var target = action.Options.GetValueOrDefault("target");
        var result = automationClient.EvaluateWorker(remoteDebuggingUrl, target, action.Arguments[0]);
        return [$"WORKER_EVALUATE {action.LineNumber:000} {result}"];
    }

    private static IReadOnlyList<string> WorkerIntercept(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var status = GetIntOption(action, "status", 200);
        ValidateWorkerInterceptOptions(action);
        var body = WorkerRouteBody(action);
        var contentType = action.Options.GetValueOrDefault("contentType") ?? "text/plain";
        var target = action.Options.GetValueOrDefault("target");
        var match = (action.Options.GetValueOrDefault("match") ?? "contains").ToLowerInvariant();
        var ignoreCase = bool.TryParse(action.Options.GetValueOrDefault("ignoreCase"), out var parsedIgnoreCase) && parsedIgnoreCase;
        var route = new WorkerRouteOptions(action.Arguments[0], status, body, contentType, match, ignoreCase, WorkerRouteHeaders(action));
        var count = automationClient.InterceptWorkerRequests(remoteDebuggingUrl, target, route);
        return [$"WORKER_INTERCEPT {action.LineNumber:000} routes={count} {action.Arguments[0]}"];
    }

    private static void ValidateWorkerInterceptOptions(BrowserScriptAction action)
    {
        ValidateNetworkUrlMatchOptions(action);
        ValidateRouteHeaderOptions(action);
        ValidateRouteBodyFile(action);
    }

    private static string WorkerRouteBody(BrowserScriptAction action)
    {
        var bodyFile = action.Options.GetValueOrDefault("bodyFile") ?? action.Options.GetValueOrDefault("file");
        if (!string.IsNullOrWhiteSpace(bodyFile))
        {
            return File.ReadAllText(Path.GetFullPath(bodyFile));
        }

        return action.Options.GetValueOrDefault("body") ?? string.Empty;
    }

    private static IReadOnlyDictionary<string, string>? WorkerRouteHeaders(BrowserScriptAction action)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddWorkerHeader(headers, action.Options.GetValueOrDefault("header"));
        foreach (var header in (action.Options.GetValueOrDefault("headers") ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            AddWorkerHeader(headers, header);
        }

        var name = action.Options.GetValueOrDefault("headerName");
        if (!string.IsNullOrWhiteSpace(name))
        {
            headers[name.Trim()] = action.Options.GetValueOrDefault("headerValue") ?? string.Empty;
        }

        return headers.Count is 0 ? null : headers;
    }

    private static void AddWorkerHeader(Dictionary<string, string> headers, string? header)
    {
        if (string.IsNullOrWhiteSpace(header)) return;
        var index = header.IndexOf(':');
        if (index > 0) headers[header[..index].Trim()] = header[(index + 1)..].Trim();
    }
}
