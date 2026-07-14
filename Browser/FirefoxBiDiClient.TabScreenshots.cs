namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    public IReadOnlyList<byte[]> GetTabScreenshots(
        string remoteDebuggingUrl,
        IReadOnlyList<string>? preparationScripts = null,
        IReadOnlyList<string>? cleanupScripts = null) => Run(async () =>
    {
        await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
        var contexts = await session.GetTopLevelContexts(remoteDebuggingUrl);
        var captures = new List<byte[]>(contexts.Count);
        for (var index = 0; index < contexts.Count; index++)
        {
            var context = contexts[index];
            try
            {
                foreach (var script in preparationScripts ?? []) _ = ReadScriptResultValue(await Evaluate(session, context.Id, script));
                _ = ReadScriptResultValue(await Evaluate(session, context.Id,
                    BrowserDomScripts.ShowGifSplitTabLabel($"Tab {index + 1}/{contexts.Count}", index == 0)));
                var response = await session.SendCommand("browsingContext.captureScreenshot", writer => writer.WriteString("context", context.Id));
                captures.Add(DecodeScreenshot(response));
            }
            finally
            {
                try { _ = ReadScriptResultValue(await Evaluate(session, context.Id, BrowserDomScripts.RemoveGifSplitTabLabel())); }
                finally { foreach (var script in cleanupScripts ?? []) _ = ReadScriptResultValue(await Evaluate(session, context.Id, script)); }
            }
        }
        return captures;
    });
}
