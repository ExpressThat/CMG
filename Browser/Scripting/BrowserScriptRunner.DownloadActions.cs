namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteDownload(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);
        var request = DownloadRequest.From(action);
        var baseline = SnapshotDownloads(request.Directory, request.Pattern);
        automationClient.Click(remoteDebuggingUrl, ResolveSelector(remoteDebuggingUrl, automationClient, action));
        return WaitForDownload(action, request, baseline);
    }

    private static IReadOnlyList<string> ExecuteWaitForDownload(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var request = DownloadRequest.From(action);
        return WaitForDownload(action, request, SnapshotDownloads(request.Directory, request.Pattern));
    }

    private static IReadOnlyList<string> WaitForDownload(
        BrowserScriptAction action,
        DownloadRequest request,
        IReadOnlyDictionary<string, DownloadFileState> baseline)
    {
        var timeout = GetIntOption(action, "timeout", 5_000);
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        var observed = new Dictionary<string, DownloadFileState>(StringComparer.OrdinalIgnoreCase);

        do
        {
            var match = FindCompletedDownload(request.Directory, request.Pattern, baseline, observed);
            if (match is not null)
            {
                return [$"DOWNLOAD {action.LineNumber:000} {match}"];
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException(
            $"No new completed download matching '{request.Pattern}' appeared in '{request.Directory}' within {timeout}ms.");
    }

    private static string? FindCompletedDownload(
        string directory,
        string pattern,
        IReadOnlyDictionary<string, DownloadFileState> baseline,
        IDictionary<string, DownloadFileState> observed)
    {
        foreach (var (path, state) in SnapshotDownloads(directory, pattern)
                     .OrderByDescending(item => item.Value.LastWriteTimeUtc))
        {
            if (baseline.TryGetValue(path, out var original) && original == state) continue;
            if (observed.TryGetValue(path, out var previous) && previous == state) return path;
            observed[path] = state;
        }
        return null;
    }

    private static IReadOnlyDictionary<string, DownloadFileState> SnapshotDownloads(string directory, string pattern)
    {
        if (!Directory.Exists(directory)) return new Dictionary<string, DownloadFileState>();
        return Directory.GetFiles(directory, pattern)
            .Where(path => !PartialDownloadExtensions.Contains(Path.GetExtension(path)))
            .Select(path => new FileInfo(path))
            .ToDictionary(
                file => file.FullName,
                file => new DownloadFileState(file.Length, file.LastWriteTimeUtc),
                StringComparer.OrdinalIgnoreCase);
    }

    private sealed record DownloadRequest(string Directory, string Pattern)
    {
        public static DownloadRequest From(BrowserScriptAction action) => new(
            action.Options.TryGetValue("directory", out var path) ? Path.GetFullPath(path) : CurrentDirectory(),
            action.Options.GetValueOrDefault("pattern") ?? "*");

        private static string CurrentDirectory() => System.IO.Directory.GetCurrentDirectory();
    }

    private readonly record struct DownloadFileState(long Length, DateTime LastWriteTimeUtc);

    private static readonly HashSet<string> PartialDownloadExtensions =
        new([".crdownload", ".part", ".partial", ".download"], StringComparer.OrdinalIgnoreCase);
}
