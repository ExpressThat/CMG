using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteListTabs(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        return automationClient
            .ListTabs(remoteDebuggingUrl)
            .Select((tab, index) => $"TAB {index} id={tab.Id} title=\"{tab.Title}\" url=\"{tab.Url}\"")
            .ToArray();
    }

    private static IReadOnlyList<string> ExecuteOpenTab(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var target = NormalizeNavigationTarget(action.Arguments[0]);
        automationClient.Evaluate(remoteDebuggingUrl, $"window.open({QuoteJs(target)}, '_blank')");
        return [$"TAB_OPENED {action.LineNumber:000} {target}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForTab(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var expected = GetIntOption(action, "count", required: true);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        IReadOnlyList<CMG.Browser.ChromePageTab> tabs;

        do
        {
            tabs = automationClient.ListTabs(remoteDebuggingUrl);
            if (tabs.Count >= expected)
            {
                return [$"TAB_COUNT {action.LineNumber:000} {tabs.Count}"];
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException($"Expected at least {expected} tab(s) within {timeout}ms, got {tabs.Count}.");
    }

    private static IReadOnlyList<string> ExecuteActivateTab(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.ActivateTab(remoteDebuggingUrl, GetIntOption(action, "index", required: true));
        return [];
    }

    private static IReadOnlyList<string> ExecuteCloseTab(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.CloseTab(remoteDebuggingUrl, GetIntOption(action, "index", required: true));
        return [];
    }

    private static IReadOnlyList<string> WriteScreenshotOutput(BrowserScriptAction action, byte[] bytes)
    {
        if (action.Options.TryGetValue("output", out var outputPath) && !string.IsNullOrWhiteSpace(outputPath))
        {
            var fullPath = Path.GetFullPath(outputPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(fullPath, bytes);
            return [$"SCREENSHOT {action.LineNumber:000} {fullPath}"];
        }

        return [$"SCREENSHOT {action.LineNumber:000} data:image/png;base64,{Convert.ToBase64String(bytes)}"];
    }

    private static string QuoteJs(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
