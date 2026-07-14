using System.Text.Json;

namespace CMG.Browser.Scripting.Recording;

public static partial class GifTimelineWriter
{
    private static void WriteGeometry(Utf8JsonWriter writer, BrowserGeometryMetrics? geometry)
    {
        if (geometry is null) return;
        writer.WriteStartObject("geometry");
        writer.WriteString("coordinateSpace", geometry.CoordinateSpace);
        writer.WriteString("correction", "css-pixel-preserving");
        writer.WriteNumber("pageZoom", geometry.PageZoom);
        writer.WriteNumber("visualScale", geometry.VisualScale);
        writer.WriteNumber("devicePixelRatio", geometry.DevicePixelRatio);
        writer.WriteNumber("visualOffsetX", geometry.VisualOffsetX);
        writer.WriteNumber("visualOffsetY", geometry.VisualOffsetY);
        writer.WriteEndObject();
    }
}
