using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient : IBrowserAutomationClient
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    public string GetElementHtml(string remoteDebuggingUrl, string selector) =>
        NonEmpty(Evaluate(remoteDebuggingUrl, $"document.querySelector({BrowserDomScripts.JsonString(selector)})?.outerHTML ?? null"), selector);

    public byte[] GetElementScreenshot(string remoteDebuggingUrl, string selector, ScreenshotOptions? options = null) =>
        Run(async () =>
        {
            options ??= new();
            await using var session = await FirefoxBiDiSession.Connect(remoteDebuggingUrl);
            var context = await session.GetPrimaryContext(remoteDebuggingUrl);
            await Evaluate(session, context.Id, BrowserDomScripts.ScrollIntoView(selector));
            await PromoteMessageBar(session, context.Id);
            var rect = await GetElementRect(session, context.Id, selector);
            var response = await session.SendCommand("browsingContext.captureScreenshot", writer =>
            {
                writer.WriteString("context", context.Id);
                writer.WriteStartObject("clip");
                writer.WriteString("type", "box");
                writer.WriteNumber("x", rect.X);
                writer.WriteNumber("y", rect.Y);
                writer.WriteNumber("width", rect.Width);
                writer.WriteNumber("height", rect.Height);
                writer.WriteEndObject();
            });

            return ScreenshotImage.ConvertIfNeeded(DecodeScreenshot(response), options);
        });
}
