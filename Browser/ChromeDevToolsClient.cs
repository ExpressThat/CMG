using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient : IBrowserAutomationClient
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    public string GetElementHtml(string remoteDebuggingUrl, string selector)
    {
        return Run(async () =>
        {
            var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

            foreach (var pageTarget in pageTargets)
            {
                await using var session = await DevToolsSession.Connect(pageTarget);

                var response = await session.SendCommand("Runtime.evaluate", writer =>
                {
                    writer.WriteString("expression", BuildOuterHtmlExpression(selector));
                    writer.WriteBoolean("returnByValue", true);
                });

                if (TryReadString(response, ["result", "result", "value"], out var html) && html is not null)
                {
                    return html;
                }
            }

            throw new ElementNotFoundException(selector);
        });
    }

    public byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector, ScreenshotOptions? options = null)
    {
        options ??= new();
        return Run(async () =>
        {
            var pageTargets = await GetPageWebSocketDebuggerUrls(remoteDebuggingUrl);

            foreach (var pageTarget in pageTargets)
            {
                await using var session = await DevToolsSession.Connect(pageTarget);

                await session.ScrollElementIntoView(selector);
                await PromoteMessageBar(session);
                var clip = await GetElementPageClip(session, selector);
                if (clip.Width <= 0 || clip.Height <= 0)
                {
                    throw new ChromeDevToolsException($"Element '{selector}' has no visible area to screenshot.");
                }

                var screenshot = await session.SendCommand("Page.captureScreenshot", writer =>
                {
                    WriteScreenshotOptions(writer, options);
                    writer.WriteStartObject("clip");
                    writer.WriteNumber("x", clip.X);
                    writer.WriteNumber("y", clip.Y);
                    writer.WriteNumber("width", clip.Width);
                    writer.WriteNumber("height", clip.Height);
                    writer.WriteNumber("scale", 1);
                    writer.WriteEndObject();
                });

                if (!TryReadString(screenshot, "result", "data", out var data) || string.IsNullOrWhiteSpace(data))
                {
                    throw new ChromeDevToolsException("Chrome did not return screenshot image data.");
                }

                return Convert.FromBase64String(data);
            }

            throw new ElementNotFoundException(selector);
        });
    }

    private static void WriteScreenshotOptions(Utf8JsonWriter writer, ScreenshotOptions options)
    {
        var type = ScreenshotImage.NormalizeType(options.Type);
        writer.WriteString("format", type);
        if (type == "jpeg" && options.Quality is { } quality)
        {
            writer.WriteNumber("quality", quality);
        }
        if (options.OmitBackground)
        {
            writer.WriteBoolean("omitBackground", true);
        }
    }

    private static async Task<ElementClip> GetElementPageClip(DevToolsSession session, string selector)
    {
        var response = await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString(
                "expression",
                $$"""
                (() => {
                  const element = {{BrowserDomScripts.Query(selector)}};
                  if (!element) return null;
                  const rect = element.getBoundingClientRect();
                  return {
                    x: Math.max(0, rect.left + window.scrollX),
                    y: Math.max(0, rect.top + window.scrollY),
                    width: rect.width,
                    height: rect.height
                  };
                })()
                """);
            writer.WriteBoolean("returnByValue", true);
        });

        if (!TryReadElement(response, ["result", "result", "value"], out var value) || value.ValueKind is JsonValueKind.Null)
        {
            throw new ElementNotFoundException(selector);
        }

        if (!TryReadDouble(value, "x", out var x) ||
            !TryReadDouble(value, "y", out var y) ||
            !TryReadDouble(value, "width", out var width) ||
            !TryReadDouble(value, "height", out var height))
        {
            throw new ChromeDevToolsException($"Chrome did not return a screenshot clip for element '{selector}'.");
        }

        return new ElementClip(x, y, width, height);
    }
}
