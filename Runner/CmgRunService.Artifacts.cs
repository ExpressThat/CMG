using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public sealed partial class CmgRunService
{
    private const int GifPaletteWarningThreshold = 240;

    internal static IReadOnlyList<string> ResolveFiles(string path)
    {
        if (File.Exists(path))
        {
            return [Path.GetFullPath(path)];
        }

        return Directory.Exists(path)
            ? Directory.GetFiles(path, "*.cmgscript", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray()
            : [];
    }

    internal static FileInfo? BuildGifPath(CmgTestCase test, CmgRunOptions options)
    {
        if (options.GifDirectory is null)
        {
            return null;
        }

        options.GifDirectory.Create();
        var project = string.IsNullOrWhiteSpace(options.ProjectName) ? string.Empty : $"{SafeName(options.ProjectName)}-";
        var browser = options.BrowserKind.ToString().ToLowerInvariant();
        var shard = options.ShardCount > 1 ? $"-shard-{options.ShardIndex}-of-{options.ShardCount}" : string.Empty;
        var safeName = string.Concat(test.Name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        return new FileInfo(Path.Combine(options.GifDirectory.FullName, $"{project}{browser}{shard}-{safeName}.gif"));
    }

    private static void WriteReports(CmgRunOptions options, IReadOnlyList<CmgTestResult> tests)
    {
        WriteReport(options.JsonReport, CmgJsonReportWriter.Write(tests));
        WriteReport(options.HtmlReport, CmgHtmlReportWriter.Write(tests));
        WriteReport(options.JUnitReport, CmgJUnitReportWriter.Write(tests));
    }

    private static void WriteReport(FileInfo? report, string content)
    {
        if (report is null)
        {
            return;
        }

        report.Directory?.Create();
        File.WriteAllText(report.FullName, content);
    }

    internal static IReadOnlyList<string> GifSizeWarnings(CmgTestResult test, CmgRunOptions options)
    {
        if (options.GifWarnSizeBytes is not long threshold)
        {
            return [];
        }

        var warnings = new List<string>();
        foreach (var path in GifPaths(test.GifPath))
        {
            var file = new FileInfo(path);
            if (file.Exists && file.Length > threshold)
            {
                warnings.Add($"GIF_WARN_SIZE test={Quote(test.Name)} path={Quote(file.FullName)} sizeBytes={file.Length} thresholdBytes={threshold}");
            }
        }

        return warnings;
    }

    internal static IReadOnlyList<string> GifPaletteWarnings(CmgTestResult test)
    {
        var warnings = new List<string>();
        foreach (var path in GifPaths(test.GifPath))
        {
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                continue;
            }

            try
            {
                var metadata = GifInspector.Inspect(file);
                if (HasPalettePressure(metadata))
                {
                    warnings.Add(
                        $"GIF_WARN_PALETTE test={Quote(test.Name)} path={Quote(file.FullName)} " +
                        $"paletteColors={metadata.PaletteColors} thresholdColors={GifPaletteWarningThreshold} palette={metadata.Palette}");
                }
            }
            catch (Exception)
            {
            }
        }

        return warnings;
    }

    internal static (CmgTestResult Result, IReadOnlyList<string> Output) ApplyGifDurationGuard(CmgTestResult test, CmgRunOptions options)
    {
        if (options.GifMaxDurationMilliseconds is not int threshold)
        {
            return (test, []);
        }

        var output = new List<string>();
        var reasons = new List<string>();
        foreach (var path in GifPaths(test.GifPath))
        {
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                continue;
            }

            try
            {
                var metadata = GifInspector.Inspect(file);
                if (metadata.DurationMilliseconds <= threshold)
                {
                    continue;
                }

                output.Add(
                    $"GIF_MAX_DURATION test={Quote(test.Name)} path={Quote(file.FullName)} " +
                    $"durationMs={metadata.DurationMilliseconds} thresholdMs={threshold}");
                reasons.Add($"GIF '{file.FullName}' duration {metadata.DurationMilliseconds}ms exceeded max {threshold}ms.");
            }
            catch (Exception exception)
            {
                output.Add($"GIF_MAX_DURATION_INSPECT_FAILED test={Quote(test.Name)} path={Quote(file.FullName)} reason={Quote(exception.Message)}");
            }
        }

        if (reasons.Count is 0)
        {
            return (test, output);
        }

        var reason = MergeError(test.Error, string.Join(' ', reasons));
        return (test with { Success = false, Error = reason }, output);
    }

    internal static (CmgTestResult Result, IReadOnlyList<string> Output) ApplyGifSizeGuard(CmgTestResult test, CmgRunOptions options)
    {
        if (options.GifMaxSizeBytes is not long threshold)
        {
            return (test, []);
        }

        var output = new List<string>();
        var reasons = new List<string>();
        foreach (var path in GifPaths(test.GifPath))
        {
            var file = new FileInfo(path);
            if (!file.Exists || file.Length <= threshold)
            {
                continue;
            }

            output.Add(
                $"GIF_MAX_SIZE test={Quote(test.Name)} path={Quote(file.FullName)} " +
                $"sizeBytes={file.Length} thresholdBytes={threshold}");
            reasons.Add($"GIF '{file.FullName}' size {file.Length} bytes exceeded max {threshold} bytes.");
        }

        if (reasons.Count is 0)
        {
            return (test, output);
        }

        var reason = MergeError(test.Error, string.Join(' ', reasons));
        return (test with { Success = false, Error = reason }, output);
    }

    private static string MergeError(string? existing, string addition) =>
        string.IsNullOrWhiteSpace(existing) ? addition : $"{existing} {addition}";

    private static bool HasPalettePressure(GifInspection metadata)
    {
        if (metadata.PaletteColors.StartsWith('>'))
        {
            return true;
        }

        return int.TryParse(metadata.PaletteColors, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var colors) &&
            colors >= GifPaletteWarningThreshold;
    }

    private static IEnumerable<string> GifPaths(string? gifPath) =>
        string.IsNullOrWhiteSpace(gifPath)
            ? []
            : gifPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    internal static string SafeName(string name) =>
        string.Concat(name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
}
