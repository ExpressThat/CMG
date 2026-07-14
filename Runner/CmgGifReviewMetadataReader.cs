using System.Text.Json;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

internal sealed record CmgGifReviewMetadata(
    string? NarrationPath,
    string? StillPdfPath,
    string? AltText,
    string? Description);

internal static class CmgGifReviewMetadataReader
{
    public static CmgGifReviewMetadata Read(CmgTestResult test, string gifPath)
    {
        var timeline = CmgGifEvidenceReader.TimelineFor(test, gifPath);
        if (timeline is not null && File.Exists(timeline))
        {
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(timeline));
                if (document.RootElement.TryGetProperty("review", out var review))
                    return new(
                        String(review, "narrationPath"),
                        String(review, "stillPdfPath"),
                        String(review, "altText"),
                        String(review, "description"));
            }
            catch (JsonException)
            {
            }
        }
        var narration = GifArtifactPaths.Narration(gifPath);
        var stillPdf = GifArtifactPaths.StillPdf(gifPath);
        return new(
            File.Exists(narration) ? narration : null,
            File.Exists(stillPdf) ? stillPdf : null,
            null,
            null);
    }

    private static string? String(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.String
            ? value.GetString()
            : null;
}
