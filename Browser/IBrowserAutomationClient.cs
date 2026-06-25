namespace CMG.Browser;

public interface IBrowserAutomationClient
{
    string GetElementHtml(string remoteDebuggingUrl, string selector);

    byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector);

    string Navigate(string remoteDebuggingUrl, string target);

    void WaitForElement(string remoteDebuggingUrl, string selector, int timeoutMilliseconds);

    void Click(string remoteDebuggingUrl, string selector);

    void Type(string remoteDebuggingUrl, string selector, string text);

    void TypeProgressively(string remoteDebuggingUrl, string selector, string text, Action? afterCharacter = null);

    void Clear(string remoteDebuggingUrl, string selector);

    void Press(string remoteDebuggingUrl, string key);

    void Hover(string remoteDebuggingUrl, string selector);

    void ScrollElementIntoView(string remoteDebuggingUrl, string selector);

    void Select(string remoteDebuggingUrl, string selector, string value);

    void ShowMessageBar(string remoteDebuggingUrl, string message);

    void PromoteMessageBar(string remoteDebuggingUrl);

    string GetElementText(string remoteDebuggingUrl, string selector);

    string Evaluate(string remoteDebuggingUrl, string expression);

    void SetViewport(string remoteDebuggingUrl, int width, int height);

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

    byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true);

    byte[] PrintPdf(string remoteDebuggingUrl, PdfPrintOptions options);

    ElementBox GetElementBox(string remoteDebuggingUrl, string selector);

    ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector);

    void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point);

    void RemoveDomCursor(string remoteDebuggingUrl);

    IReadOnlyList<ChromePageTab> ListTabs(string remoteDebuggingUrl);

    void ActivateTab(string remoteDebuggingUrl, int index);

    void CloseTab(string remoteDebuggingUrl, int index);
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

public sealed record ElementBox(double X, double Y, double Width, double Height);

public sealed record PdfPrintOptions(bool Landscape, bool PrintBackground, double Scale);
