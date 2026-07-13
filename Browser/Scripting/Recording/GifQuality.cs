namespace CMG.Browser.Scripting.Recording;

public enum GifQuality
{
    Archival,
    Highest,
    High,
    Medium,
    Low
}

public static class GifQualityParser
{
    public static bool TryParse(string? value, out GifQuality quality)
    {
        quality = GifQuality.Highest;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "archival" => Set(out quality, GifQuality.Archival),
            "highest" or "best" => Set(out quality, GifQuality.Highest),
            "high" => Set(out quality, GifQuality.High),
            "medium" => Set(out quality, GifQuality.Medium),
            "low" => Set(out quality, GifQuality.Low),
            _ => false
        };
    }

    public static string Values => "archival, highest, high, medium, low";

    private static bool Set(out GifQuality target, GifQuality value)
    {
        target = value;
        return true;
    }
}
