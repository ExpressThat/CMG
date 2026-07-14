using System.Text.RegularExpressions;

namespace CMG.Runner;

internal static partial class CmgDownloadArtifacts
{
    public static IReadOnlyList<CmgDownloadArtifact> For(CmgTestResult test) =>
        test.Output.Concat(test.Steps.SelectMany(step => step.Output))
            .Select(Parse)
            .Where(artifact => artifact is not null)
            .Cast<CmgDownloadArtifact>()
            .DistinctBy(artifact => artifact.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static CmgDownloadArtifact? Parse(string line)
    {
        var match = DownloadLine().Match(line);
        if (!match.Success) return null;
        var path = match.Groups[1].Value.Trim().Trim('"');
        try
        {
            var fullPath = Path.GetFullPath(path);
            return File.Exists(fullPath)
                ? new(fullPath, Path.GetFileName(fullPath), new FileInfo(fullPath).Length)
                : null;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    [GeneratedRegex(@"^DOWNLOAD\s+\d+\s+(.+)$")]
    private static partial Regex DownloadLine();
}

internal sealed record CmgDownloadArtifact(string Path, string Name, long SizeBytes);
