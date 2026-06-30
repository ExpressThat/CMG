using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    public byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true, bool fullPage = false, ScreenshotOptions? options = null) =>
        Run(async () =>
        {
            options ??= new(FullPage: fullPage);
            if (fullPage && !options.FullPage)
            {
                options = options with { FullPage = true };
            }
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            if (promoteMessageBar)
            {
                await PromoteMessageBar(session, context.Id);
            }

            var response = await session.SendCommand("browsingContext.captureScreenshot", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteString("origin", options.FullPage ? "document" : "viewport");
            });

            return ScreenshotImage.ConvertIfNeeded(DecodeScreenshot(response), options);
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

    public void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point, ClickPulseStyle? pulseStyle = null, bool pressed = false, bool trail = false, bool breadcrumb = false, PointerVisualOptions? visual = null) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MoveDomCursor(point, pulseStyle, pressed, trail, breadcrumb, visual));

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
            return true;
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
            throw new ChromeDevToolsException($"Browser context '{id}' was not found.");
        SetFirefoxActiveContext(remoteDebuggingUrl, context.TargetId);
    }

    public void CloseBrowserContext(string remoteDebuggingUrl, string id)
    {
        var context = FindFirefoxContext(remoteDebuggingUrl, id) ??
            throw new ChromeDevToolsException($"Browser context '{id}' was not found.");
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
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            return await ListFirefoxWorkers(session);
        });

    public string EvaluateWorker(string remoteDebuggingUrl, string? target, string expression) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var workers = await ListFirefoxWorkers(session);
            var worker = string.IsNullOrWhiteSpace(target)
                ? workers.FirstOrDefault()
                : workers.FirstOrDefault(worker =>
                    worker.Id == target ||
                    worker.Type.Contains(target, StringComparison.OrdinalIgnoreCase) ||
                    worker.Url.Contains(target, StringComparison.OrdinalIgnoreCase));
            if (worker is null)
            {
                throw new ChromeDevToolsException($"Worker '{target ?? "<first>"}' was not available.");
            }

            return ReadScriptResultValue(await session.SendCommand("script.evaluate", writer =>
            {
                writer.WriteString("expression", expression);
                writer.WriteBoolean("awaitPromise", true);
                writer.WriteString("resultOwnership", "none");
                writer.WriteStartObject("target");
                writer.WriteString("realm", worker.Id);
                writer.WriteEndObject();
            }));
        });

    public int InterceptWorkerRequests(string remoteDebuggingUrl, string? target, WorkerRouteOptions options) =>
        int.Parse(EvaluateWorker(remoteDebuggingUrl, target, BuildFirefoxWorkerInterceptScript(options)), System.Globalization.CultureInfo.InvariantCulture);

    private static async Task<IReadOnlyList<BrowserWorkerInfo>> ListFirefoxWorkers(FirefoxBiDiSession session)
    {
        var response = await session.SendCommand("script.getRealms");
        if (!TryReadElement(response, ["result", "realms"], out var realms) || realms.ValueKind is not JsonValueKind.Array)
        {
            throw new ChromeDevToolsException("Firefox did not return worker realms.");
        }

        return realms.EnumerateArray()
            .Select(ReadWorker)
            .Where(worker => worker is not null)
            .Cast<BrowserWorkerInfo>()
            .ToArray();
    }

    private static BrowserWorkerInfo? ReadWorker(JsonElement realm)
    {
        if (!TryReadString(realm, "type", out var type) || string.IsNullOrWhiteSpace(type) ||
            !type.Contains("worker", StringComparison.OrdinalIgnoreCase) ||
            !TryReadString(realm, "realm", out var id) || string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        _ = TryReadString(realm, "origin", out var origin);
        return new BrowserWorkerInfo(id, type, type, origin ?? string.Empty);
    }

}
