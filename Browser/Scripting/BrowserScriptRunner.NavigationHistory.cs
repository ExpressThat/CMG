namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static string MoveHistoryAndWaitForUrlChange(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string direction,
        int timeout)
    {
        var start = automationClient.Evaluate(remoteDebuggingUrl, "location.href");
        automationClient.Evaluate(remoteDebuggingUrl, $"setTimeout(() => history.{direction}(), 0); true");
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        Exception? lastException = null;
        do
        {
            try
            {
                var url = automationClient.Evaluate(remoteDebuggingUrl, "location.href");
                if (!string.Equals(url, start, StringComparison.Ordinal))
                {
                    return url;
                }
            }
            catch (Exception exception) when (exception is ChromeDevToolsException or ElementNotFoundException)
            {
                lastException = exception;
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow <= deadline);

        var detail = lastException is null ? $"Last URL: {start}" : $"Last error: {lastException.Message}";
        throw new ScriptExecutionException($"History {direction} did not change URL within {timeout}ms. {detail}");
    }
}
