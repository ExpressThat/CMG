using System.Text.Json;

namespace CMG.Browser;

public sealed partial class ChromeDevToolsClient
{
    public BrowserGeometryMetrics GetGeometryMetrics(string remoteDebuggingUrl) => Run(async () =>
    {
        await using var session = await OpenPrimaryPageSession(remoteDebuggingUrl);
        var metrics = await session.SendCommand("Page.getLayoutMetrics");
        var viewport = Viewport(metrics);
        var runtime = await session.SendCommand("Runtime.evaluate", writer =>
        {
            writer.WriteString("expression", "window.devicePixelRatio || 1");
            writer.WriteBoolean("returnByValue", true);
        });
        var dpr = TryReadElement(runtime, ["result", "result"], out var runtimeResult) &&
            TryReadDouble(runtimeResult, "value", out var ratio) ? ratio : 1;
        return new BrowserGeometryMetrics(
            Number(viewport, "zoom", 1), Number(viewport, "scale", 1), dpr,
            Number(viewport, "offsetX", 0), Number(viewport, "offsetY", 0));
    });

    private static JsonElement Viewport(JsonElement metrics)
    {
        if (TryReadElement(metrics, ["result", "cssVisualViewport"], out var css)) return css;
        return TryReadElement(metrics, ["result", "visualViewport"], out var legacy) ? legacy : default;
    }

    private static double Number(JsonElement element, string name, double fallback) =>
        element.ValueKind is JsonValueKind.Object && TryReadDouble(element, name, out var value) ? value : fallback;
}
