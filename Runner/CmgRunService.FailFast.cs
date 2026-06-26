namespace CMG.Runner;

public sealed partial class CmgRunService
{
    internal static bool HasReachedMaxFailures(IReadOnlyList<CmgTestResult> tests, int maxFailures) =>
        maxFailures > 0 && tests.Count(test => !test.Success) >= maxFailures;

    private static bool ContinueAfterFailureLimit(
        CmgRunOptions options,
        IReadOnlyList<CmgTestResult> tests,
        List<string> output)
    {
        if (!HasReachedMaxFailures(tests, options.MaxFailures))
        {
            return true;
        }

        output.Add($"RUN STOP maxFailures={options.MaxFailures}");
        return false;
    }
}
