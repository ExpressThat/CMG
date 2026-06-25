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
        var result = automationClient.Evaluate(
            remoteDebuggingUrl,
            BrowserConsoleScripts.WaitFor(action.Arguments[0], level, timeout));
        return [$"CONSOLE {action.LineNumber:000} {result}"];
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
        var result = automationClient.Evaluate(remoteDebuggingUrl, BrowserConsoleScripts.WaitForPageError(action.Arguments[0], timeout));
        return [$"PAGE_ERROR {action.LineNumber:000} {result}"];
    }
}
