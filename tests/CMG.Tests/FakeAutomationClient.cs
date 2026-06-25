using CMG.Browser;

namespace CMG.Tests;

internal sealed class FakeAutomationClient : IBrowserAutomationClient
{
    public string LastExpression { get; private set; } = string.Empty;
    public string LastInitScript { get; private set; } = string.Empty;
    public string LastClickedSelector { get; private set; } = string.Empty;
    public ViewportSize? LastViewport { get; private set; }
    public PdfPrintOptions? LastPdfOptions { get; private set; }
    public Queue<string> TextResponses { get; } = new();
    public Queue<string> EvaluateResponses { get; } = new();
    public Queue<IReadOnlyList<ChromePageTab>> TabResponses { get; } = new();
    public List<BrowserContextInfo> BrowserContexts { get; } = [];
    public List<BrowserWorkerInfo> Workers { get; } = [];
    public string ActiveBrowserContext { get; private set; } = string.Empty;
    public WorkerRouteOptions? LastWorkerRoute { get; private set; }
    public CoverageOptions? LastCoverageOptions { get; private set; }
    public ElementPoint? LastMouseMove { get; private set; }
    public ElementPoint? LastMouseDown { get; private set; }
    public ElementPoint? LastMouseUp { get; private set; }
    public string LastKeyDown { get; private set; } = string.Empty;
    public string LastKeyUp { get; private set; } = string.Empty;
    public string LastInsertedText { get; private set; } = string.Empty;

    public string GetElementHtml(string remoteDebuggingUrl, string selector) => string.Empty;
    public byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector) => [];
    public string Navigate(string remoteDebuggingUrl, string target) => target;
    public void WaitForElement(string remoteDebuggingUrl, string selector, int timeoutMilliseconds) { }
    public void Click(string remoteDebuggingUrl, string selector) => LastClickedSelector = selector;
    public void Type(string remoteDebuggingUrl, string selector, string text) { }
    public void TypeProgressively(string remoteDebuggingUrl, string selector, string text, Action? afterCharacter = null) { }
    public void Clear(string remoteDebuggingUrl, string selector) { }
    public void Press(string remoteDebuggingUrl, string key) { }
    public void KeyDown(string remoteDebuggingUrl, string key) => LastKeyDown = key;
    public void KeyUp(string remoteDebuggingUrl, string key) => LastKeyUp = key;
    public void InsertText(string remoteDebuggingUrl, string text) => LastInsertedText = text;
    public void Hover(string remoteDebuggingUrl, string selector) { }
    public void ScrollElementIntoView(string remoteDebuggingUrl, string selector) { }
    public void Select(string remoteDebuggingUrl, string selector, string value) { }
    public void ShowMessageBar(string remoteDebuggingUrl, string message) { }
    public void PromoteMessageBar(string remoteDebuggingUrl) { }
    public string GetElementText(string remoteDebuggingUrl, string selector) =>
        TextResponses.Count > 0 ? TextResponses.Dequeue() : string.Empty;
    public string Evaluate(string remoteDebuggingUrl, string expression)
    {
        LastExpression = expression;
        return EvaluateResponses.Count > 0 ? EvaluateResponses.Dequeue() : "{}";
    }
    public string AddInitScript(string remoteDebuggingUrl, string source)
    {
        LastInitScript = source;
        return "init-1";
    }
    public void SetViewport(string remoteDebuggingUrl, int width, int height) => LastViewport = new(width, height);
    public ViewportSize GetViewportSize(string remoteDebuggingUrl) => new(800, 600);
    public void DragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector) { }
    public void MouseDragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector, IReadOnlyList<ElementPoint> path, Action<ElementPoint>? afterMove = null) { }
    public void BeginPageDrag(string remoteDebuggingUrl, string sourceSelector, ElementPoint point) { }
    public void MovePageDrag(string remoteDebuggingUrl, ElementPoint point) { }
    public void EndPageDrag(string remoteDebuggingUrl, ElementPoint point) { }
    public void RemoveDefaultDragGhost(string remoteDebuggingUrl) { }
    public void MoveMouse(string remoteDebuggingUrl, ElementPoint point, int buttons) => LastMouseMove = point;
    public void MouseDown(string remoteDebuggingUrl, ElementPoint point) => LastMouseDown = point;
    public void MouseUp(string remoteDebuggingUrl, ElementPoint point) => LastMouseUp = point;
    public byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true) => [];
    public byte[] PrintPdf(string remoteDebuggingUrl, PdfPrintOptions options)
    {
        LastPdfOptions = options;
        return [1, 2, 3];
    }
    public ElementBox GetElementBox(string remoteDebuggingUrl, string selector) => new(0, 0, 1, 1);
    public ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector) => new(0, 0);
    public void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point) { }
    public void RemoveDomCursor(string remoteDebuggingUrl) { }
    public IReadOnlyList<ChromePageTab> ListTabs(string remoteDebuggingUrl) =>
        TabResponses.Count > 0 ? TabResponses.Dequeue() : [];
    public void ActivateTab(string remoteDebuggingUrl, int index) { }
    public void CloseTab(string remoteDebuggingUrl, int index) { }
    public IReadOnlyList<BrowserContextInfo> ListBrowserContexts(string remoteDebuggingUrl) => BrowserContexts;
    public BrowserContextInfo NewBrowserContext(string remoteDebuggingUrl, string initialUrl)
    {
        var info = new BrowserContextInfo($"context-{BrowserContexts.Count + 1}", $"target-{BrowserContexts.Count + 1}", initialUrl, Active: true);
        BrowserContexts.Add(info);
        ActiveBrowserContext = info.Id;
        return info;
    }
    public void UseBrowserContext(string remoteDebuggingUrl, string id) => ActiveBrowserContext = id;
    public void CloseBrowserContext(string remoteDebuggingUrl, string id) => BrowserContexts.RemoveAll(context => context.Id == id || context.TargetId == id);
    public IReadOnlyList<BrowserWorkerInfo> ListWorkers(string remoteDebuggingUrl) => Workers;
    public string EvaluateWorker(string remoteDebuggingUrl, string? target, string expression) => $"worker:{expression}";
    public int InterceptWorkerRequests(string remoteDebuggingUrl, string? target, WorkerRouteOptions options)
    {
        LastWorkerRoute = options;
        return 1;
    }
    public void StartCoverage(string remoteDebuggingUrl, CoverageOptions options) => LastCoverageOptions = options;
    public string StopCoverage(string remoteDebuggingUrl) => """{"js":[],"css":[]}""";
}
