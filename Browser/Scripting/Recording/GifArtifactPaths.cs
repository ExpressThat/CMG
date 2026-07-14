namespace CMG.Browser.Scripting.Recording;

public static class GifArtifactPaths
{
    public static string Timeline(string artifactPath) => Sidecar(artifactPath, ".timeline.json");
    public static string Debug(string artifactPath) => Sidecar(artifactPath, ".debug.json");
    public static string Narration(string artifactPath) => Sidecar(artifactPath, ".narration.txt");
    public static string Frames(string artifactPath) => Sidecar(artifactPath, ".frames");

    public static string SidecarFileName(string artifactPath, string suffix) =>
        Path.GetFileName(Sidecar(artifactPath, suffix));

    private static string Sidecar(string artifactPath, string suffix)
    {
        var fullPath = Path.GetFullPath(artifactPath);
        return Path.GetExtension(fullPath).Equals(".gif", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(fullPath, suffix)
            : fullPath + suffix;
    }
}
