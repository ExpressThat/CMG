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
        if (options.Workers <= 1 || options.MaxFailures > 0 || selectedTests.Any(HasOrderingHooks))
        {
            return RunSelectedTestsSequentially(selectedTests, remoteDebuggingUrl, options, tests, output);
        }

        var results = new CmgTestResult[selectedTests.Count];
        Parallel.ForEach(
            selectedTests.Select((test, index) => (test, index)),
            new ParallelOptions { MaxDegreeOfParallelism = options.Workers },
            item => results[item.index] = RunScheduledTest(item.test, remoteDebuggingUrl, options));

        foreach (var result in results)
        {
            tests.Add(result);
            output.Add(TestOutput(StatusWord(result), result.Name, options));
        }

        return true;
    }

    private bool RunSelectedTestsSequentially(
        IReadOnlyList<CmgTestCase> selectedTests,
        string remoteDebuggingUrl,
        CmgRunOptions options,
        List<CmgTestResult> tests,
        List<string> output)
    {
        foreach (var test in selectedTests)
        {
            var result = RunScheduledTest(test, remoteDebuggingUrl, options);
            tests.Add(result);
            output.Add(TestOutput(StatusWord(result), result.Name, options));
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

    private static bool HasOrderingHooks(CmgTestCase test) =>
        test.RootBeforeAll.Count > 0 || test.RootAfterAll.Count > 0 ||
        test.SuiteBeforeAll.Count > 0 || test.SuiteAfterAll.Count > 0;

    private static string StatusWord(CmgTestResult result) =>
        result.Status.Equals("skipped", StringComparison.OrdinalIgnoreCase) ? "SKIP" : result.Success ? "PASS" : "FAIL";

    private static string TestOutput(string status, string name, CmgRunOptions options) =>
        string.IsNullOrWhiteSpace(options.ProjectName)
            ? $"TEST {status} {name}"
            : $"TEST {status} [{options.ProjectName}] {name}";
}
