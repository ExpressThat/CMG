using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    public byte[] GetPageScreenshot(string remoteDebuggingUrl, bool promoteMessageBar = true, bool fullPage = false, ScreenshotOptions? options = null)
    {
        options ??= new(FullPage: fullPage);
        if (fullPage && !options.FullPage)
        {
            options = options with { FullPage = true };
        }

        return Run(async () =>
        {
            await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
            if (promoteMessageBar)
            {
                await PromoteMessageBar(session);
            }

            var screenshot = options.FullPage
                ? await CaptureFullPageScreenshot(session, options)
                : await session.SendCommand("Page.captureScreenshot", writer => WriteScreenshotOptions(writer, options));

            if (!TryReadString(screenshot, "result", "data", out var data) || string.IsNullOrWhiteSpace(data))
            {
                throw new ChromeDevToolsException("Chrome did not return screenshot image data.");
            }

            return Convert.FromBase64String(data);
        });
    }

    private static async Task<JsonElement> CaptureFullPageScreenshot(DevToolsSession session, ScreenshotOptions options)
    {
        var metrics = await session.SendCommand("Page.getLayoutMetrics");
        if (!TryReadElement(metrics, ["result", "contentSize"], out var size) ||
            !TryReadDouble(size, "width", out var width) ||
            !TryReadDouble(size, "height", out var height))
        {
            throw new ChromeDevToolsException("Chrome did not return full-page layout metrics.");
        }

        return await session.SendCommand("Page.captureScreenshot", writer =>
        {
            WriteScreenshotOptions(writer, options);
            writer.WriteBoolean("captureBeyondViewport", true);
            if (options.Clip is null)
            {
                writer.WriteStartObject("clip");
                writer.WriteNumber("x", 0);
                writer.WriteNumber("y", 0);
                writer.WriteNumber("width", Math.Max(1, width));
                writer.WriteNumber("height", Math.Max(1, height));
                writer.WriteNumber("scale", 1);
                writer.WriteEndObject();
            }
        });
    }

    public ElementPoint GetElementCenter(string remoteDebuggingUrl, string selector)
    {
        return Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, selector) ??
                throw new ElementNotFoundException(selector);

            await using var session = await DevToolsSession.Connect(pageTarget);
            var clip = await GetElementClip(session, selector);
            await EnsurePointInViewport(session, selector, clip.CenterX, clip.CenterY);

            return new ElementPoint(clip.CenterX, clip.CenterY);
        });
    }

    public ElementBox GetElementBox(string remoteDebuggingUrl, string selector)
    {
        return Run(async () =>
        {
            var pageTarget = await TryFindPageWithSelector(remoteDebuggingUrl, selector) ??
                throw new ElementNotFoundException(selector);

            await using var session = await DevToolsSession.Connect(pageTarget);
            var clip = await GetElementClip(session, selector);
            return new ElementBox(clip.X, clip.Y, clip.Width, clip.Height);
        });
    }

    public void MoveDomCursor(string remoteDebuggingUrl, ElementPoint point, ClickPulseStyle? pulseStyle = null, bool pressed = false, bool trail = false, bool breadcrumb = false, PointerVisualOptions? visual = null)
    {
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.MoveDomCursor(point, pulseStyle, pressed, trail, breadcrumb, visual));
    }

    public void RemoveDomCursor(string remoteDebuggingUrl)
    {
        Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveDomCursor());
    }

    public IReadOnlyList<ChromePageTab> ListTabs(string remoteDebuggingUrl)
    {
        return Run(async () =>
        {
            var targets = await GetPageTargets(remoteDebuggingUrl);

            return targets
                .Select(target => new ChromePageTab(target.Id, target.Title, target.Url))
                .ToArray();
        });
    }

    public void OpenTab(string remoteDebuggingUrl, string target)
    {
        Run(async () =>
        {
            using var httpClient = new HttpClient { Timeout = CommandTimeout };
            var response = await httpClient.PutAsync(
                $"{remoteDebuggingUrl.TrimEnd('/')}/json/new?{Uri.EscapeDataString(target)}",
                content: null);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            if (TryReadString(document.RootElement, "id", out var id) && !string.IsNullOrWhiteSpace(id))
            {
                SetActiveTarget(remoteDebuggingUrl, id);
            }

            return true;
        });
    }

    public void ActivateTab(string remoteDebuggingUrl, int index)
    {
        Run(async () =>
        {
            var target = await GetPageTargetAt(remoteDebuggingUrl, index);
            using var httpClient = new HttpClient { Timeout = CommandTimeout };
            await httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json/activate/{target.Id}");
            SetActiveTarget(remoteDebuggingUrl, target.Id);

            return true;
        });
    }

    public void CloseTab(string remoteDebuggingUrl, int index)
    {
        Run(async () =>
        {
            var target = await GetPageTargetAt(remoteDebuggingUrl, index);
            using var httpClient = new HttpClient { Timeout = CommandTimeout };
            await httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json/close/{target.Id}");
            ClearActiveTarget(remoteDebuggingUrl, target.Id);

            return true;
        });
    }
}
