using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public sealed partial class CmgRunService
{
    internal static IReadOnlyList<string> GifAuthoringWarnings(
        IReadOnlyList<CmgTestCase> tests,
        CmgRunOptions options)
    {
        if (options.GifDirectory is null || GifRecordingPolicy.IsDisabled) return [];
        var unbounded = tests.Count(test =>
            CmgGifRetentionPolicy.TryParse(test, options, out var policy, out _) &&
            policy.Mode is CmgGifRetentionMode.Always && policy.SampleRate is 1 && !policy.CleanPassed);
        return unbounded > 20
            ? [$"GIF_RETENTION_WARN tests={unbounded} threshold=20 reason=large-suite suggestion=--gif-on-failure"]
            : [];
    }

    internal static CmgTestResult ApplyGifRetention(
        CmgTestCase test,
        IReadOnlyList<CmgTestResult> attempts,
        CmgGifRetentionPolicy policy)
    {
        var last = attempts.LastOrDefault() ??
            new CmgTestResult(test.Name, test.SourcePath, false, [], "Test did not run.", null, []);
        var commandAttempts = policy.Mode switch
        {
            CmgGifRetentionMode.OnFailure when last.Success => [],
            CmgGifRetentionMode.OnFailure => attempts,
            CmgGifRetentionMode.OnRetry => attempts.Where(attempt => !attempt.Success).ToArray(),
            CmgGifRetentionMode.Off => [],
            _ => attempts
        };
        var keptCommands = CommandPaths(commandAttempts);
        var allCommands = CommandPaths(attempts);
        var removedCommands = allCommands.Except(keptCommands, StringComparer.OrdinalIgnoreCase).ToArray();
        var removedArtifacts = removedCommands.Where(GifFamilyExists).ToArray();
        try
        {
            foreach (var path in removedArtifacts) DeleteGifFamily(path);
        }
        catch (InvalidOperationException exception)
        {
            return last with
            {
                Success = false,
                Error = $"GIF retention cleanup failed. {exception.Message}"
            };
        }

        var focused = attempts.SelectMany(attempt => GifPaths(attempt.GifPath))
            .Where(path => !allCommands.Contains(path, StringComparer.OrdinalIgnoreCase));
        var paths = focused.Concat(keptCommands).Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var qualities = attempts.SelectMany(attempt => attempt.GifQualities)
            .Where(value => paths.Contains(value.Key, StringComparer.OrdinalIgnoreCase))
            .GroupBy(value => value.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.OrdinalIgnoreCase);
        var artifactLines = attempts.SelectMany(attempt => attempt.Output.Where(IsGifRecordingOutput))
            .Where(line => !removedCommands.Any(path => ReferencesPath(line, path)));
        var retentionLines = removedArtifacts.Select(path =>
            $"GIF_RETENTION test={Quote(test.Name)} path={Quote(Path.GetFullPath(path))} action=deleted mode={Mode(policy.Mode)}").ToArray();
        return last with
        {
            GifPath = paths.Length == 0 ? null : string.Join(';', paths),
            GifQualities = qualities,
            Output = last.Output.Where(line => !IsGifRecordingOutput(line))
                .Concat(artifactLines).Concat(retentionLines).Distinct().ToArray(),
            CleanGifPathsAfterReport = last.Success && policy.CleanPassed ? keptCommands : []
        };
    }

    internal static IReadOnlyList<string> CleanPassedGifsAfterReports(IReadOnlyList<CmgTestResult> tests)
    {
        var output = new List<string>();
        foreach (var test in tests.Where(test => test.Success))
        {
            foreach (var path in test.CleanGifPathsAfterReport.Where(GifFamilyExists))
            {
                DeleteGifFamily(path);
                output.Add($"GIF_CLEAN_PASSED test={Quote(test.Name)} path={Quote(Path.GetFullPath(path))} status=deleted");
            }
        }
        return output;
    }

    private static string? TryCleanPassedGifsAfterReports(
        IReadOnlyList<CmgTestResult> tests,
        ICollection<string> output)
    {
        try
        {
            foreach (var line in CleanPassedGifsAfterReports(tests)) output.Add(line);
            return null;
        }
        catch (InvalidOperationException exception)
        {
            return $"GIF post-report cleanup failed. {exception.Message}";
        }
    }

    private static void DeleteGifFamily(string path)
    {
        try
        {
            var narrations = GifArtifactFamily.NarrationPaths(path);
            DeleteFile(path);
            DeleteFile(Path.ChangeExtension(path, ".timeline.json"));
            DeleteFile(Path.ChangeExtension(path, ".debug.json"));
            foreach (var narration in narrations) DeleteFile(narration);
            var frames = Path.ChangeExtension(path, null) + ".frames";
            if (Directory.Exists(frames)) Directory.Delete(frames, recursive: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Could not delete GIF artifact family for '{Path.GetFullPath(path)}': {exception.Message}", exception);
        }
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }

    private static bool GifFamilyExists(string path) =>
        File.Exists(path) ||
        File.Exists(Path.ChangeExtension(path, ".timeline.json")) ||
        File.Exists(Path.ChangeExtension(path, ".debug.json")) ||
        GifArtifactFamily.NarrationPaths(path).Any(File.Exists) ||
        Directory.Exists(Path.ChangeExtension(path, null) + ".frames");

    private static string Mode(CmgGifRetentionMode mode) => mode switch
    {
        CmgGifRetentionMode.OnFailure => "onFailure",
        CmgGifRetentionMode.OnRetry => "onRetry",
        CmgGifRetentionMode.Off => "off",
        _ => "always"
    };

    private static bool IsGifRecordingOutput(string line) =>
        line.StartsWith("GIF ", StringComparison.Ordinal) || IsGifArtifactOutput(line);

    private static string[] CommandPaths(IEnumerable<CmgTestResult> attempts) =>
        attempts.Select(attempt => attempt.CommandGifPath)
            .Where(path => !string.IsNullOrWhiteSpace(path)).Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static bool ReferencesPath(string line, string path)
    {
        var normalized = line.Replace("\\\\", "\\", StringComparison.Ordinal);
        return new[]
        {
            path,
            Path.ChangeExtension(path, ".timeline.json"),
            Path.ChangeExtension(path, ".debug.json"),
            Path.ChangeExtension(path, null) + ".frames"
        }.Concat(GifArtifactFamily.NarrationPaths(path))
            .Any(candidate => normalized.Contains(candidate, StringComparison.OrdinalIgnoreCase));
    }
}
