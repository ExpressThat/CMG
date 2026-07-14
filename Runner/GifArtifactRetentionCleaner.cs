using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

internal static class GifArtifactRetentionCleaner
{
    public static IReadOnlyList<string> Clean(DirectoryInfo directory, int days, DateTimeOffset? now = null)
    {
        if (days < 1) throw new ArgumentOutOfRangeException(nameof(days));
        if (!directory.Exists) return [];
        var cutoff = (now ?? DateTimeOffset.UtcNow).UtcDateTime.AddDays(-days);
        var output = new List<string>();
        foreach (var gif in directory.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                     .Where(file => GifArtifactExtensions.Contains(file.Extension))
                     .Where(file => file.LastWriteTimeUtc < cutoff))
        {
            var narrations = GifArtifactFamily.NarrationPaths(gif.FullName);
            Delete(gif.FullName);
            Delete(GifArtifactPaths.Timeline(gif.FullName));
            Delete(GifArtifactPaths.Debug(gif.FullName));
            foreach (var narration in narrations) Delete(narration);
            var frames = GifArtifactPaths.Frames(gif.FullName);
            if (Directory.Exists(frames)) Directory.Delete(frames, recursive: true);
            output.Add($"GIF_RETENTION_CLEANUP path={Quote(gif.FullName)} ageDays={days} action=deleted");
        }
        return output;
    }

    private static void Delete(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static readonly HashSet<string> GifArtifactExtensions =
        new([".gif", ".apng", ".webp", ".mp4"], StringComparer.OrdinalIgnoreCase);
}
