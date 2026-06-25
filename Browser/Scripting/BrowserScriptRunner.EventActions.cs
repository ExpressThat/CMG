namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteWaitForEvent(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 2);
        var eventName = action.Arguments[0].ToLowerInvariant();

        return eventName switch
        {
            "popup" or "page" or "tab" => ExecuteWaitForTab(remoteDebuggingUrl, automationClient, action with { Arguments = [] }),
            "download" => ExecuteWaitForDownload(action with { Arguments = [] }),
            "dialog" => WaitForDialog(remoteDebuggingUrl, automationClient, action with { Arguments = [EventMatcher(action)] }),
            "console" => ExecuteWaitForConsole(remoteDebuggingUrl, automationClient, action with { Arguments = [EventMatcher(action)] }),
            "pageerror" or "page-error" => ExecuteWaitForPageError(remoteDebuggingUrl, automationClient, action with { Arguments = [EventMatcher(action)] }),
            "request" => ExecuteWaitForRequest(remoteDebuggingUrl, automationClient, action with { Arguments = [EventMatcher(action)] }),
            "requestfinished" or "request-finished" =>
                ExecuteWaitForRequestFinished(remoteDebuggingUrl, automationClient, action with { Arguments = [EventMatcher(action)] }),
            "requestfailed" or "request-failed" =>
                ExecuteWaitForRequestFailed(remoteDebuggingUrl, automationClient, action with { Arguments = [EventMatcher(action)] }),
            "response" => ExecuteWaitForResponse(remoteDebuggingUrl, automationClient, action with { Arguments = [EventMatcher(action)] }),
            _ => throw new ScriptExecutionException(
                "waitForEvent supports popup, page, tab, download, dialog, console, pageError, request, requestFinished, requestFailed, and response.")
        };
    }

    private static string EventMatcher(BrowserScriptAction action)
    {
        if (action.Arguments.Count > 1)
        {
            return action.Arguments[1];
        }

        if (action.Options.TryGetValue("pattern", out var pattern))
        {
            return pattern;
        }

        if (action.Options.TryGetValue("text", out var text))
        {
            return text;
        }

        if (action.Options.TryGetValue("message", out var message))
        {
            return message;
        }

        if (action.Options.TryGetValue("url", out var url))
        {
            return url;
        }

        throw new ScriptExecutionException($"waitForEvent {action.Arguments[0]} requires a matcher argument or pattern= option.");
    }
}
