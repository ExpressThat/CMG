using System.Text.Json;

namespace CMG.Browser.Scripting.Recording;

public static partial class GifTimelineWriter
{
    private static void WriteReview(
        Utf8JsonWriter writer,
        string gifPath,
        GifReviewOptions review,
        GifFrameSink sink,
        IReadOnlyList<GifTimelineStep> steps,
        GifRecordingOutcome? outcome)
    {
        writer.WriteStartObject("review");
        var narration = review.ResolveNarrationPath(gifPath);
        if (narration is null) writer.WriteNull("narrationPath");
        else writer.WriteString("narrationPath", Path.GetFullPath(narration));
        var altText = review.RenderAltText(gifPath, sink, steps, outcome);
        if (altText is null) writer.WriteNull("altText"); else writer.WriteString("altText", altText);
        if (review.Description is null) writer.WriteNull("description");
        else writer.WriteString("description", review.Description);
        var stillPdf = review.ResolveStillPdfPath(gifPath);
        if (stillPdf is null) writer.WriteNull("stillPdfPath");
        else writer.WriteString("stillPdfPath", Path.GetFullPath(stillPdf));
        writer.WriteEndObject();
    }
}
