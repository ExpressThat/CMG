using System.Text.Json;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public static partial class CmgJsonReportWriter
{
    private static void WriteGifMetadata(Utf8JsonWriter writer, CmgTestResult test)
    {
        writer.WriteStartArray("gifMetadata");
        foreach (var path in GifPaths(test.GifPath))
        {
            writer.WriteStartObject();
            writer.WriteString("path", path);
            writer.WriteString("format", RecordingFormat(path));
            writer.WriteString("quality", QualityFor(test, path));
            var timeline = CmgGifEvidenceReader.TimelineFor(test, path);
            if (timeline is null) writer.WriteNull("timelinePath"); else writer.WriteString("timelinePath", timeline);
            var review = CmgGifReviewMetadataReader.Read(test, path);
            if (review.NarrationPath is null) writer.WriteNull("narrationPath"); else writer.WriteString("narrationPath", review.NarrationPath);
            if (review.AltText is null) writer.WriteNull("altText"); else writer.WriteString("altText", review.AltText);
            if (review.Description is null) writer.WriteNull("description"); else writer.WriteString("description", review.Description);
            var file = new FileInfo(path);
            writer.WriteBoolean("exists", file.Exists);
            if (file.Exists)
            {
                WriteInspection(writer, file);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static string RecordingFormat(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".apng" => "apng", ".webp" => "webp", ".mp4" => "mp4", _ => "gif"
    };

    private static IEnumerable<string> GifPaths(string? gifPath)
    {
        if (string.IsNullOrWhiteSpace(gifPath))
        {
            yield break;
        }

        foreach (var path in gifPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return path;
        }
    }

    private static string? QualityFor(CmgTestResult test, string path)
    {
        if (test.GifQualities.TryGetValue(path, out var quality))
        {
            return quality;
        }

        return TimelineQuality(path);
    }

    private static string? TimelineQuality(string gifPath)
    {
        var timeline = GifArtifactPaths.Timeline(gifPath);
        if (!File.Exists(timeline))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(timeline));
            return document.RootElement.TryGetProperty("quality", out var quality)
                ? quality.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void WriteInspection(Utf8JsonWriter writer, FileInfo file)
    {
        if (file.Extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
        {
            WriteVideoInspection(writer, file);
            return;
        }
        try
        {
            var metadata = GifInspector.InspectRecording(file);
            writer.WriteNumber("frames", metadata.FrameCount);
            writer.WriteNumber("durationMs", metadata.DurationMilliseconds);
            writer.WriteNumber("width", metadata.Width);
            writer.WriteNumber("height", metadata.Height);
            writer.WriteNumber("sizeBytes", metadata.SizeBytes);
            writer.WriteNumber("fps", metadata.DurationMilliseconds > 0 ? Math.Round(metadata.FrameCount * 1000d / metadata.DurationMilliseconds, 2) : 0);
            writer.WriteString("palette", metadata.Palette);
            writer.WriteString("paletteColors", metadata.PaletteColors);
            writer.WriteBoolean("transparent", metadata.Transparent);
            writer.WriteNumber("repeat", metadata.RepeatCount);
        }
        catch (Exception exception)
        {
            writer.WriteString("error", exception.Message);
        }
    }

    private static void WriteVideoInspection(Utf8JsonWriter writer, FileInfo file)
    {
        var metadata = CmgRecordingTimelineMetadataReader.Read(file.FullName);
        if (metadata is null) { writer.WriteNumber("sizeBytes", file.Length); return; }
        writer.WriteNumber("frames", metadata.Frames);
        writer.WriteNumber("durationMs", metadata.DurationMilliseconds);
        writer.WriteNumber("width", metadata.Width);
        writer.WriteNumber("height", metadata.Height);
        writer.WriteNumber("sizeBytes", metadata.SizeBytes);
        writer.WriteNumber("fps", metadata.DurationMilliseconds > 0
            ? Math.Round(metadata.Frames * 1000d / metadata.DurationMilliseconds, 2) : 0);
    }
}
