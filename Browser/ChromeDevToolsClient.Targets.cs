using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    private async Task<Uri?> TryFindPageWithSelector(string remoteDebuggingUrl, string selector)
    {
        var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

        foreach (var pageTarget in pageTargets)
        {
            await using var session = await DevToolsSession.Connect(pageTarget);
            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", $"Boolean(document.querySelector({ToJsonStringLiteral(selector)}))");
                writer.WriteBoolean("returnByValue", true);
            });

            if (TryReadBoolean(response, ["result", "result", "value"], out var exists) && exists)
            {
                return pageTarget;
            }
        }

        return null;
    }

    private static async Task<DevToolsSession> OpenPrimaryPageSession(string remoteDebuggingUrl)
    {
        var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

        return await DevToolsSession.Connect(pageTargets[0]);
    }

    private static async Task WaitForPagePaint(DevToolsSession session)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow <= deadline)
        {
            var response = await session.SendCommand("Runtime.evaluate", writer =>
            {
                writer.WriteString("expression", "document.readyState === 'complete' || document.readyState === 'interactive'");
                writer.WriteBoolean("returnByValue", true);
            });

            if (TryReadBoolean(response, ["result", "result", "value"], out var ready) && ready)
            {
                break;
            }

            await Task.Delay(PollInterval);
        }

        await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString(
                "expression",
                "new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");
            writer.WriteBoolean("awaitPromise", true);
        });
    }

    private static async Task<string> GetCurrentPageUrl(DevToolsSession session)
    {
        var response = await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString("expression", "location.href");
            writer.WriteBoolean("returnByValue", true);
        });

        return TryReadString(response, ["result", "result", "value"], out var url) && url is not null
            ? url
            : string.Empty;
    }

    private static async Task<IReadOnlyList<Uri>> GetPageWebSocketDebuggerUrls(string remoteDebuggingUrl)
    {
        return (await GetPageTargets(remoteDebuggingUrl))
            .Select(target => target.WebSocketDebuggerUrl)
            .ToArray();
    }

    private static async Task<IReadOnlyList<PageTarget>> GetPageTargets(string remoteDebuggingUrl)
    {
        using var httpClient = new HttpClient
        {
            Timeout = CommandTimeout
        };

        var targetsJson = await httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json");
        using var targets = JsonDocument.Parse(targetsJson);
        var pageTargets = new List<PageTarget>();

        foreach (var target in targets.RootElement.EnumerateArray())
        {
            if (!TryReadString(target, "type", out var type) ||
                !string.Equals(type, "page", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryReadString(target, "webSocketDebuggerUrl", out var webSocketDebuggerUrl) &&
                !string.IsNullOrWhiteSpace(webSocketDebuggerUrl) &&
                TryReadString(target, "id", out var id) &&
                !string.IsNullOrWhiteSpace(id))
            {
                _ = TryReadString(target, "title", out var title);
                _ = TryReadString(target, "url", out var url);
                pageTargets.Add(new PageTarget(id, title ?? string.Empty, url ?? string.Empty, new Uri(webSocketDebuggerUrl)));
            }
        }

        if (pageTargets.Count is 0)
        {
            throw new ChromeDevToolsException("No Chrome page target was available through remote debugging.");
        }

        return pageTargets;
    }

    private static async Task<PageTarget> GetPageTargetAt(string remoteDebuggingUrl, int index)
    {
        var targets = await GetPageTargets(remoteDebuggingUrl);
        if (index < 0 || index >= targets.Count)
        {
            throw new ChromeDevToolsException($"Tab index {index} does not exist. Available tab count: {targets.Count}.");
        }

        return targets[index];
    }
}
