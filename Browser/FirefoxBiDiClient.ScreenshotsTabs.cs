using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    public byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            if (promoteMessageBar)
            {
                await PromoteMessageBar(session, context.Id);
            }

            var response = await session.SendCommand("browsingContext.captureScreenshot", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteString("origin", "viewport");
            });

            return DecodeScreenshot(response);
        });

    public byte[] PrintPdf(string remoteDebuggingUrl, PdfPrintOptions options) =>
        throw new ChromeDevToolsException("PDF generation is not supported for Firefox WebDriver BiDi in CMG yet. Use Chrome or Edge for printPdf.");

    public ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            var rect = await GetElementRect(session, context.Id, selector);
            await EnsurePointInViewport(session, context.Id, selector, rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            return new ElementPoint(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        });

    public ElementBox GetElementBox(string remoteDebuggingUrl, string selector) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            var rect = await GetElementRect(session, context.Id, selector);
            return new ElementBox(rect.X, rect.Y, rect.Width, rect.Height);
        });

    public void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MoveDomCursor(point));

    public void RemoveDomCursor(string remoteDebuggingUrl) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveDomCursor());

    public IReadOnlyList<ChromePageTab> ListTabs(string remoteDebuggingUrl) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var contexts = await session.GetTopLevelContexts();
            var tabs = new List<ChromePageTab>();

            foreach (var context in contexts)
            {
                tabs.Add(new ChromePageTab(context.Id, ReadScriptResultValue(await Evaluate(session, context.Id, "document.title")), context.Url));
            }

            return tabs;
        });

    public void ActivateTab(string remoteDebuggingUrl, int index) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetContextAt(index);
            await session.SendCommand("browsingContext.activate", writer => writer.WriteString("context", context.Id));
            return true;
        });

    public void CloseTab(string remoteDebuggingUrl, int index) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetContextAt(index);
            await session.SendCommand("browsingContext.close", writer => writer.WriteString("context", context.Id));
            return true;
        });

    public IReadOnlyList<BrowserContextInfo> ListBrowserContexts(string remoteDebuggingUrl) =>
        throw UnsupportedContextException();

    public BrowserContextInfo NewBrowserContext(string remoteDebuggingUrl, string initialUrl) =>
        throw UnsupportedContextException();

    public void UseBrowserContext(string remoteDebuggingUrl, string id) =>
        throw UnsupportedContextException();

    public void CloseBrowserContext(string remoteDebuggingUrl, string id) =>
        throw UnsupportedContextException();

    public IReadOnlyList<BrowserWorkerInfo> ListWorkers(string remoteDebuggingUrl) =>
        throw UnsupportedWorkerException();

    public string EvaluateWorker(string remoteDebuggingUrl, string? target, string expression) =>
        throw UnsupportedWorkerException();

    public int InterceptWorkerRequests(string remoteDebuggingUrl, string? target, WorkerRouteOptions options) =>
        throw UnsupportedWorkerException();

    private static ChromeDevToolsException UnsupportedContextException() =>
        new("Isolated browser contexts are not supported for Firefox WebDriver BiDi in CMG yet. Use Chrome or Edge for newContext/useContext/closeContext.");

    private static ChromeDevToolsException UnsupportedWorkerException() =>
        new("Worker target control is not supported for Firefox WebDriver BiDi in CMG yet. Use Chrome or Edge for listWorkers/workerEvaluate/workerIntercept.");
}
