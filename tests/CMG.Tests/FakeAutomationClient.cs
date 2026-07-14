using CMG.Browser;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

internal sealed class FakeAutomationClient : IBrowserAutomationClient
{
    public string LastExpression { get; private set; } = string.Empty;
    public List<string> EvaluatedExpressions { get; } = [];
    public string LastInitScript { get; private set; } = string.Empty;
    public string LastDiagnosticsUrl { get; private set; } = string.Empty;
    public string LastClickedSelector { get; private set; } = string.Empty;
    public int ClickCount { get; private set; }
    public string LastHoveredSelector { get; private set; } = string.Empty;
    public string LastWaitSelector { get; private set; } = string.Empty;
    public int LastWaitTimeout { get; private set; }
    public string LastTypedSelector { get; private set; } = string.Empty;
    public string LastTypedText { get; private set; } = string.Empty;
    public int LastTypeDelay { get; private set; }
    public string LastClearedSelector { get; private set; } = string.Empty;
    public string LastSelectedSelector { get; private set; } = string.Empty;
    public string LastSelectedValue { get; private set; } = string.Empty;
    public string LastMessageBar { get; private set; } = string.Empty;
    public List<string> MessageBars { get; } = [];
    public BrowserCaptionOptions? LastCaptionOptions { get; private set; }
    public string LastElementTextSelector { get; private set; } = string.Empty;
    public string LastElementBoxSelector { get; private set; } = string.Empty;
    public string LastElementCenterSelector { get; private set; } = string.Empty;
    public ViewportSize? LastViewport { get; private set; }
    public ViewportOptions? LastViewportOptions { get; private set; }
    public List<ViewportOptions> ViewportOptionsHistory { get; } = [];
    public string LastScrolledSelector { get; private set; } = string.Empty;
    public PdfPrintOptions? LastPdfOptions { get; private set; }
    public Queue<string> TextResponses { get; } = new();
    public Queue<string> EvaluateResponses { get; } = new();
    public Queue<ElementBox> ElementBoxes { get; } = new();
    public Queue<byte[]> PageScreenshotResponses { get; } = new();
    public Queue<IReadOnlyList<ChromePageTab>> TabResponses { get; } = new();
    public string LastOpenedTab { get; private set; } = string.Empty;
    public List<BrowserContextInfo> BrowserContexts { get; } = [];
    public List<BrowserWorkerInfo> Workers { get; } = [];
    public string ActiveBrowserContext { get; private set; } = string.Empty;
    public WorkerRouteOptions? LastWorkerRoute { get; private set; }
    public CoverageOptions? LastCoverageOptions { get; private set; }
    public ScreenshotOptions? LastElementScreenshotOptions { get; private set; }
    public ScreenshotOptions? LastPageScreenshotOptions { get; private set; }
    public List<ScreenshotOptions> PageScreenshotOptions { get; } = [];
    public ElementPoint? LastMouseMove { get; private set; }
    public ElementPoint? LastMouseDown { get; private set; }
    public ElementPoint? LastMouseUp { get; private set; }
    public ElementPoint? LastBeginDragPoint { get; private set; }
    public ElementPoint? LastMoveDragPoint { get; private set; }
    public ElementPoint? LastEndDragPoint { get; private set; }
    public string LastDragSource { get; private set; } = string.Empty;
    public string LastDragTarget { get; private set; } = string.Empty;
    public string LastKeyDown { get; private set; } = string.Empty;
    public string LastKeyUp { get; private set; } = string.Empty;
    public string LastInsertedText { get; private set; } = string.Empty;
    public List<string> KeyEvents { get; } = [];
    public bool LastFullPageScreenshot { get; private set; }
    public int MouseMoveCount { get; private set; }
    public int PageScreenshotCount { get; private set; }
    public ClickPulseStyle? LastCursorPulseStyle { get; private set; }
    public List<ClickPulseStyle?> CursorPulseStyles { get; } = [];
    public List<ElementPoint> CursorPoints { get; } = [];
    public bool LastCursorPressed { get; private set; }
    public bool LastCursorTrail { get; private set; }
    public bool LastCursorBreadcrumb { get; private set; }
    public PointerVisualOptions? LastCursorVisual { get; private set; }
    public bool RemoveDomCursorCalled { get; private set; }
    public List<(bool Pressed, bool Trail, bool Breadcrumb, PointerVisualOptions? Visual)> CursorStates { get; } = [];

    public string GetElementHtml(string remoteDebuggingUrl, string selector) => string.Empty;
    public byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector, ScreenshotOptions? options = null)
    {
        LastElementScreenshotOptions = options ?? new();
        return [];
    }
    public string Navigate(string remoteDebuggingUrl, string target) => target;
    public void WaitForElement(string remoteDebuggingUrl, string selector, int timeoutMilliseconds)
    {
        LastWaitSelector = selector;
        LastWaitTimeout = timeoutMilliseconds;
    }
    public void Click(string remoteDebuggingUrl, string selector)
    {
        LastClickedSelector = selector;
        ClickCount++;
    }
    public void Type(string remoteDebuggingUrl, string selector, string text)
    {
        LastTypedSelector = selector;
        LastTypedText = text;
    }
    public void TypeProgressively(string remoteDebuggingUrl, string selector, string text, int delayMilliseconds = 80, Action? afterCharacter = null)
    {
        LastTypedSelector = selector;
        LastTypedText = text;
        LastTypeDelay = delayMilliseconds;
        afterCharacter?.Invoke();
    }
    public void Clear(string remoteDebuggingUrl, string selector) => LastClearedSelector = selector;
    public void Press(string remoteDebuggingUrl, string key) => KeyEvents.Add($"press:{key}");
    public void KeyDown(string remoteDebuggingUrl, string key)
    {
        LastKeyDown = key;
        KeyEvents.Add($"down:{key}");
    }
    public void KeyUp(string remoteDebuggingUrl, string key)
    {
        LastKeyUp = key;
        KeyEvents.Add($"up:{key}");
    }
    public void InsertText(string remoteDebuggingUrl, string text) => LastInsertedText = text;
    public void Hover(string remoteDebuggingUrl, string selector) => LastHoveredSelector = selector;
    public void ScrollElementIntoView(string remoteDebuggingUrl, string selector) => LastScrolledSelector = selector;
    public void Select(string remoteDebuggingUrl, string selector, string value)
    {
        LastSelectedSelector = selector;
        LastSelectedValue = value;
    }
    public void ShowMessageBar(string remoteDebuggingUrl, string message, BrowserCaptionOptions? options = null)
    {
        LastMessageBar = message;
        MessageBars.Add(message);
        LastCaptionOptions = options;
    }
    public void PromoteMessageBar(string remoteDebuggingUrl) { }
    public string GetElementText(string remoteDebuggingUrl, string selector)
    {
        LastElementTextSelector = selector;
        return TextResponses.Count > 0 ? TextResponses.Dequeue() : string.Empty;
    }
    public string Evaluate(string remoteDebuggingUrl, string expression)
    {
        LastExpression = expression;
        EvaluatedExpressions.Add(expression);
        return EvaluateResponses.Count > 0 ? EvaluateResponses.Dequeue() : "{}";
    }
    public string AddInitScript(string remoteDebuggingUrl, string source)
    {
        LastInitScript = source;
        return "init-1";
    }
    public void ArmDiagnostics(string remoteDebuggingUrl)
    {
        LastDiagnosticsUrl = remoteDebuggingUrl;
    }
    public void SetViewport(string remoteDebuggingUrl, ViewportOptions options)
    {
        LastViewport = new(options.Width, options.Height);
        LastViewportOptions = options;
        ViewportOptionsHistory.Add(options);
    }
    public ViewportSize GetViewportSize(string remoteDebuggingUrl) => new(800, 600);
    public void DragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector)
    {
        LastDragSource = sourceSelector;
        LastDragTarget = targetSelector;
    }
    public void MouseDragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector, IReadOnlyList<ElementPoint> path, Action<ElementPoint>? afterMove = null) { }
    public void BeginPageDrag(string remoteDebuggingUrl, string sourceSelector, ElementPoint point) => LastBeginDragPoint = point;
    public void MovePageDrag(string remoteDebuggingUrl, ElementPoint point) => LastMoveDragPoint = point;
    public void EndPageDrag(string remoteDebuggingUrl, ElementPoint point) => LastEndDragPoint = point;
    public void RemoveDefaultDragGhost(string remoteDebuggingUrl) { }
    public void MoveMouse(string remoteDebuggingUrl, ElementPoint point, int buttons)
    {
        LastMouseMove = point;
        MouseMoveCount++;
    }
    public void MouseDown(string remoteDebuggingUrl, ElementPoint point) => LastMouseDown = point;
    public void MouseUp(string remoteDebuggingUrl, ElementPoint point) => LastMouseUp = point;
    public byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true, bool fullPage = false, ScreenshotOptions? options = null)
    {
        PageScreenshotCount++;
        LastPageScreenshotOptions = options ?? new(FullPage: fullPage);
        PageScreenshotOptions.Add(LastPageScreenshotOptions);
        LastFullPageScreenshot = LastPageScreenshotOptions.FullPage;
        if (PageScreenshotResponses.Count > 0) return PageScreenshotResponses.Dequeue();
        using var image = new Image<Rgba32>(1, 1, Color.White);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
    public byte[] PrintPdf(string remoteDebuggingUrl, PdfPrintOptions options)
    {
        LastPdfOptions = options;
        return [1, 2, 3];
    }
    public ElementBox GetElementBox(string remoteDebuggingUrl, string selector)
    {
        LastElementBoxSelector = selector;
        return ElementBoxes.Count > 0 ? ElementBoxes.Dequeue() : new(0, 0, 1, 1);
    }
    public ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector)
    {
        LastElementCenterSelector = selector;
        return new(0, 0);
    }
    public void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point, ClickPulseStyle? pulseStyle = null, bool pressed = false, bool trail = false, bool breadcrumb = false, PointerVisualOptions? visual = null)
    {
        CursorPoints.Add(point);
        LastCursorPulseStyle = pulseStyle;
        CursorPulseStyles.Add(pulseStyle);
        LastCursorPressed = pressed;
        LastCursorTrail = trail;
        LastCursorBreadcrumb = breadcrumb;
        LastCursorVisual = visual;
        CursorStates.Add((pressed, trail, breadcrumb, visual));
    }
    public void RemoveDomCursor(string remoteDebuggingUrl) => RemoveDomCursorCalled = true;
    public IReadOnlyList<ChromePageTab> ListTabs(string remoteDebuggingUrl) =>
        TabResponses.Count > 0 ? TabResponses.Dequeue() : [];
    public void OpenTab(string remoteDebuggingUrl, string target) => LastOpenedTab = target;
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
