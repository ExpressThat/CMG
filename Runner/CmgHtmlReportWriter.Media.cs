namespace CMG.Runner;

public static partial class CmgHtmlReportWriter
{
    private static string RecordingMedia(string path, string source, string alt)
    {
        if (Path.GetExtension(path).Equals(".mp4", StringComparison.OrdinalIgnoreCase))
            return $"<video controls preload=\"metadata\" aria-label=\"{alt}\"><source src=\"{source}\" type=\"video/mp4\"></video><a href=\"{source}\">Open MP4</a>";
        return $"<a href=\"{source}\"><img src=\"{source}\" alt=\"{alt}\"></a>";
    }

    private static string RecordingLabel(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".apng" => "APNG", ".webp" => "WebP", ".mp4" => "MP4", _ => "GIF"
    };
}
