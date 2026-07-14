using System.Text.Json;

namespace CMG.Browser.Scripting.Recording;

public static partial class GifTimelineWriter
{
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
        writer.WriteString("storage", "disk-delta");
        writer.WriteNumber("changedRegionFrameCount", sink.ChangedRegionFrameCount);
        writer.WriteNumber("spoolBytes", sink.SpoolBytes);
        writer.WriteBoolean("streamingGif", sink.UsesStreamingGif);
        writer.WriteNumber("parallelPreprocessedFrameCount", sink.ParallelPreprocessedFrameCount);
        if (sink.BudgetBytes is long budget) writer.WriteNumber("sizeBudgetBytes", budget);
        else writer.WriteNull("sizeBudgetBytes");
        writer.WriteBoolean("sizeBudgetApplied", sink.BudgetApplied);
        writer.WriteBoolean("sizeBudgetMet", sink.BudgetMet);
        writer.WriteNumber("sizeBudgetAttempts", sink.BudgetAttempts);
        writer.WriteNumber("finalSizeBytes", sink.FinalSizeBytes);
        writer.WriteString("finalQuality", sink.FinalBudgetQuality.ToString().ToLowerInvariant());
        writer.WriteNumber("finalScale", sink.FinalBudgetScale);
        WriteGeometry(writer, sink.Geometry);
        writer.WriteEndObject();
    }
}
