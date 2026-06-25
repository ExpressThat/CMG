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

    private static IReadOnlyList<string> ExecuteSet(BrowserScriptAction action, ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 2, 2);
        context.Variables[action.Arguments[0]] = action.Arguments[1];
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
}
