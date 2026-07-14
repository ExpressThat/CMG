using System.Text.Json;

namespace CMG.Browser.Scripting.Recording;

public static partial class GifTimelineWriter
{
    public static string Write(
        string path,
        string gifPath,
        ScriptRecordingOptions options,
        GifFrameSink sink,
        IReadOnlyList<GifTimelineCheckpoint>? checkpoints = null,
        IReadOnlyList<GifTimelineStep>? steps = null,
        IReadOnlyList<GifRedactionAuditEntry>? redactionAudit = null,
        IReadOnlyList<GifDebugFrame>? debugFrames = null)
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
        WritePayload(writer, gif, options, sink, checkpoints ?? [], steps ?? [], redactionAudit ?? [], debugFrames ?? []);
        return fullPath;
    }

    private static void WritePayload(
        Utf8JsonWriter writer,
        FileInfo gif,
        ScriptRecordingOptions options,
        GifFrameSink sink,
        IReadOnlyList<GifTimelineCheckpoint> checkpoints,
        IReadOnlyList<GifTimelineStep> steps,
        IReadOnlyList<GifRedactionAuditEntry> redactionAudit,
        IReadOnlyList<GifDebugFrame> debugFrames)
    {
        writer.WriteStartObject();
        writer.WriteNumber("version", 2);
        writer.WriteString("createdAtUtc", DateTimeOffset.UtcNow);
        writer.WriteString("gifPath", gif.FullName);
        writer.WriteNumber("fileSizeBytes", gif.Exists ? gif.Length : 0);
        writer.WriteString("quality", options.Quality.ToString().ToLowerInvariant());
        writer.WriteNumber("frameCount", sink.FrameCount);
        writer.WriteNumber("durationMilliseconds", sink.DurationMilliseconds);
        writer.WriteNumber("width", sink.Width);
        writer.WriteNumber("height", sink.Height);
        WriteCaptureDiagnostics(writer, sink);
        WriteEncoding(writer, options);
        WriteColor(writer, options.EffectiveColor);
        WriteFraming(writer, options);
        WritePointerEvidence(writer, options.EffectivePointerEvidence);
        writer.WritePropertyName("frameDelaysMilliseconds");
        writer.WriteStartArray();
        foreach (var delay in sink.FrameDelaysMilliseconds)
        {
            writer.WriteNumberValue(delay);
        }
        writer.WriteEndArray();
        WriteCheckpoints(writer, checkpoints);
        WriteSteps(writer, steps);
        WriteRedactions(writer, options.EffectiveRedaction, redactionAudit);
        WriteDebugFrames(writer, debugFrames);
        WriteTiming(writer, options);
        writer.WriteEndObject();
    }

    private static void WriteCaptureDiagnostics(Utf8JsonWriter writer, GifFrameSink sink)
    {
        writer.WriteStartObject("captureDiagnostics");
        writer.WriteNumber("sourceFrameCount", sink.SourceFrameCount);
        writer.WriteNumber("retainedFrameCount", sink.FrameCount);
        writer.WriteNumber("duplicateFramesCoalesced", sink.DuplicateFramesCoalesced);
        writer.WriteNumber("sampledFramesSkipped", sink.SampledFramesSkipped);
        writer.WriteNumber("blankFrameCount", sink.BlankFrameCount);
        writer.WriteNumber("iccProfileFrameCount", sink.IccProfileFrameCount);
        writer.WriteNumber("cicpProfileFrameCount", sink.CicpProfileFrameCount);
        writer.WriteNumber("gammaMetadataFrameCount", sink.GammaMetadataFrameCount);
        writer.WriteNumber("colorProfileChangeCount", sink.ColorProfileChangeCount);
        writer.WriteNumber("peakRetainedPixelBytes", sink.PeakRetainedPixelBytes);
        writer.WriteNumber("processingMilliseconds", Math.Round(sink.ProcessingMilliseconds, 2));
        writer.WriteEndObject();
    }

    private static void WriteDebugFrames(Utf8JsonWriter writer, IReadOnlyList<GifDebugFrame> frames)
    {
        writer.WriteStartArray("debugFrames");
        foreach (var frame in frames)
        {
            writer.WriteStartObject();
            writer.WriteNumber("frameIndex", frame.FrameIndex);
            writer.WriteNumber("startTimeMilliseconds", frame.StartTimeMilliseconds);
            writer.WriteNumber("delayMilliseconds", frame.DelayMilliseconds);
            writer.WriteString("kind", frame.Kind);
            if (frame.Action is null) writer.WriteNull("action"); else writer.WriteString("action", frame.Action);
            if (frame.LineNumber is int line) writer.WriteNumber("lineNumber", line); else writer.WriteNull("lineNumber");
            writer.WriteString("context", frame.Context);
            if (frame.Target is null) writer.WriteNull("target"); else writer.WriteString("target", frame.Target);
            writer.WriteNumber("pointerX", frame.Pointer.X);
            writer.WriteNumber("pointerY", frame.Pointer.Y);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private static void WriteRedactions(
        Utf8JsonWriter writer,
        GifRedactionOptions options,
        IReadOnlyList<GifRedactionAuditEntry> audit)
    {
        writer.WriteStartObject("redactions");
        writer.WriteString("automatic", options.Auto.ToString().ToLowerInvariant());
        writer.WriteBoolean("strict", options.Strict);
        writer.WriteStartArray("initialRules");
        foreach (var rule in options.EffectiveRules)
        {
            writer.WriteStartObject();
            writer.WriteString("locator", rule.Locator);
            writer.WriteString("style", rule.Style.ToString().ToLowerInvariant());
            writer.WriteNumber("padding", rule.Padding);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteStartArray("audit");
        foreach (var entry in audit)
        {
            writer.WriteStartObject();
            writer.WriteString("operation", entry.Operation);
            writer.WriteString("locator", entry.Locator);
            writer.WriteString("style", entry.Style);
            writer.WriteNumber("frameIndex", entry.FrameIndex);
            writer.WriteNumber("timeMilliseconds", entry.TimeMilliseconds);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteSteps(Utf8JsonWriter writer, IReadOnlyList<GifTimelineStep> steps)
    {
        writer.WritePropertyName("steps");
        writer.WriteStartArray();
        foreach (var step in steps.OrderBy(step => step.Sequence))
        {
            writer.WriteStartObject();
            writer.WriteNumber("sequence", step.Sequence);
            writer.WriteNumber("lineNumber", step.LineNumber);
            writer.WriteString("action", step.Action);
            writer.WriteString("context", step.Context);
            writer.WriteBoolean("success", step.Success);
            writer.WriteNumber("startFrameIndex", step.StartFrameIndex);
            if (step.EndFrameIndex is int end) writer.WriteNumber("endFrameIndex", end); else writer.WriteNull("endFrameIndex");
            writer.WriteNumber("startTimeMilliseconds", step.StartTimeMilliseconds);
            writer.WriteNumber("endTimeMilliseconds", step.EndTimeMilliseconds);
            if (step.FailureFrameIndex is int failure) writer.WriteNumber("failureFrameIndex", failure); else writer.WriteNull("failureFrameIndex");
            if (step.Error is null) writer.WriteNull("error"); else writer.WriteString("error", step.Error);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
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
        writer.WriteString("pointerPath", options.EffectivePointerMotion.PointerPath.ToString().ToLowerInvariant());
        writer.WriteString("dragPath", (options.EffectivePointerMotion.DragPath ?? options.EffectivePointerMotion.PointerPath).ToString().ToLowerInvariant());
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

    private static void WriteEncoding(Utf8JsonWriter writer, ScriptRecordingOptions options)
    {
        var encoding = options.EffectiveEncoding;
        writer.WritePropertyName("encoding");
        writer.WriteStartObject();
        writer.WriteString("dither", encoding.Dither.ToString().ToLowerInvariant());
        writer.WriteString("palette", encoding.Palette.ToString().ToLowerInvariant());
        if (encoding.Colors is int colors) writer.WriteNumber("colors", colors); else writer.WriteNull("colors");
        if (encoding.KeepFramesDirectory is not null)
            writer.WriteString("keepFramesDirectory", Path.GetFullPath(encoding.KeepFramesDirectory));
        else
            writer.WriteNull("keepFramesDirectory");
        writer.WriteEndObject();
    }

    private static void WriteFraming(Utf8JsonWriter writer, ScriptRecordingOptions options)
    {
        var framing = options.EffectiveFraming;
        writer.WritePropertyName("framing");
        writer.WriteStartObject();
        if (framing.CropSelector is null) writer.WriteNull("crop"); else writer.WriteString("crop", framing.CropSelector);
        writer.WriteNumber("cropPadding", framing.CropPadding);
        writer.WriteNumber("safeArea", framing.SafeArea);
        writer.WriteNumber("layoutStabilityMilliseconds", framing.LayoutStabilityMilliseconds);
        writer.WriteNumber("scale", framing.Scale);
        if (framing.MaxWidth is int width) writer.WriteNumber("maxWidth", width); else writer.WriteNull("maxWidth");
        if (framing.MaxHeight is int height) writer.WriteNumber("maxHeight", height); else writer.WriteNull("maxHeight");
        if (framing.ViewportWidth is int viewportWidth)
            writer.WriteString("viewport", $"{viewportWidth}x{framing.ViewportHeight}");
        else writer.WriteNull("viewport");
        writer.WriteNumber("pixelRatio", framing.PixelRatio);
        writer.WriteEndObject();
    }
}
