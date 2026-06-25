using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    private static readonly Dictionary<string, Dictionary<string, BrowserContextInfo>> FirefoxCreatedContexts = [];
    private static readonly Dictionary<string, string> FirefoxActiveContexts = [];
    private static readonly object FirefoxContextLock = new();

    private static IReadOnlyList<FirefoxContext> PrioritizeFirefoxActiveContext(
        string remoteDebuggingUrl,
        IReadOnlyList<FirefoxContext> contexts)
    {
        lock (FirefoxContextLock)
        {
            if (!FirefoxActiveContexts.TryGetValue(Key(remoteDebuggingUrl), out var activeId))
            {
                return contexts;
            }

            var active = contexts.FirstOrDefault(context => context.Id == activeId);
            return active is null ? contexts : [active, .. contexts.Where(context => context.Id != activeId)];
        }
    }

    private static void StoreFirefoxContext(string remoteDebuggingUrl, BrowserContextInfo info)
    {
        lock (FirefoxContextLock)
        {
            var contexts = FirefoxCreatedContexts.GetValueOrDefault(Key(remoteDebuggingUrl)) ?? [];
            contexts[info.Id] = info;
            FirefoxCreatedContexts[Key(remoteDebuggingUrl)] = contexts;
        }
    }

    private static IReadOnlyList<BrowserContextInfo> SnapshotFirefoxContexts(string remoteDebuggingUrl)
    {
        lock (FirefoxContextLock)
        {
            return FirefoxCreatedContexts.GetValueOrDefault(Key(remoteDebuggingUrl))?.Values.ToArray() ?? [];
        }
    }

    private static BrowserContextInfo? FindFirefoxContext(string remoteDebuggingUrl, string id) =>
        SnapshotFirefoxContexts(remoteDebuggingUrl).FirstOrDefault(context => context.Id == id || context.TargetId == id);

    private static void RemoveFirefoxContext(string remoteDebuggingUrl, string id)
    {
        lock (FirefoxContextLock)
        {
            if (FirefoxCreatedContexts.TryGetValue(Key(remoteDebuggingUrl), out var contexts))
            {
                contexts.Remove(id);
            }
        }
    }

    private static void SetFirefoxActiveContext(string remoteDebuggingUrl, string contextId)
    {
        lock (FirefoxContextLock)
        {
            FirefoxActiveContexts[Key(remoteDebuggingUrl)] = contextId;
        }
    }

    private static void ClearFirefoxActiveContext(string remoteDebuggingUrl, string contextId)
    {
        lock (FirefoxContextLock)
        {
            if (FirefoxActiveContexts.GetValueOrDefault(Key(remoteDebuggingUrl)) == contextId)
            {
                FirefoxActiveContexts.Remove(Key(remoteDebuggingUrl));
            }
        }
    }

    private static string ReadRequired(JsonElement root, IReadOnlyList<string> path) =>
        TryReadString(root, path, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ChromeDevToolsException($"Firefox did not return {string.Join('.', path)}.");

    private static string Key(string remoteDebuggingUrl) => remoteDebuggingUrl.TrimEnd('/');
}
