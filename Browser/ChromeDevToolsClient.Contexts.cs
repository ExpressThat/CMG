using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    private static readonly Dictionary<string, Dictionary<string, BrowserContextInfo>> CreatedContexts = [];
    private static readonly object ContextLock = new();

    public IReadOnlyList<BrowserContextInfo> ListBrowserContexts(string remoteDebuggingUrl)
    {
        var contexts = SnapshotContexts(remoteDebuggingUrl);
        var activeTarget = ActiveTargets.GetValueOrDefault(Key(remoteDebuggingUrl));
        return contexts
            .Select(context => context with { Active = context.TargetId == activeTarget })
            .ToArray();
    }

    public BrowserContextInfo NewBrowserContext(string remoteDebuggingUrl, string initialUrl) =>
        Run(async () =>
        {
            await using var session = await DevToolsSession.Connect(await GetBrowserWebSocketDebuggerUrl(remoteDebuggingUrl), enablePage: false);
            var contextId = ReadRequired(await session.SendCommand("Target.createBrowserContext"), ["result", "browserContextId"]);
            var target = await session.SendCommand("Target.createTarget", writer =>
            {
                writer.WriteString("url", initialUrl);
                writer.WriteString("browserContextId", contextId);
            });
            var targetId = ReadRequired(target, ["result", "targetId"]);
            SetActiveTarget(remoteDebuggingUrl, targetId);
            var info = new BrowserContextInfo(contextId, targetId, initialUrl, Active: true);
            StoreContext(remoteDebuggingUrl, info);
            return info;
        });

    public void UseBrowserContext(string remoteDebuggingUrl, string id)
    {
        var context = FindContext(remoteDebuggingUrl, id) ??
            throw new ChromeDevToolsException($"Browser context '{id}' was not found in this script run.");
        SetActiveTarget(remoteDebuggingUrl, context.TargetId);
    }

    public void CloseBrowserContext(string remoteDebuggingUrl, string id)
    {
        var context = FindContext(remoteDebuggingUrl, id) ??
            throw new ChromeDevToolsException($"Browser context '{id}' was not found in this script run.");
        Run(async () =>
        {
            await using var session = await DevToolsSession.Connect(await GetBrowserWebSocketDebuggerUrl(remoteDebuggingUrl), enablePage: false);
            await session.SendCommand("Target.disposeBrowserContext", writer => writer.WriteString("browserContextId", context.Id));
            RemoveContext(remoteDebuggingUrl, context.Id);
            ClearActiveTarget(remoteDebuggingUrl, context.TargetId);
            return true;
        });
    }

    private static async Task<Uri> GetBrowserWebSocketDebuggerUrl(string remoteDebuggingUrl)
    {
        using var httpClient = new HttpClient { Timeout = CommandTimeout };
        using var document = JsonDocument.Parse(await httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json/version"));
        var url = ReadRequired(document.RootElement, ["webSocketDebuggerUrl"]);
        return new Uri(url);
    }

    private static string ReadRequired(JsonElement root, IReadOnlyList<string> path) =>
        TryReadString(root, path, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ChromeDevToolsException($"Chrome did not return {string.Join('.', path)}.");

    private static void StoreContext(string remoteDebuggingUrl, BrowserContextInfo info)
    {
        lock (ContextLock)
        {
            var contexts = CreatedContexts.GetValueOrDefault(Key(remoteDebuggingUrl)) ?? [];
            contexts[info.Id] = info;
            CreatedContexts[Key(remoteDebuggingUrl)] = contexts;
        }
    }

    private static IReadOnlyList<BrowserContextInfo> SnapshotContexts(string remoteDebuggingUrl)
    {
        lock (ContextLock)
        {
            return CreatedContexts.GetValueOrDefault(Key(remoteDebuggingUrl))?.Values.ToArray() ?? [];
        }
    }

    private static BrowserContextInfo? FindContext(string remoteDebuggingUrl, string id) =>
        SnapshotContexts(remoteDebuggingUrl).FirstOrDefault(context => context.Id == id || context.TargetId == id);

    private static void RemoveContext(string remoteDebuggingUrl, string id)
    {
        lock (ContextLock)
        {
            if (CreatedContexts.TryGetValue(Key(remoteDebuggingUrl), out var contexts))
            {
                contexts.Remove(id);
            }
        }
    }
}
