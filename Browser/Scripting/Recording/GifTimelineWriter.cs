using System.Text.Json;

namespace CMG.Browser.Scripting.Recording;

public static class GifTimelineWriter
{
    public static string Write(
        string path,
        string gifPath,
        ScriptRecordingOptions options,
        GifFrameSink sink,
        IReadOnlyList<GifTimelineCheckpoint>? checkpoints = null)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var gif = new FileInfo(gifPath);
        using var stream = File.Create(fullPath);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        WritePayload(writer, gif, options, sink, checkpoints ?? []);
        return fullPath;
    }

    private static void WritePayload(
        Utf8JsonWriter writer,
        FileInfo gif,
        ScriptRecordingOptions options,
        GifFrameSink sink,
        IReadOnlyList<GifTimelineCheckpoint> checkpoints)
    {
        writer.WriteStartObject();
        writer.WriteNumber("version", 1);
        writer.WriteString("createdAtUtc", DateTimeOffset.UtcNow);
        writer.WriteString("gifPath", gif.FullName);
        writer.WriteNumber("fileSizeBytes", gif.Exists ? gif.Length : 0);
        writer.WriteString("quality", options.Quality.ToString().ToLowerInvariant());
        writer.WriteNumber("frameCount", sink.FrameCount);
        writer.WriteNumber("durationMilliseconds", sink.DurationMilliseconds);
        writer.WriteNumber("width", sink.Width);
        writer.WriteNumber("height", sink.Height);
        writer.WritePropertyName("frameDelaysMilliseconds");
        writer.WriteStartArray();
        foreach (var delay in sink.FrameDelaysMilliseconds)
        {
            writer.WriteNumberValue(delay);
        }
        writer.WriteEndArray();
        WriteCheckpoints(writer, checkpoints);
        WriteTiming(writer, options);
        writer.WriteEndObject();
    }

    private static void WriteCheckpoints(Utf8JsonWriter writer, IReadOnlyList<GifTimelineCheckpoint> checkpoints)
    {
        writer.WritePropertyName("checkpoints");
        writer.WriteStartArray();
        foreach (var checkpoint in checkpoints)
        {
            writer.WriteStartObject();
            writer.WriteString("name", checkpoint.Name);
            writer.WriteNumber("lineNumber", checkpoint.LineNumber);
            writer.WriteNumber("frameIndex", checkpoint.FrameIndex);
            writer.WriteNumber("timeMilliseconds", checkpoint.TimeMilliseconds);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private static void WriteTiming(Utf8JsonWriter writer, ScriptRecordingOptions options)
    {
        writer.WritePropertyName("timing");
        writer.WriteStartObject();
        if (options.EffectivePointerMotion.PointerDurationMilliseconds is int duration)
        {
            writer.WriteNumber("pointerDurationMilliseconds", duration);
        }
        else
        {
            writer.WriteNull("pointerDurationMilliseconds");
        }
        writer.WriteString("pointerSpeed", options.EffectivePointerMotion.PointerSpeed);
        writer.WriteString("pointerEasing", options.EffectivePointerMotion.PointerEasing.ToString().ToLowerInvariant());
        writer.WriteString("clickPulse", options.ClickPulse.ToString().ToLowerInvariant());
        writer.WriteNumber("holdAfterActionMilliseconds", options.HoldAfterActionMilliseconds);
        writer.WriteNumber("holdOnFailureMilliseconds", options.HoldOnFailureMilliseconds);
        writer.WriteNumber("preClickHoldMilliseconds", options.PreClickHoldMilliseconds);
        writer.WriteNumber("postClickHoldMilliseconds", options.PostClickHoldMilliseconds);
        writer.WriteNumber("holdAfterNavigationMilliseconds", options.HoldAfterNavigationMilliseconds);
        writer.WriteNumber("holdAfterAssertionMilliseconds", options.HoldAfterAssertionMilliseconds);
        writer.WriteNumber("frameDelayMilliseconds", options.FrameDelayMilliseconds);
        writer.WriteNumber("fps", Math.Round(1000d / Math.Max(1, options.FrameDelayMilliseconds), 2));
        writer.WriteEndObject();
    }
}
