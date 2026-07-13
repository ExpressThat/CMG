using System.Text.Json;

namespace CMG.Browser.Scripting.Recording;

public static partial class GifTimelineWriter
{
    private static void WriteColor(Utf8JsonWriter writer, GifColorOptions options)
    {
        writer.WriteStartObject("color");
        if (options.Background is null) writer.WriteNull("background");
        else writer.WriteString("background", options.Background);
        writer.WriteString("gradientMode", options.GradientMode.ToString().ToLowerInvariant());
        writer.WriteBoolean("highContrastPalette", options.HighContrastPalette);
        writer.WriteEndObject();
    }
}
