using System.Net.WebSockets;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    public IReadOnlyList<byte[]> GetTabScreenshots(
        string remoteDebuggingUrl,
        IReadOnlyList<string>? preparationScripts = null,
        IReadOnlyList<string>? cleanupScripts = null) => Run(async () =>
    {
        var targets = await GetPageTargets(remoteDebuggingUrl);
        var captures = new List<byte[]>(targets.Count);
        Exception? failure = null;
        try
        {
            for (var index = 0; index < targets.Count; index++)
            {
                captures.Add(await CaptureTabWithRetry(targets[index], index, targets.Count,
                    preparationScripts ?? [], cleanupScripts ?? []));
            }
            return captures;
        }
        catch (Exception exception)
        {
            failure = exception;
            throw;
        }
        finally
        {
            if (targets.Count > 1)
            {
                try { await BringTabToFront(targets[0]); }
                catch when (failure is not null) { }
            }
        }
    });

    private static async Task<byte[]> CaptureTabWithRetry(
        PageTarget target,
        int index,
        int count,
        IReadOnlyList<string> preparationScripts,
        IReadOnlyList<string> cleanupScripts)
    {
        Exception? lastFailure = null;
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                return await CaptureTab(target, index, count, preparationScripts, cleanupScripts);
            }
            catch (Exception exception) when (IsTransientTabCaptureFailure(exception))
            {
                lastFailure = exception;
                await RecoverTabCapture(target, cleanupScripts);
                if (attempt < 2) await Task.Delay(100);
            }
        }
        throw new ChromeDevToolsException(
            $"Chrome split-view capture failed for tab {index + 1}/{count} '{target.Title}' after 2 attempts. Reason: {lastFailure?.Message}");
    }

    private static async Task<byte[]> CaptureTab(
        PageTarget target,
        int index,
        int count,
        IReadOnlyList<string> preparationScripts,
        IReadOnlyList<string> cleanupScripts)
    {
        await using var session = await DevToolsSession.Connect(target.WebSocketDebuggerUrl);
        Exception? failure = null;
        try
        {
            if (index > 0)
            {
                await session.SendCommand("Page.bringToFront");
                await EvaluateTabScript(session,
                    "new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");
            }
            foreach (var script in preparationScripts) await EvaluateTabScript(session, script);
            await EvaluateTabScript(session, BrowserDomScripts.ShowGifSplitTabLabel($"Tab {index + 1}/{count}", index == 0));
            var screenshot = await session.SendCommand("Page.captureScreenshot");
            if (!TryReadString(screenshot, "result", "data", out var data) || string.IsNullOrWhiteSpace(data))
                throw new ChromeDevToolsException($"Chrome did not return screenshot data for tab '{target.Title}'.");
            return Convert.FromBase64String(data);
        }
        catch (Exception exception)
        {
            failure = exception;
            throw;
        }
        finally
        {
            try { await CleanupTab(session, cleanupScripts); }
            catch when (failure is not null) { }
        }
    }

    private static async Task RecoverTabCapture(PageTarget target, IReadOnlyList<string> cleanupScripts)
    {
        try
        {
            await using var session = await DevToolsSession.Connect(target.WebSocketDebuggerUrl);
            await CleanupTab(session, cleanupScripts);
        }
        catch (Exception exception) when (IsTransientTabCaptureFailure(exception) || exception is ChromeDevToolsException) { }
    }

    private static async Task CleanupTab(DevToolsSession session, IReadOnlyList<string> cleanupScripts)
    {
        await EvaluateTabScript(session, BrowserDomScripts.RemoveGifSplitTabLabel());
        foreach (var script in cleanupScripts) await EvaluateTabScript(session, script);
    }

    private static async Task BringTabToFront(PageTarget target)
    {
        await using var session = await DevToolsSession.Connect(target.WebSocketDebuggerUrl);
        await session.SendCommand("Page.bringToFront");
    }

    private static async Task EvaluateTabScript(DevToolsSession session, string script)
    {
        var response = await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString("expression", script);
            writer.WriteBoolean("awaitPromise", true);
        });
        if (TryReadString(response, ["result", "exceptionDetails", "text"], out var error) && !string.IsNullOrWhiteSpace(error))
            throw new ChromeDevToolsException(error);
    }

    private static bool IsTransientTabCaptureFailure(Exception exception) =>
        exception is OperationCanceledException or WebSocketException or InvalidOperationException;
}
