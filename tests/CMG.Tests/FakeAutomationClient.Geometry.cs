using CMG.Browser;

namespace CMG.Tests;

internal sealed partial class FakeAutomationClient
{
    public BrowserGeometryMetrics Geometry { get; set; } = new();

    public BrowserGeometryMetrics GetGeometryMetrics(string remoteDebuggingUrl) => Geometry;
}
