using System.Text.Json;

namespace CMG.Browser.Scripting.Recording;

public static partial class GifTimelineWriter
{
    private static void WritePointerEvidence(Utf8JsonWriter writer, GifPointerEvidenceOptions options)
    {
        writer.WriteStartObject("pointerEvidence");
        writer.WriteString("contrast", options.Contrast.ToString().ToLowerInvariant());
        writer.WriteString("targetCallout", options.TargetCallout.ToString().ToLowerInvariant());
        writer.WriteNumber("targetCalloutThreshold", options.TargetCalloutThreshold);
        writer.WriteString("targetZoom", options.TargetZoom.ToString().ToLowerInvariant());
        writer.WriteNumber("targetZoomThreshold", options.TargetZoomThreshold);
        writer.WriteString("pagePosition", options.PagePosition.ToString().ToLowerInvariant());
        writer.WriteString("tabContext", options.TabContext.ToString().ToLowerInvariant());
        writer.WriteBoolean("focusPulse", options.FocusPulse);
        writer.WriteString("idle", options.Idle.ToString().ToLowerInvariant());
        writer.WriteNumber("idleThresholdMilliseconds", options.IdleThresholdMilliseconds);
        writer.WriteBoolean("teleportMarker", options.TeleportMarker);
        writer.WriteNumber("mouseDownHoldMilliseconds", options.MouseDownHoldMilliseconds);
        writer.WriteEndObject();
    }
}
