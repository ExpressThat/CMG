using CMG.Browser;

namespace CMG.Tests;

internal sealed class FakeAutomationClient : IBrowserAutomationClient
{
    public string GetElementHtml(string remoteDebuggingUrl, string selector) => string.Empty;
    public byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector) => [];
    public string Navigate(string remoteDebuggingUrl, string target) => target;
    public void WaitForElement(string remoteDebuggingUrl, string selector, int timeoutMilliseconds) { }
    public void Click(string remoteDebuggingUrl, string selector) { }
    public void Type(string remoteDebuggingUrl, string selector, string text) { }
    public void TypeProgressively(string remoteDebuggingUrl, string selector, string text, Action? afterCharacter = null) { }
    public void Clear(string remoteDebuggingUrl, string selector) { }
    public void Press(string remoteDebuggingUrl, string key) { }
    public void Hover(string remoteDebuggingUrl, string selector) { }
    public void ScrollElementIntoView(string remoteDebuggingUrl, string selector) { }
    public void Select(string remoteDebuggingUrl, string selector, string value) { }
    public void ShowMessageBar(string remoteDebuggingUrl, string message) { }
    public void PromoteMessageBar(string remoteDebuggingUrl) { }
    public string GetElementText(string remoteDebuggingUrl, string selector) => string.Empty;
    public string Evaluate(string remoteDebuggingUrl, string expression) => "{}";
    public void SetViewport(string remoteDebuggingUrl, int width, int height) { }
    public ViewportSize GetViewportSize(string remoteDebuggingUrl) => new(800, 600);
    public void DragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector) { }
    public void MouseDragAndDrop(string remoteDebuggingUrl, string sourceSelector, string targetSelector, IReadOnlyList<ElementPoint> path, Action<ElementPoint>? afterMove = null) { }
    public void BeginPageDrag(string remoteDebuggingUrl, string sourceSelector, ElementPoint point) { }
    public void MovePageDrag(string remoteDebuggingUrl, ElementPoint point) { }
    public void EndPageDrag(string remoteDebuggingUrl, ElementPoint point) { }
    public void RemoveDefaultDragGhost(string remoteDebuggingUrl) { }
    public void MoveMouse(string remoteDebuggingUrl, ElementPoint point, int buttons) { }
    public void MouseDown(string remoteDebuggingUrl, ElementPoint point) { }
    public void MouseUp(string remoteDebuggingUrl, ElementPoint point) { }
    public byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true) => [];
    public ElementBox GetElementBox(string remoteDebuggingUrl, string selector) => new(0, 0, 1, 1);
    public ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector) => new(0, 0);
    public void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point) { }
    public void RemoveDomCursor(string remoteDebuggingUrl) { }
    public IReadOnlyList<ChromePageTab> ListTabs(string remoteDebuggingUrl) => [];
    public void ActivateTab(string remoteDebuggingUrl, int index) { }
    public void CloseTab(string remoteDebuggingUrl, int index) { }
}
