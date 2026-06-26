using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    public string Navigate(string remoteDebuggingUrl, string target) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            await ApplyInitScripts(session, remoteDebuggingUrl);
            var response = await session.SendCommand("browsingContext.navigate", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteString("url", target);
                writer.WriteString("wait", "complete");
            });

            if (TryReadString(response, ["result", "url"], out var url) &&
                !string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            throw new ChromeDevToolsException($"Firefox did not return a final URL after navigating to '{target}'.");
        });

    public void WaitForElement(string remoteDebuggingUrl, string selector, int timeoutMilliseconds)
    {
        Run(async () =>
        {
            var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTimeOffset.UtcNow <= deadline)
            {
                if (Evaluate(remoteDebuggingUrl, $"Boolean({BrowserDomScripts.Query(selector)})") is "true")
                {
                    return true;
                }

                await Task.Delay(PollInterval);
            }

            throw new ElementNotFoundException(selector);
        });
    }

    public void Click(string remoteDebuggingUrl, string selector) =>
        ExecuteVisibleElementScript(remoteDebuggingUrl, selector, "element.click(); return true;");

    public void Type(string remoteDebuggingUrl, string selector, string text) =>
        ExecuteVisibleElementScript(
            remoteDebuggingUrl,
            selector,
            $"element.focus({{ preventScroll: true }}); element.value = `${{element.value ?? ''}}{BrowserDomScripts.EscapeTemplate(text)}`; element.dispatchEvent(new Event('input', {{ bubbles: true }})); element.dispatchEvent(new Event('change', {{ bubbles: true }})); return true;");

    public void TypeProgressively(string remoteDebuggingUrl, string selector, string text, Action? afterCharacter = null)
    {
        Click(remoteDebuggingUrl, selector);

        foreach (var character in text)
        {
            Type(remoteDebuggingUrl, selector, character.ToString());
            afterCharacter?.Invoke();
            Thread.Sleep(80);
        }
    }

    public void Clear(string remoteDebuggingUrl, string selector) =>
        ExecuteVisibleElementScript(remoteDebuggingUrl, selector, "element.focus({ preventScroll: true }); element.value = ''; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true })); return true;");

    public void Press(string remoteDebuggingUrl, string key) =>
        Evaluate(
            remoteDebuggingUrl,
            $"(() => {{ const target = document.activeElement || document.body; const options = {{ key: {BrowserDomScripts.JsonString(key)}, bubbles: true, cancelable: true }}; target.dispatchEvent(new KeyboardEvent('keydown', options)); target.dispatchEvent(new KeyboardEvent('keyup', options)); return true; }})()");

    public void KeyDown(string remoteDebuggingUrl, string key) =>
        DispatchKeyboard(remoteDebuggingUrl, key, "keydown");

    public void KeyUp(string remoteDebuggingUrl, string key) =>
        DispatchKeyboard(remoteDebuggingUrl, key, "keyup");

    public void InsertText(string remoteDebuggingUrl, string text) =>
        Evaluate(
            remoteDebuggingUrl,
            $"(() => {{ const target = document.activeElement || document.body; if ('value' in target) {{ target.value = `${{target.value ?? ''}}{BrowserDomScripts.EscapeTemplate(text)}`; target.dispatchEvent(new Event('input', {{ bubbles: true }})); target.dispatchEvent(new Event('change', {{ bubbles: true }})); return true; }} document.execCommand?.('insertText', false, {BrowserDomScripts.JsonString(text)}); return true; }})()");

    private void DispatchKeyboard(string remoteDebuggingUrl, string key, string type) =>
        Evaluate(
            remoteDebuggingUrl,
            $"(() => {{ const target = document.activeElement || document.body; const options = {{ key: {BrowserDomScripts.JsonString(key)}, bubbles: true, cancelable: true }}; target.dispatchEvent(new KeyboardEvent({BrowserDomScripts.JsonString(type)}, options)); return true; }})()");

    public void Hover(string remoteDebuggingUrl, string selector) =>
        ExecuteVisibleElementScript(remoteDebuggingUrl, selector, "const rect = element.getBoundingClientRect(); const options = { bubbles: true, clientX: rect.left + rect.width / 2, clientY: rect.top + rect.height / 2 }; element.dispatchEvent(new MouseEvent('mouseover', options)); element.dispatchEvent(new MouseEvent('mousemove', options)); return true;");

    public void ScrollElementIntoView(string remoteDebuggingUrl, string selector) =>
        ExecuteElementScript(remoteDebuggingUrl, selector, "element.scrollIntoView({ block: 'center', inline: 'center' }); return true;");

    public void Select(string remoteDebuggingUrl, string selector, string value) =>
        ExecuteVisibleElementScript(remoteDebuggingUrl, selector, $"element.value = {BrowserDomScripts.JsonString(value)}; element.dispatchEvent(new Event('input', {{ bubbles: true }})); element.dispatchEvent(new Event('change', {{ bubbles: true }})); return true;");

    public void ShowMessageBar(string remoteDebuggingUrl, string message) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.ShowMessageBar(message));

    public void PromoteMessageBar(string remoteDebuggingUrl) =>
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.PromoteMessageBar());

    public string GetElementText(string remoteDebuggingUrl, string selector) =>
        NonEmpty(Evaluate(remoteDebuggingUrl, $"(() => {{ const element = {BrowserDomScripts.Query(selector)}; return element?.innerText ?? element?.textContent ?? null; }})()"), selector);

    public string Evaluate(string remoteDebuggingUrl, string expression) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            return ReadScriptResultValue(await Evaluate(session, context.Id, expression));
        });

    private static readonly Dictionary<string, List<string>> InitScripts = [];
    private static readonly object InitScriptLock = new();

    public string AddInitScript(string remoteDebuggingUrl, string source)
    {
        lock (InitScriptLock)
        {
            var key = remoteDebuggingUrl.TrimEnd('/');
            var scripts = InitScripts.GetValueOrDefault(key) ?? [];
            scripts.Add(source);
            InitScripts[key] = scripts;
            return scripts.Count.ToString(CultureInfo.InvariantCulture);
        }
    }

    private static async Task ApplyInitScripts(FirefoxBiDiSession session, string remoteDebuggingUrl)
    {
        foreach (var source in SnapshotInitScripts(remoteDebuggingUrl))
        {
            await session.SendCommand("script.addPreloadScript", writer =>
            {
                writer.WriteString("functionDeclaration", $"() => {{ {source} }}");
            });
        }
    }

    private static IReadOnlyList<string> SnapshotInitScripts(string remoteDebuggingUrl)
    {
        lock (InitScriptLock)
        {
            return InitScripts.GetValueOrDefault(remoteDebuggingUrl.TrimEnd('/'))?.ToArray() ?? [];
        }
    }

    public void SetViewport(string remoteDebuggingUrl, ViewportOptions options) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            await session.SendCommand("browsingContext.setViewport", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteStartObject("viewport");
                writer.WriteNumber("width", options.Width);
                writer.WriteNumber("height", options.Height);
                writer.WriteEndObject();
                writer.WriteNumber("devicePixelRatio", options.DeviceScaleFactor);
            });
            if (options.HasTouch || options.IsMobile)
            {
                await Evaluate(session, context.Id, "Object.defineProperty(navigator, 'maxTouchPoints', { configurable: true, get: () => 1 }); window.ontouchstart = window.ontouchstart || null;");
            }

            return true;
        });

    public ViewportSize GetViewportSize(string remoteDebuggingUrl) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            var json = ReadScriptResultValue(await Evaluate(session, context.Id, "JSON.stringify({ width: window.innerWidth, height: window.innerHeight })"));
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            return new ViewportSize(root.GetProperty("width").GetDouble(), root.GetProperty("height").GetDouble());
        });
}
