using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    private IReadOnlyList<WorkerTarget> ListPageBridgeWorkers(string remoteDebuggingUrl)
    {
        var json = Evaluate(remoteDebuggingUrl, WorkerBridgeScript("list"));
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "[]" : json);
        return document.RootElement.EnumerateArray()
            .Select(ReadBridgeWorker)
            .Where(worker => worker is not null)
            .Cast<WorkerTarget>()
            .ToArray();
    }

    private bool TryEvaluatePageBridgeWorker(string remoteDebuggingUrl, string? target, string expression, out string result)
    {
        result = string.Empty;
        var workers = ListPageBridgeWorkers(remoteDebuggingUrl);
        var worker = string.IsNullOrWhiteSpace(target)
            ? workers.FirstOrDefault()
            : workers.FirstOrDefault(item =>
                item.Id == target ||
                item.Url.Contains(target, StringComparison.OrdinalIgnoreCase) ||
                item.Title.Contains(target, StringComparison.OrdinalIgnoreCase));
        if (worker is null)
        {
            return false;
        }

        result = Evaluate(remoteDebuggingUrl, WorkerBridgeScript("evaluate", worker.Id, expression));
        return true;
    }

    private bool TryInterceptPageBridgeWorker(
        string remoteDebuggingUrl,
        string? target,
        WorkerRouteOptions options,
        out int count)
    {
        count = 0;
        var workers = ListPageBridgeWorkers(remoteDebuggingUrl);
        var worker = string.IsNullOrWhiteSpace(target)
            ? workers.FirstOrDefault()
            : workers.FirstOrDefault(item =>
                item.Id == target ||
                item.Url.Contains(target, StringComparison.OrdinalIgnoreCase) ||
                item.Title.Contains(target, StringComparison.OrdinalIgnoreCase));
        if (worker is null)
        {
            return false;
        }

        var result = Evaluate(remoteDebuggingUrl, WorkerBridgeScript("route", worker.Id, WorkerRouteJson(options)));
        count = int.TryParse(result, out var parsed) ? parsed : 0;
        return true;
    }

    private static WorkerTarget? ReadBridgeWorker(JsonElement element)
    {
        if (!TryReadString(element, "id", out var id) || string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        _ = TryReadString(element, "type", out var type);
        _ = TryReadString(element, "title", out var title);
        _ = TryReadString(element, "url", out var url);
        return new WorkerTarget(id, type ?? "worker", title ?? string.Empty, url ?? string.Empty);
    }

    private static string WorkerBridgeScript(string command, string? id = null, string? expression = null) => $$"""
    (() => {
      const OriginalWorker = window.__cmgOriginalWorker || window.Worker;
      if (!window.__cmgOriginalWorker) {
        window.__cmgOriginalWorker = OriginalWorker;
        window.__cmgWorkers = [];
        window.Worker = function(url, options) {
          const absoluteUrl = new URL(String(url), location.href).href;
          const id = `cmg-worker-${window.__cmgWorkers.length + 1}`;
          const source = `
            const __cmgRoutes = [];
            const __cmgOriginalFetch = self.fetch.bind(self);
            self.fetch = async (input, init) => {
              const url = typeof input === 'string' ? input : input?.url ?? '';
              const route = __cmgRoutes.find(item => {
                const actual = item.ignoreCase ? String(url).toLowerCase() : String(url);
                const pattern = item.ignoreCase ? String(item.pattern).toLowerCase() : String(item.pattern);
                if (item.match === 'exact') return actual === pattern;
                if (item.match === 'regex') return new RegExp(String(item.pattern), item.ignoreCase ? 'i' : '').test(String(url));
                return actual.includes(pattern);
              });
              return route ? new Response(route.body, { status: route.status, headers: route.headers }) : __cmgOriginalFetch(input, init);
            };
            self.addEventListener('message', async event => {
              const data = event.data || {};
              if (!data.__cmgWorkerCommand) return;
              event.stopImmediatePropagation();
              try {
                if (data.kind === 'evaluate') {
                  const value = await (0, eval)(data.expression);
                  self.postMessage({ __cmgWorkerResult: data.id, ok: true, value: value == null ? '' : String(value) });
                } else if (data.kind === 'route') {
                  __cmgRoutes.push(data.route);
                  self.postMessage({ __cmgWorkerResult: data.id, ok: true, value: String(__cmgRoutes.length) });
                }
              } catch (error) {
                self.postMessage({ __cmgWorkerResult: data.id, ok: false, error: String(error && error.message || error) });
              }
            }, true);
            importScripts(${JSON.stringify(absoluteUrl)});
          `;
          const worker = new OriginalWorker(URL.createObjectURL(new Blob([source], { type: 'text/javascript' })), options);
          window.__cmgWorkers.push({ id, type: 'worker', title: options?.name || '', url: absoluteUrl, worker });
          return worker;
        };
        window.Worker.prototype = OriginalWorker.prototype;
      }
      const command = {{ToJsonStringLiteral(command)}};
      if (command === 'list') return JSON.stringify(window.__cmgWorkers.map(({ worker, ...info }) => info));
      const item = window.__cmgWorkers.find(worker => worker.id === {{ToJsonStringLiteral(id ?? string.Empty)}});
      if (!item) throw new Error('No CMG worker matched {{id}}.');
      const messageId = `cmg-command-${Date.now()}-${Math.random()}`;
      const payload = {
        __cmgWorkerCommand: true,
        id: messageId,
        kind: command === 'route' ? 'route' : 'evaluate',
        expression: {{ToJsonStringLiteral(expression ?? string.Empty)}},
        route: command === 'route' ? JSON.parse({{ToJsonStringLiteral(expression ?? "{}")}}) : undefined
      };
      return new Promise((resolve, reject) => {
        const timeout = setTimeout(() => reject(new Error('Worker command timed out.')), 5000);
        item.worker.addEventListener('message', function handler(event) {
          if (!event.data || event.data.__cmgWorkerResult !== messageId) return;
          item.worker.removeEventListener('message', handler);
          clearTimeout(timeout);
          event.data.ok ? resolve(event.data.value || '') : reject(new Error(event.data.error || 'Worker command failed.'));
        });
        item.worker.postMessage(payload);
      });
    })()
    """;

    private static string WorkerRouteJson(WorkerRouteOptions options)
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

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("pattern", options.Pattern);
            writer.WriteString("match", options.Match);
            writer.WriteBoolean("ignoreCase", options.IgnoreCase);
            writer.WriteNumber("status", options.Status);
            writer.WriteString("body", options.Body);
            writer.WriteStartObject("headers");
            foreach (var header in headers)
            {
                writer.WriteString(header.Key, header.Value);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
