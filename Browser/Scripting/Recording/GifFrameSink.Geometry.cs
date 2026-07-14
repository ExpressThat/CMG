namespace CMG.Browser.Scripting.Recording;

public sealed partial class GifFrameSink
{
    public BrowserGeometryMetrics? Geometry { get; private set; }

    public void SetGeometry(BrowserGeometryMetrics geometry) => Geometry = geometry;
}
