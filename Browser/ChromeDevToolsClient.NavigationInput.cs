using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    public string Navigate(string remoteDebuggingUrl, string target)
    {
        return Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            await ApplyInitScripts(session, remoteDebuggingUrl);
            var response = await session.SendCommand("Page.navigate", writer => writer.WriteString("url", target));
            if (TryReadString(response, ["result", "errorText"], out var errorText) &&
                !string.IsNullOrWhiteSpace(errorText))
            {
                throw new ChromeDevToolsException($"Navigation to '{target}' failed: {errorText}.");
            }

            await WaitForPagePaint(session);

            return await GetCurrentPageUrl(session);
        });
    }

    public void WaitForElement(string remoteDebuggingUrl, string selector, int timeoutMilliseconds)
    {
        Run(async () =>
        {
            var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeoutMilliseconds);

            while (DateTimeOffset.UtcNow <= deadline)
            {
                if (await TryFindPageWithSelector(remoteDebuggingUrl, selector) is not null)
                {
                    return true;
                }

                await Task.Delay(PollInterval);
            }

            throw new ElementNotFoundException(selector);
        });
    }

    public void Click(string remoteDebuggingUrl, string selector)
    {
        Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, selector) ??
                throw new ElementNotFoundException(selector);

            await using var session = await DevToolsSession.Connect(pageTarget);

            var clip = await GetElementClip(session, selector);
            await EnsurePointInViewport(session, selector, clip.CenterX, clip.CenterY);
            await ClickAt(session, clip.CenterX, clip.CenterY);
            await Task.Delay(50);

            return true;
        });
    }

    public void Type(string remoteDebuggingUrl, string selector, string text)
    {
        _ = GetElementCenter(remoteDebuggingUrl, selector);
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            $"element.focus({{ preventScroll: true }}); element.value = `${{element.value ?? ''}}{BrowserDomScripts.EscapeTemplate(text)}`; element.dispatchEvent(new Event('input', {{ bubbles: true }})); element.dispatchEvent(new Event('change', {{ bubbles: true }})); return true;");
    }

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

    public void Clear(string remoteDebuggingUrl, string selector)
    {
        _ = GetElementCenter(remoteDebuggingUrl, selector);
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            "element.focus({ preventScroll: true }); element.value = ''; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true })); return true;");
    }

    public void Press(string remoteDebuggingUrl, string key)
    {
        Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);

            await session.SendCommand("Input.dispatchKeyEvent", writer =>
            {
                writer.WriteString("type", "keyDown");
                writer.WriteString("key", key);
            });

            await session.SendCommand("Input.dispatchKeyEvent", writer =>
            {
                writer.WriteString("type", "keyUp");
                writer.WriteString("key", key);
            });

            return true;
        });
    }

    public void Hover(string remoteDebuggingUrl, string selector)
    {
        _ = GetElementCenter(remoteDebuggingUrl, selector);
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            "const rect = element.getBoundingClientRect(); const options = { bubbles: true, clientX: rect.left + rect.width / 2, clientY: rect.top + rect.height / 2 }; element.dispatchEvent(new MouseEvent('mouseover', options)); element.dispatchEvent(new MouseEvent('mousemove', options)); return true;");
    }

    public void ScrollElementIntoView(string remoteDebuggingUrl, string selector)
    {
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            "element.scrollIntoView({ block: 'center', inline: 'center' }); return true;");
    }

    public void Select(string remoteDebuggingUrl, string selector, string value)
    {
        _ = GetElementCenter(remoteDebuggingUrl, selector);
        ExecuteElementScript(
            remoteDebuggingUrl,
            selector,
            $"element.value = {ToJsonStringLiteral(value)}; element.dispatchEvent(new Event('input', {{ bubbles: true }})); element.dispatchEvent(new Event('change', {{ bubbles: true }})); return true;");
    }
}
