namespace CMG.Browser.Scripting.Recording;

public enum GifArtifactFormat { Gif, Apng, Webp, Mp4 }

public static class GifArtifactFormatParser
{
    public const string Values = "gif, apng, webp, mp4";

    public static GifArtifactFormat Parse(string? value, string context)
    {
        if (string.IsNullOrWhiteSpace(value)) return GifArtifactFormat.Gif;
        return value.Trim().ToLowerInvariant() switch
        {
            "gif" => GifArtifactFormat.Gif,
            "apng" or "png" => GifArtifactFormat.Apng,
            "webp" => GifArtifactFormat.Webp,
            "mp4" or "h264" => GifArtifactFormat.Mp4,
            _ => throw new CMG.Browser.Scripting.ScriptExecutionException(
                $"{context} option format= must be one of: {Values}.")
        };
    }

    public static string Extension(this GifArtifactFormat format) => format switch
    {
        GifArtifactFormat.Apng => ".apng",
        GifArtifactFormat.Webp => ".webp",
        GifArtifactFormat.Mp4 => ".mp4",
        _ => ".gif"
    };

    public static string WithExtension(string path, GifArtifactFormat format)
    {
        var extension = Path.GetExtension(path);
        if (string.IsNullOrWhiteSpace(extension) || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
            return Path.ChangeExtension(path, format.Extension());
        if (!extension.Equals(format.Extension(), StringComparison.OrdinalIgnoreCase))
            throw new CMG.Browser.Scripting.ScriptExecutionException(
                $"Recording format {format.ToString().ToLowerInvariant()} requires a '{format.Extension()}' output path, got '{path}'.");
        return path;
    }
}
