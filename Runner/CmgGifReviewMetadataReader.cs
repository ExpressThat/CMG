using System.Text.Json;

namespace CMG.Runner;

internal sealed record CmgGifReviewMetadata(string? NarrationPath, string? AltText, string? Description);

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
                        String(review, "altText"),
                        String(review, "description"));
            }
            catch (JsonException)
            {
            }
        }
        var narration = Path.ChangeExtension(gifPath, ".narration.txt");
        return new(File.Exists(narration) ? narration : null, null, null);
    }

    private static string? String(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.String
            ? value.GetString()
            : null;
}
