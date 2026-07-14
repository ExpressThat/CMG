namespace CMG.Runner;

public sealed partial class CmgRunService
{
    private static CmgTestResult ParseFailure(string file, string? error, CmgRunOptions options) =>
        new(Path.GetFileName(file), file, false, [], error, null, [])
        {
            Project = options.ProjectName,
            Browser = options.BrowserKind,
            BrowserPort = options.BrowserPort
        };

    private static CmgTestResult SkippedTest(CmgTestCase test, CmgRunOptions options)
    {
        var reason = test.Options.TryGetValue("reason", out var value) ? value : "Skipped by test option.";
        return new CmgTestResult(test.Name, test.SourcePath, true, [], reason, null, [])
        {
            Tags = test.Options.TryGetValue("tag", out var tag) ? tag : string.Empty,
            Status = "skipped",
            Project = options.ProjectName,
            Browser = options.BrowserKind,
            BrowserPort = options.BrowserPort,
            Annotations = test.Annotations
        };
    }

    private static bool IsSkipped(CmgTestCase test) => IsTruthyOption(test, "skip");

    private static bool IsTruthyOption(CmgTestCase test, string name) =>
        test.Options.TryGetValue(name, out var value) &&
        (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("yes", StringComparison.OrdinalIgnoreCase));
}
