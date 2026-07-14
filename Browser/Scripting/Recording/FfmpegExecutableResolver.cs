namespace CMG.Browser.Scripting.Recording;

internal sealed record FfmpegExecutable(string Path, bool IsBundled);

internal static class FfmpegExecutableResolver
{
    public static FfmpegExecutable Resolve(
        string? configuredPath,
        string? baseDirectory = null,
        string? environmentPath = null)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath)) return new(configuredPath, false);

        baseDirectory ??= AppContext.BaseDirectory;
        var sibling = System.IO.Path.Combine(
            baseDirectory,
            OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg");
        if (File.Exists(sibling)) return new(sibling, true);

        environmentPath ??= Environment.GetEnvironmentVariable("CMG_FFMPEG");
        return !string.IsNullOrWhiteSpace(environmentPath)
            ? new(environmentPath, false)
            : new("ffmpeg", false);
    }
}
