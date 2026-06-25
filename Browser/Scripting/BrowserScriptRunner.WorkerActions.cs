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
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        do
        {
            var match = automationClient.ListWorkers(remoteDebuggingUrl).FirstOrDefault(worker => worker.Url.Contains(action.Arguments[0], StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return [$"WORKER_READY {action.LineNumber:000} id={match.Id} url=\"{match.Url}\""];
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException($"Worker '{action.Arguments[0]}' was not available within {timeout}ms.");
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
        var body = action.Options.GetValueOrDefault("body") ?? string.Empty;
        var contentType = action.Options.GetValueOrDefault("contentType") ?? "text/plain";
        var target = action.Options.GetValueOrDefault("target");
        var count = automationClient.InterceptWorkerRequests(remoteDebuggingUrl, target, new WorkerRouteOptions(action.Arguments[0], status, body, contentType));
        return [$"WORKER_INTERCEPT {action.LineNumber:000} routes={count} {action.Arguments[0]}"];
    }
}
