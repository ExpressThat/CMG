using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    public IReadOnlyList<BrowserWorkerInfo> ListWorkers(string remoteDebuggingUrl) =>
        Run(async () => MergeWorkerTargets(await GetWorkerTargets(remoteDebuggingUrl), ListPageBridgeWorkers(remoteDebuggingUrl))
            .Select(worker => new BrowserWorkerInfo(worker.Id, worker.Type, worker.Title, worker.Url))
            .ToArray());

    public string EvaluateWorker(string remoteDebuggingUrl, string? target, string expression) =>
        Run(async () =>
        {
            if (TryEvaluatePageBridgeWorker(remoteDebuggingUrl, target, expression, out var bridgeResult))
            {
                return bridgeResult;
            }

            try
            {
                await using var session = await DevToolsSession.Connect(await GetBrowserWebSocketDebuggerUrl(remoteDebuggingUrl), enablePage: false);
                var worker = await FindWorkerTarget(session, target);
                var attach = await session.SendCommand("Target.attachToTarget", writer =>
                {
                    writer.WriteString("targetId", worker.Id);
                    writer.WriteBoolean("flatten", true);
                });
                var sessionId = ReadRequired(attach, ["result", "sessionId"]);
                var response = await session.SendCommand("Runtime.evaluate", writer =>
                {
                    writer.WriteString("expression", expression);
                    writer.WriteBoolean("returnByValue", true);
                    writer.WriteBoolean("awaitPromise", true);
                }, sessionId);

                return ReadScriptResult(response);
            }
            catch (OperationCanceledException)
            {
                throw new ChromeDevToolsException("Worker evaluation timed out.");
            }
        });

    public int InterceptWorkerRequests(string remoteDebuggingUrl, string? target, WorkerRouteOptions options) =>
        TryInterceptPageBridgeWorker(remoteDebuggingUrl, target, options, out var count)
            ? count
            : int.Parse(EvaluateWorker(remoteDebuggingUrl, target, BuildWorkerInterceptScript(options)), CultureInfo.InvariantCulture);

    private static async Task<IReadOnlyList<WorkerTarget>> GetWorkerTargets(string remoteDebuggingUrl)
    {
        await using var session = await DevToolsSession.Connect(await GetBrowserWebSocketDebuggerUrl(remoteDebuggingUrl), enablePage: false);
        return await GetWorkerTargets(session);
    }

    private static async Task<WorkerTarget> FindWorkerTarget(DevToolsSession session, string? target)
    {
        var workers = await GetWorkerTargets(session);
        if (workers.Count is 0)
        {
            throw new ChromeDevToolsException("No worker target is available.");
        }

        return string.IsNullOrWhiteSpace(target)
            ? workers[0]
            : workers.FirstOrDefault(worker => worker.Id == target ||
                worker.Url.Contains(target, StringComparison.OrdinalIgnoreCase) ||
                worker.Title.Contains(target, StringComparison.OrdinalIgnoreCase)) ??
                throw new ChromeDevToolsException($"No worker matched '{target}'.");
    }

    private static IReadOnlyList<WorkerTarget> MergeWorkerTargets(IReadOnlyList<WorkerTarget> first, IReadOnlyList<WorkerTarget> second)
    {
        var workers = first.ToDictionary(worker => worker.Id, StringComparer.OrdinalIgnoreCase);
        foreach (var worker in second)
        {
            workers[worker.Id] = worker;
        }

        return workers.Values.ToArray();
    }

    private static async Task<IReadOnlyList<WorkerTarget>> GetWorkerTargets(DevToolsSession session)
    {
        await session.SendCommand("Target.setDiscoverTargets", writer => writer.WriteBoolean("discover", true));
        var targets = await session.SendCommand("Target.getTargets");
        var workers = new List<WorkerTarget>();

        if (!TryReadElement(targets, ["result", "targetInfos"], out var infos) || infos.ValueKind is not JsonValueKind.Array)
        {
            return workers;
        }

        foreach (var target in infos.EnumerateArray())
        {
            if (!TryReadString(target, "type", out var type) || type is null || !type.Contains("worker", StringComparison.OrdinalIgnoreCase) ||
                !TryReadString(target, "targetId", out var id) || string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            _ = TryReadString(target, "title", out var title);
            _ = TryReadString(target, "url", out var url);
            workers.Add(new WorkerTarget(id, type, title ?? string.Empty, url ?? string.Empty));
        }

        return workers;
    }

    private static string ReadScriptResult(JsonElement response)
    {
        if (TryReadString(response, ["result", "exceptionDetails", "text"], out var exception) && !string.IsNullOrWhiteSpace(exception))
        {
            throw new ChromeDevToolsException(exception);
        }

        if (!TryReadElement(response, ["result", "result"], out var result))
        {
            return string.Empty;
        }

        if (result.TryGetProperty("value", out var value))
        {
            return value.ValueKind is JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
        }

        return TryReadString(result, "description", out var description) ? description ?? string.Empty : string.Empty;
    }

    private static string BuildWorkerInterceptScript(WorkerRouteOptions options) => $$"""
    (() => {
      const routes = globalThis.__cmgWorkerRoutes ||= [];
      routes.push({
        pattern: {{ToJsonStringLiteral(options.Pattern)}},
        match: {{ToJsonStringLiteral(options.Match)}},
        ignoreCase: {{options.IgnoreCase.ToString().ToLowerInvariant()}},
        status: {{options.Status}},
        body: {{ToJsonStringLiteral(options.Body)}},
        contentType: {{ToJsonStringLiteral(options.ContentType)}},
        headers: {{WorkerHeaders(options)}}
      });
      if (!globalThis.__cmgOriginalFetch) {
        globalThis.__cmgOriginalFetch = globalThis.fetch?.bind(globalThis);
        globalThis.fetch = async (input, init) => {
          const url = typeof input === 'string' ? input : input?.url ?? '';
          const route = routes.find(item => {
            const actual = item.ignoreCase ? String(url).toLowerCase() : String(url);
            const pattern = item.ignoreCase ? String(item.pattern).toLowerCase() : String(item.pattern);
            if (item.match === 'exact') return actual === pattern;
            if (item.match === 'regex') return new RegExp(String(item.pattern), item.ignoreCase ? 'i' : '').test(String(url));
            return actual.includes(pattern);
          });
          return route ? new Response(route.body, { status: route.status, headers: route.headers }) : globalThis.__cmgOriginalFetch(input, init);
        };
      }
      return routes.length.toString();
    })()
    """;

    private static string WorkerHeaders(WorkerRouteOptions options)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["content-type"] = options.ContentType
        };
        if (options.Headers is not null)
        {
            foreach (var header in options.Headers)
            {
                headers[header.Key.ToLowerInvariant()] = header.Value;
            }
        }

        return "{" + string.Join(",", headers.Select(header => $"{ToJsonStringLiteral(header.Key)}:{ToJsonStringLiteral(header.Value)}")) + "}";
    }

    private sealed record WorkerTarget(string Id, string Type, string Title, string Url);
}
