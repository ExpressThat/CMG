namespace CMG.Runner;

internal static class GifArtifactRetentionCleaner
{
    public static IReadOnlyList<string> Clean(DirectoryInfo directory, int days, DateTimeOffset? now = null)
    {
        if (days < 1) throw new ArgumentOutOfRangeException(nameof(days));
        if (!directory.Exists) return [];
        var cutoff = (now ?? DateTimeOffset.UtcNow).UtcDateTime.AddDays(-days);
        var output = new List<string>();
        foreach (var gif in directory.EnumerateFiles("*.gif", SearchOption.TopDirectoryOnly)
                     .Where(file => file.LastWriteTimeUtc < cutoff))
        {
            Delete(gif.FullName);
            Delete(Path.ChangeExtension(gif.FullName, ".timeline.json"));
            Delete(Path.ChangeExtension(gif.FullName, ".debug.json"));
            var frames = Path.ChangeExtension(gif.FullName, null) + ".frames";
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
}
