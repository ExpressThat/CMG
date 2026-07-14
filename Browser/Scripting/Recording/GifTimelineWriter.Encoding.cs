using System.Text.Json;

namespace CMG.Browser.Scripting.Recording;

public static partial class GifTimelineWriter
{
    private static void WriteEncoding(Utf8JsonWriter writer, ScriptRecordingOptions options)
    {
        var encoding = options.EffectiveEncoding;
        writer.WriteStartObject("encoding");
        writer.WriteString("dither", encoding.Dither.ToString().ToLowerInvariant());
        writer.WriteString("palette", encoding.Palette.ToString().ToLowerInvariant());
        if (encoding.Colors is int colors) writer.WriteNumber("colors", colors); else writer.WriteNull("colors");
        if (encoding.KeepFramesDirectory is not null)
            writer.WriteString("keepFramesDirectory", Path.GetFullPath(encoding.KeepFramesDirectory));
        else
            writer.WriteNull("keepFramesDirectory");
        if (encoding.SizeBudget?.Bytes is long budget) writer.WriteNumber("sizeBudgetBytes", budget); else writer.WriteNull("sizeBudgetBytes");
        writer.WriteBoolean("budgetQualityFallback", encoding.SizeBudget?.QualityFallback ?? true);
        writer.WriteBoolean("budgetDownscaleFallback", encoding.SizeBudget?.DownscaleFallback ?? true);
        writer.WriteEndObject();
    }
}
