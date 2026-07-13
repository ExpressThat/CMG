namespace CMG.Runner;

public sealed partial class CmgRunService
{
    private bool RunSelectedTests(
        IReadOnlyList<CmgTestCase> selectedTests,
        string remoteDebuggingUrl,
        CmgRunOptions options,
        List<CmgTestResult> tests,
        List<string> output)
    {
        foreach (var test in selectedTests)
        {
            var rawResult = RunScheduledTest(test, remoteDebuggingUrl, options);
            var (sizedResult, sizeOutput) = ApplyGifSizeGuard(rawResult, options);
            var (result, durationOutput) = ApplyGifDurationGuard(sizedResult, options);
            tests.Add(result);
            output.Add(TestOutput(StatusWord(result), result.Name, options));
            output.AddRange(result.Output.Where(IsGifArtifactOutput).Distinct(StringComparer.Ordinal));
            output.AddRange(sizeOutput);
            output.AddRange(durationOutput);
            output.AddRange(GifSizeWarnings(result, options));
            output.AddRange(GifPaletteWarnings(result));
            if (!ContinueAfterFailureLimit(options, tests, output))
            {
                return false;
            }
        }

        return true;
    }

    private CmgTestResult RunScheduledTest(CmgTestCase test, string remoteDebuggingUrl, CmgRunOptions options)
    {
        if (IsSkipped(test))
        {
            return SkippedTest(test, options);
        }

        var validation = validator.Validate(test);
        if (!validation.Success)
        {
            return new CmgTestResult(
                test.Name,
                test.SourcePath,
                false,
                [],
                validation.Error,
                null,
                [new CmgStepResult(validation.LineNumber, validation.Action, false, [], validation.Error, null)])
            {
                Tags = test.Options.TryGetValue("tag", out var tag) ? tag : string.Empty,
                Annotations = test.Annotations,
                Project = options.ProjectName
            };
        }

        return RunTestWithRetries(test, remoteDebuggingUrl, options);
    }

    private static string StatusWord(CmgTestResult result) =>
        result.Status.Equals("skipped", StringComparison.OrdinalIgnoreCase) ? "SKIP" : result.Success ? "PASS" : "FAIL";

    private static string TestOutput(string status, string name, CmgRunOptions options) =>
        string.IsNullOrWhiteSpace(options.ProjectName)
            ? $"TEST {status} {name}"
            : $"TEST {status} [{options.ProjectName}] {name}";

    private static bool IsGifArtifactOutput(string line) =>
        line.StartsWith("GIF_FRAMES ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_TIMELINE ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_DEBUG ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_CAPTURE_STATS ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_WARN_UNCHANGED ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_WARN_BLANK ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_WARN_COLOR_PROFILE ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_WARN_MULTIPLE_TARGETS ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_WARN_TINY_TARGET ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_WARN_SCROLLED ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_WARN_NON_VISUAL ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_WAIT_COMPRESSION ", StringComparison.Ordinal) ||
        line.StartsWith("GIF_FAILURE_CAPTION ", StringComparison.Ordinal);
}
