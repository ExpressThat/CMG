namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    public void OpenTab(string remoteDebuggingUrl, string target) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var created = await session.SendCommand("browsingContext.create", writer => writer.WriteString("type", "tab"));
            var contextId = ReadRequired(created, ["result", "context"]);
            await session.SendCommand("browsingContext.navigate", writer =>
            {
                writer.WriteString("context", contextId);
                writer.WriteString("url", target);
                writer.WriteString("wait", "complete");
            });
            await session.SendCommand("browsingContext.activate", writer => writer.WriteString("context", contextId));
            var contexts = await session.GetTopLevelContexts();
            SetFirefoxActiveContext(remoteDebuggingUrl, contexts.First(context => context.Id == contextId), contexts);
            return true;
        });

    public void ActivateTab(string remoteDebuggingUrl, int index) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetContextAt(remoteDebuggingUrl, index);
            await session.SendCommand("browsingContext.activate", writer => writer.WriteString("context", context.Id));
            var contexts = await session.GetTopLevelContexts();
            SetFirefoxActiveContext(remoteDebuggingUrl, context, contexts);
            return true;
        });

    public void CloseTab(string remoteDebuggingUrl, int index) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetContextAt(remoteDebuggingUrl, index);
            var contexts = await session.GetTopLevelContexts();
            await session.SendCommand("browsingContext.close", writer => writer.WriteString("context", context.Id));
            RemoveFirefoxTabSelection(remoteDebuggingUrl, context, contexts);
            return true;
        });
}
