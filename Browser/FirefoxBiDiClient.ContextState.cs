using System.Text.Json;
using System.Text;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    private static readonly Dictionary<string, Dictionary<string, BrowserContextInfo>> FirefoxCreatedContexts = [];
    private static readonly Dictionary<string, FirefoxContextSelection> FirefoxActiveContexts = [];
    private static readonly object FirefoxContextLock = new();

    private static IReadOnlyList<FirefoxContext> PrioritizeFirefoxActiveContext(
        string remoteDebuggingUrl,
        IReadOnlyList<FirefoxContext> contexts)
    {
        lock (FirefoxContextLock)
        {
            if (!TryGetFirefoxSelection(remoteDebuggingUrl, out var selection))
            {
                return contexts;
            }

            var activeIndex = ResolveFirefoxSelectionIndex(selection.Index, contexts.Count);
            var active = activeIndex < 0 ? null : contexts[activeIndex];
            return active is null ? contexts : [active, .. contexts.Where(context => context.Id != active.Id)];
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

    private static void SetFirefoxActiveContext(
        string remoteDebuggingUrl,
        FirefoxContext selected,
        IReadOnlyList<FirefoxContext> contexts)
    {
        lock (FirefoxContextLock)
        {
            var index = contexts.TakeWhile(context => context.Id != selected.Id).Count();
            StoreFirefoxSelection(remoteDebuggingUrl, new(index, selected.Url));
        }
    }

    private static void RemoveFirefoxTabSelection(
        string remoteDebuggingUrl,
        FirefoxContext removed,
        IReadOnlyList<FirefoxContext> contexts)
    {
        lock (FirefoxContextLock)
        {
            if (!TryGetFirefoxSelection(remoteDebuggingUrl, out var selection)) return;
            var removedIndex = contexts.TakeWhile(context => context.Id != removed.Id).Count();
            if (selection.Index == removedIndex) ClearFirefoxSelection(remoteDebuggingUrl);
            else if (selection.Index > removedIndex)
                StoreFirefoxSelection(remoteDebuggingUrl, selection with { Index = selection.Index - 1 });
        }
    }

    private static bool TryGetFirefoxSelection(string remoteDebuggingUrl, out FirefoxContextSelection selection)
    {
        selection = null!;
        if (FirefoxActiveContexts.TryGetValue(Key(remoteDebuggingUrl), out selection!)) return true;
        var file = BrowserPaths.GetActiveTargetFile(Key(remoteDebuggingUrl));
        if (!File.Exists(file)) return false;
        var lines = File.ReadAllLines(file);
        if (lines.Length != 2 || !int.TryParse(lines[0], out var index)) return false;
        try
        {
            selection = new(index, Encoding.UTF8.GetString(Convert.FromBase64String(lines[1])));
            FirefoxActiveContexts[Key(remoteDebuggingUrl)] = selection;
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static void StoreFirefoxSelection(string remoteDebuggingUrl, FirefoxContextSelection selection)
    {
        FirefoxActiveContexts[Key(remoteDebuggingUrl)] = selection;
        BrowserPaths.EnsureAppDataDirectory();
        File.WriteAllLines(BrowserPaths.GetActiveTargetFile(Key(remoteDebuggingUrl)),
            [selection.Index.ToString(), Convert.ToBase64String(Encoding.UTF8.GetBytes(selection.Url))]);
    }

    private static void ClearFirefoxSelection(string remoteDebuggingUrl)
    {
        FirefoxActiveContexts.Remove(Key(remoteDebuggingUrl));
        var file = BrowserPaths.GetActiveTargetFile(Key(remoteDebuggingUrl));
        if (File.Exists(file)) File.Delete(file);
    }

    private static string ReadRequired(JsonElement root, IReadOnlyList<string> path) =>
        TryReadString(root, path, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ChromeDevToolsException($"Firefox did not return {string.Join('.', path)}.");

    private static string Key(string remoteDebuggingUrl) => remoteDebuggingUrl.TrimEnd('/');

    internal static int ResolveFirefoxSelectionIndex(int selectedIndex, int contextCount) =>
        selectedIndex >= 0 && selectedIndex < contextCount ? selectedIndex : -1;

    private sealed record FirefoxContextSelection(int Index, string Url);
}
