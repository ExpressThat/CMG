namespace CMG.Browser;

public static class BrowserEndpointProbe
{
    public static bool WaitUntilReady(BrowserKind browserKind, string remoteDebuggingUrl, TimeSpan timeout)
    {
        if (!browserKind.UsesChromiumDevTools())
        {
            return true;
        }

        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        do
        {
            if (TryProbe(remoteDebuggingUrl))
            {
                return true;
            }

            Thread.Sleep(100);
        }
        while (DateTimeOffset.UtcNow <= deadline);

        return false;
    }

    private static bool TryProbe(string remoteDebuggingUrl)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var json = httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json").GetAwaiter().GetResult();
            return json.Contains("webSocketDebuggerUrl", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            return false;
        }
    }
}
