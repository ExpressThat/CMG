using System.Text.Json;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

internal static class CmgGifEvidenceReader
{
    public static IReadOnlyDictionary<int, IReadOnlyList<CmgStepGifEvidence>> Read(
        IEnumerable<string> output,
        string? fallbackGifPath)
    {
        var result = new Dictionary<int, IReadOnlyList<CmgStepGifEvidence>>();
        foreach (var timelinePath in TimelinePaths(output))
        {
            ReadTimeline(timelinePath, fallbackGifPath, result);
        }

        return result;
    }

    public static string? TimelineFor(CmgTestResult test, string gifPath)
    {
        var evidencePath = test.Steps
            .SelectMany(step => step.GifEvidence)
            .FirstOrDefault(item => PathsEqual(item.GifPath, gifPath))
            ?.TimelinePath;
        if (evidencePath is not null)
        {
            return evidencePath;
        }

        var conventional = GifArtifactPaths.Timeline(gifPath);
        return File.Exists(conventional) ? conventional : null;
    }

    private static IEnumerable<string> TimelinePaths(IEnumerable<string> output) =>
        output
            .Where(line => line.StartsWith("GIF_TIMELINE ", StringComparison.Ordinal))
            .Select(line => line["GIF_TIMELINE ".Length..].Trim())
            .Where(path => path.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private static void ReadTimeline(
        string timelinePath,
        string? fallbackGifPath,
        Dictionary<int, IReadOnlyList<CmgStepGifEvidence>> result)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(timelinePath));
            var root = document.RootElement;
            var gifPath = root.TryGetProperty("gifPath", out var gif)
                ? gif.GetString() ?? fallbackGifPath
                : fallbackGifPath;
            if (gifPath is null || !root.TryGetProperty("steps", out var steps))
            {
                return;
            }

            foreach (var step in steps.EnumerateArray())
            {
                var sequence = step.GetProperty("sequence").GetInt32();
                var evidence = new CmgStepGifEvidence(
                    gifPath,
                    Path.GetFullPath(timelinePath),
                    step.GetProperty("startFrameIndex").GetInt32(),
                    NullableInt(step, "endFrameIndex"),
                    step.GetProperty("startTimeMilliseconds").GetInt32(),
                    step.GetProperty("endTimeMilliseconds").GetInt32(),
                    NullableInt(step, "failureFrameIndex"),
                    NullableInt(step, "capturedFrameCount") ?? 0,
                    NullableInt(step, "capturedDurationMilliseconds") ?? 0,
                    NullableLong(step, "estimatedRgbaBytes") ?? 0);
                var existing = result.GetValueOrDefault(sequence) ?? [];
                result[sequence] = existing.Concat([evidence]).ToArray();
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
        }
    }

    private static int? NullableInt(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind is JsonValueKind.Number
            ? value.GetInt32()
            : null;

    private static long? NullableLong(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind is JsonValueKind.Number
            ? value.GetInt64()
            : null;

    private static bool PathsEqual(string left, string right) =>
        Path.GetFullPath(left).Equals(Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
}
