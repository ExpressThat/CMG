using System.Text.Json;

namespace CMG.Runner;

public static partial class CmgJsonReportWriter
{
    private static void WriteStepGifEvidence(Utf8JsonWriter writer, CmgStepResult step)
    {
        writer.WriteStartArray("gifEvidence");
        foreach (var evidence in step.GifEvidence)
        {
            writer.WriteStartObject();
            writer.WriteString("gifPath", evidence.GifPath);
            writer.WriteString("timelinePath", evidence.TimelinePath);
            writer.WriteNumber("startFrameIndex", evidence.StartFrameIndex);
            if (evidence.EndFrameIndex is int end) writer.WriteNumber("endFrameIndex", end); else writer.WriteNull("endFrameIndex");
            writer.WriteNumber("startTimeMilliseconds", evidence.StartTimeMilliseconds);
            writer.WriteNumber("endTimeMilliseconds", evidence.EndTimeMilliseconds);
            writer.WriteNumber("capturedFrameCount", evidence.CapturedFrameCount);
            writer.WriteNumber("capturedDurationMilliseconds", evidence.CapturedDurationMilliseconds);
            writer.WriteNumber("estimatedRgbaBytes", evidence.EstimatedRgbaBytes);
            if (evidence.FailureFrameIndex is int failure) writer.WriteNumber("failureFrameIndex", failure); else writer.WriteNull("failureFrameIndex");
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}
