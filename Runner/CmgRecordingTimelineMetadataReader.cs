using System.Text.Json;

namespace CMG.Runner;

internal sealed record CmgRecordingTimelineMetadata(
    int Frames, int DurationMilliseconds, int Width, int Height, long SizeBytes);

internal static class CmgRecordingTimelineMetadataReader
{
    public static CmgRecordingTimelineMetadata? Read(string artifactPath)
    {
        var timeline = CMG.Browser.Scripting.Recording.GifArtifactPaths.Timeline(artifactPath);
        if (!File.Exists(timeline)) return null;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(timeline));
            var root = document.RootElement;
            return new(
                root.GetProperty("frameCount").GetInt32(),
                root.GetProperty("durationMilliseconds").GetInt32(),
                root.GetProperty("width").GetInt32(),
                root.GetProperty("height").GetInt32(),
                root.GetProperty("fileSizeBytes").GetInt64());
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or
            KeyNotFoundException or InvalidOperationException or FormatException or OverflowException)
        {
            return null;
        }
    }
}
