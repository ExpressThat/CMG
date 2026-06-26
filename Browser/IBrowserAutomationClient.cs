namespace CMG.Browser;

public interface IBrowserAutomationClient
{
    string GetElementHtml(string remoteDebuggingUrl, string selector);

    byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector, ScreenshotOptions? options = null);

    string Navigate(string remoteDebuggingUrl, string target);

    void WaitForElement(string remoteDebuggingUrl, string selector, int timeoutMilliseconds);

    void Click(string remoteDebuggingUrl, string selector);

    void Type(string remoteDebuggingUrl, string selector, string text);

    void TypeProgressively(string remoteDebuggingUrl, string selector, string text, Action? afterCharacter = null);

    void Clear(string remoteDebuggingUrl, string selector);

    void Press(string remoteDebuggingUrl, string key);

    void KeyDown(string remoteDebuggingUrl, string key);

    void KeyUp(string remoteDebuggingUrl, string key);

    void InsertText(string remoteDebuggingUrl, string text);

    void Hover(string remoteDebuggingUrl, string selector);

    void ScrollElementIntoView(string remoteDebuggingUrl, string selector);

    void Select(string remoteDebuggingUrl, string selector, string value);

    void ShowMessageBar(string remoteDebuggingUrl, string message);

    void PromoteMessageBar(string remoteDebuggingUrl);

    string GetElementText(string remoteDebuggingUrl, string selector);

    string Evaluate(string remoteDebuggingUrl, string expression);

    string AddInitScript(string remoteDebuggingUrl, string source);

    void SetViewport(string remoteDebuggingUrl, ViewportOptions options);

    ViewportSize GetViewportSize(string remoteDebuggingUrl);

    void DragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector);

    void MouseDragAndDrop(
        string remoteDebuggingUrl,
        string sourceSelector,
        string targetSelector,
        IReadOnlyList<ElementPoint> path,
        Action<ElementPoint>? afterMove = null);

    void BeginPageDrag(string remoteDebuggingUrl, string sourceSelector, ElementPoint point);

    void MovePageDrag(string remoteDebuggingUrl, ElementPoint point);

    void EndPageDrag(string remoteDebuggingUrl, ElementPoint point);

    void RemoveDefaultDragGhost(string remoteDebuggingUrl);

    void MoveMouse(string remoteDebuggingUrl, ElementPoint point, int buttons);

    void MouseDown(string remoteDebuggingUrl, ElementPoint point);

    void MouseUp(string remoteDebuggingUrl, ElementPoint point);

    byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true, bool fullPage = false, ScreenshotOptions? options = null);

    byte[] PrintPdf(string remoteDebuggingUrl, PdfPrintOptions options);

    ElementBox GetElementBox(string remoteDebuggingUrl, string selector);

    ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector);

    void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point);

    void RemoveDomCursor(string remoteDebuggingUrl);

    IReadOnlyList<ChromePageTab> ListTabs(string remoteDebuggingUrl);

    void ActivateTab(string remoteDebuggingUrl, int index);

    void CloseTab(string remoteDebuggingUrl, int index);

    IReadOnlyList<BrowserContextInfo> ListBrowserContexts(string remoteDebuggingUrl);

    BrowserContextInfo NewBrowserContext(string remoteDebuggingUrl, string initialUrl);

    void UseBrowserContext(string remoteDebuggingUrl, string id);

    void CloseBrowserContext(string remoteDebuggingUrl, string id);

    IReadOnlyList<BrowserWorkerInfo> ListWorkers(string remoteDebuggingUrl);

    string EvaluateWorker(string remoteDebuggingUrl, string? target, string expression);

    int InterceptWorkerRequests(string remoteDebuggingUrl, string? target, WorkerRouteOptions options);

    void StartCoverage(string remoteDebuggingUrl, CoverageOptions options);

    string StopCoverage(string remoteDebuggingUrl);
}

public sealed class BrowserAutomationClientFactory
{
    private readonly ChromeDevToolsClient chromeDevToolsClient;
    private readonly FirefoxBiDiClient firefoxBiDiClient;

    public BrowserAutomationClientFactory(
        ChromeDevToolsClient chromeDevToolsClient,
        FirefoxBiDiClient firefoxBiDiClient)
    {
        this.chromeDevToolsClient = chromeDevToolsClient;
        this.firefoxBiDiClient = firefoxBiDiClient;
    }

    public IBrowserAutomationClient Create(BrowserKind browserKind) =>
        browserKind.UsesFirefoxBiDi() ? firefoxBiDiClient : chromeDevToolsClient;
}

public sealed record ViewportSize(double Width, double Height);

public sealed record ViewportOptions(
    int Width,
    int Height,
    double DeviceScaleFactor = 1,
    bool IsMobile = false,
    bool HasTouch = false);

public sealed record ElementBox(double X, double Y, double Width, double Height);

public sealed record ScreenshotOptions(
    string Type = "png",
    int? Quality = null,
    bool FullPage = false,
    bool OmitBackground = false);

public sealed record PdfPrintOptions(
    bool Landscape,
    bool PrintBackground,
    double Scale,
    string? Format = null,
    string? Width = null,
    string? Height = null,
    string? MarginTop = null,
    string? MarginRight = null,
    string? MarginBottom = null,
    string? MarginLeft = null,
    string? PageRanges = null,
    bool PreferCssPageSize = false);

public sealed record BrowserContextInfo(string Id, string TargetId, string Url, bool Active);

public sealed record BrowserWorkerInfo(string Id, string Type, string Title, string Url);

public sealed record WorkerRouteOptions(
    string Pattern,
    int Status,
    string Body,
    string ContentType,
    string Match = "contains",
    bool IgnoreCase = false,
    IReadOnlyDictionary<string, string>? Headers = null);

public sealed record CoverageOptions(bool JavaScript, bool Css);
