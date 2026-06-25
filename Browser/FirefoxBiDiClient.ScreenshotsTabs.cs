using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    public byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true, bool fullPage = false) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            if (promoteMessageBar)
            {
                await PromoteMessageBar(session, context.Id);
            }

            var response = await session.SendCommand("browsingContext.captureScreenshot", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteString("origin", fullPage ? "document" : "viewport");
            });

            return DecodeScreenshot(response);
        });

    public byte[] PrintPdf(string remoteDebuggingUrl, PdfPrintOptions options) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            var response = await session.SendCommand("browsingContext.print", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteBoolean("background", options.PrintBackground);
                writer.WriteString("orientation", options.Landscape ? "landscape" : "portrait");
                writer.WriteNumber("scale", options.Scale);
            });

            return DecodePdf(response);
        });

    public ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            var rect = await GetElementRect(session, context.Id, selector);
            await EnsurePointInViewport(session, context.Id, selector, rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            return new ElementPoint(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        });

    public ElementBox GetElementBox(string remoteDebuggingUrl, string selector) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
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
            var contexts = await session.GetTopLevelContexts(remoteDebuggingUrl);
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
            var context = await session.GetContextAt(remoteDebuggingUrl, index);
            await session.SendCommand("browsingContext.activate", writer => writer.WriteString("context", context.Id));
            return true;
        });

    public void CloseTab(string remoteDebuggingUrl, int index) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetContextAt(remoteDebuggingUrl, index);
            await session.SendCommand("browsingContext.close", writer => writer.WriteString("context", context.Id));
            return true;
        });

    public IReadOnlyList<BrowserContextInfo> ListBrowserContexts(string remoteDebuggingUrl)
    {
        var contexts = SnapshotFirefoxContexts(remoteDebuggingUrl);
        var active = FirefoxActiveContexts.GetValueOrDefault(Key(remoteDebuggingUrl));
        return contexts.Select(context => context with { Active = context.TargetId == active }).ToArray();
    }

    public BrowserContextInfo NewBrowserContext(string remoteDebuggingUrl, string initialUrl) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var userContext = ReadRequired(await session.SendCommand("browser.createUserContext"), ["result", "userContext"]);
            var created = await session.SendCommand("browsingContext.create", writer =>
            {
                writer.WriteString("type", "tab");
                writer.WriteString("userContext", userContext);
            });
            var contextId = ReadRequired(created, ["result", "context"]);
            if (!initialUrl.Equals("about:blank", StringComparison.OrdinalIgnoreCase))
            {
                await session.SendCommand("browsingContext.navigate", writer =>
                {
                    writer.WriteString("context", contextId);
                    writer.WriteString("url", initialUrl);
                    writer.WriteString("wait", "complete");
                });
            }

            SetFirefoxActiveContext(remoteDebuggingUrl, contextId);
            var info = new BrowserContextInfo(userContext, contextId, initialUrl, Active: true);
            StoreFirefoxContext(remoteDebuggingUrl, info);
            return info;
        });

    public void UseBrowserContext(string remoteDebuggingUrl, string id)
    {
        var context = FindFirefoxContext(remoteDebuggingUrl, id) ??
            throw new ChromeDevToolsException($"Browser context '{id}' was not found in this script run.");
        SetFirefoxActiveContext(remoteDebuggingUrl, context.TargetId);
    }

    public void CloseBrowserContext(string remoteDebuggingUrl, string id)
    {
        var context = FindFirefoxContext(remoteDebuggingUrl, id) ??
            throw new ChromeDevToolsException($"Browser context '{id}' was not found in this script run.");
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            await session.SendCommand("browser.removeUserContext", writer => writer.WriteString("userContext", context.Id));
            RemoveFirefoxContext(remoteDebuggingUrl, context.Id);
            ClearFirefoxActiveContext(remoteDebuggingUrl, context.TargetId);
            return true;
        });
    }

    public IReadOnlyList<BrowserWorkerInfo> ListWorkers(string remoteDebuggingUrl) =>
        throw UnsupportedWorkerException();

    public string EvaluateWorker(string remoteDebuggingUrl, string? target, string expression) =>
        throw UnsupportedWorkerException();

    public int InterceptWorkerRequests(string remoteDebuggingUrl, string? target, WorkerRouteOptions options) =>
        throw UnsupportedWorkerException();

    public void StartCoverage(string remoteDebuggingUrl, CoverageOptions options) =>
        throw UnsupportedCoverageException();

    public string StopCoverage(string remoteDebuggingUrl) =>
        throw UnsupportedCoverageException();

    private static ChromeDevToolsException UnsupportedWorkerException() =>
        new("Worker target control is not supported for Firefox WebDriver BiDi in CMG yet. Use Chrome or Edge for listWorkers/workerEvaluate/workerIntercept.");

    private static ChromeDevToolsException UnsupportedCoverageException() =>
        new("Coverage collection is not supported for Firefox WebDriver BiDi in CMG yet. Use Chrome or Edge for startCoverage/stopCoverage.");
}
