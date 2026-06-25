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
            var context = await session.GetPrimaryContext();
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
                if (Evaluate(remoteDebuggingUrl, $"Boolean(document.querySelector({BrowserDomScripts.JsonString(selector)}))") is "true")
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
        NonEmpty(Evaluate(remoteDebuggingUrl, $"document.querySelector({BrowserDomScripts.JsonString(selector)})?.innerText ?? document.querySelector({BrowserDomScripts.JsonString(selector)})?.textContent ?? null"), selector);

    public string Evaluate(string remoteDebuggingUrl, string expression) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            return ReadScriptResultValue(await Evaluate(session, context.Id, expression));
        });

    public void SetViewport(string remoteDebuggingUrl, int width, int height) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            await session.SendCommand("browsingContext.setViewport", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteStartObject("viewport");
                writer.WriteNumber("width", width);
                writer.WriteNumber("height", height);
                writer.WriteEndObject();
                writer.WriteNumber("devicePixelRatio", 1);
            });

            return true;
        });

    public ViewportSize GetViewportSize(string remoteDebuggingUrl) =>
        Run(async () =>
        {
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext();
            var json = ReadScriptResultValue(await Evaluate(session, context.Id, "JSON.stringify({ width: window.innerWidth, height: window.innerHeight })"));
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            return new ViewportSize(root.GetProperty("width").GetDouble(), root.GetProperty("height").GetDouble());
        });
}
