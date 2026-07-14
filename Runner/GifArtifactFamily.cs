using System.Text.Json;

namespace CMG.Runner;

internal static class GifArtifactFamily
{
    public static IReadOnlyList<string> NarrationPaths(string gifPath)
    {
        var conventional = Path.ChangeExtension(gifPath, ".narration.txt");
        var paths = new List<string> { conventional };
        var timeline = Path.ChangeExtension(gifPath, ".timeline.json");
        if (!File.Exists(timeline)) return paths;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(timeline));
            if (document.RootElement.TryGetProperty("review", out var review) &&
                review.TryGetProperty("narrationPath", out var value) &&
                value.ValueKind is JsonValueKind.String &&
                value.GetString() is string candidate &&
                IsInsideArtifactDirectory(gifPath, candidate))
                paths.Add(Path.GetFullPath(candidate));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
        }
        return paths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static bool IsInsideArtifactDirectory(string gifPath, string candidate)
    {
        var root = Path.GetFullPath(Path.GetDirectoryName(Path.GetFullPath(gifPath)) ?? ".")
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return Path.GetFullPath(candidate).StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }
}
