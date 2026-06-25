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
        automationClient.Click(remoteDebuggingUrl, ResolveSelector(remoteDebuggingUrl, automationClient, action));
        return WaitForDownload(action);
    }

    private static IReadOnlyList<string> ExecuteWaitForDownload(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        return WaitForDownload(action);
    }

    private static IReadOnlyList<string> WaitForDownload(BrowserScriptAction action)
    {
        var directory = action.Options.TryGetValue("directory", out var path)
            ? Path.GetFullPath(path)
            : Directory.GetCurrentDirectory();
        var pattern = action.Options.TryGetValue("pattern", out var value) ? value : "*";
        var timeout = GetIntOption(action, "timeout", 5_000);
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);

        do
        {
            var match = FindDownload(directory, pattern);
            if (match is not null)
            {
                return [$"DOWNLOAD {action.LineNumber:000} {match}"];
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException($"No download matching '{pattern}' appeared in '{directory}' within {timeout}ms.");
    }

    private static string? FindDownload(string directory, string pattern)
    {
        if (!Directory.Exists(directory))
        {
            return null;
        }

        return Directory
            .GetFiles(directory, pattern)
            .Where(file => !file.EndsWith(".crdownload", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }
}
