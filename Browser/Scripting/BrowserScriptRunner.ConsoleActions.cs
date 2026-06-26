namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteCaptureConsole(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserConsoleScripts.Install());
        return [$"CONSOLE_CAPTURE {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForConsole(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var level = action.Options.TryGetValue("level", out var value) ? value : string.Empty;
        ValidateConsoleLevel(action, level);
        ValidateTextMatchOptions(action, action.Arguments[0]);
        var result = automationClient.Evaluate(
            remoteDebuggingUrl,
            BrowserConsoleScripts.WaitFor(action.Arguments[0], level, timeout, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        return [$"CONSOLE {action.LineNumber:000} {result}"];
    }

    private static IReadOnlyList<string> ExecuteExpectNoConsole(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 1);
        var timeout = GetIntOption(action, "timeout", 0);
        var level = action.Options.TryGetValue("level", out var levelValue) ? levelValue : "error";
        ValidateConsoleLevel(action, level);
        var text = action.Arguments.FirstOrDefault() ?? string.Empty;
        ValidateTextMatchOptions(action, text);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserConsoleScripts.ExpectNone(text, level, timeout, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        return [$"CONSOLE_OK {action.LineNumber:000} level={level}"];
    }

    private static void ValidateConsoleLevel(BrowserScriptAction action, string level)
    {
        if (string.IsNullOrWhiteSpace(level) || level is "log" or "info" or "warn" or "error")
        {
            return;
        }

        throw new ScriptExecutionException($"{action.Name} option level= must be log, info, warn, or error.");
    }

    private static IReadOnlyList<string> ExecuteCapturePageErrors(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserConsoleScripts.InstallPageErrors());
        return [$"PAGE_ERROR_CAPTURE {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForPageError(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        ValidateTextMatchOptions(action, action.Arguments[0]);
        var result = automationClient.Evaluate(
            remoteDebuggingUrl,
            BrowserConsoleScripts.WaitForPageError(action.Arguments[0], timeout, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        return [$"PAGE_ERROR {action.LineNumber:000} {result}"];
    }

    private static IReadOnlyList<string> ExecuteExpectNoPageError(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 1);
        var timeout = GetIntOption(action, "timeout", 0);
        var text = action.Arguments.FirstOrDefault() ?? string.Empty;
        ValidateTextMatchOptions(action, text);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserConsoleScripts.ExpectNoPageError(text, timeout, EventTextMatchMode(action), GetBoolOption(action, "ignoreCase")));
        return [$"PAGE_ERROR_OK {action.LineNumber:000}"];
    }
}
