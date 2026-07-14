using System.Text.Json;

namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    public BrowserGeometryMetrics GetGeometryMetrics(string remoteDebuggingUrl)
    {
        var json = Evaluate(remoteDebuggingUrl,
            "JSON.stringify({scale:visualViewport?.scale||1,dpr:devicePixelRatio||1,x:visualViewport?.offsetLeft||0,y:visualViewport?.offsetTop||0})");
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        return new BrowserGeometryMetrics(
            VisualScale: root.GetProperty("scale").GetDouble(),
            DevicePixelRatio: root.GetProperty("dpr").GetDouble(),
            VisualOffsetX: root.GetProperty("x").GetDouble(),
            VisualOffsetY: root.GetProperty("y").GetDouble());
    }
}
