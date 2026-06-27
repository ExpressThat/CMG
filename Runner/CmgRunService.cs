using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Runner;

public interface ICmgRunService
{
    CmgRunResult Run(string path, CmgRunOptions options);
}

public sealed partial class CmgRunService : ICmgRunService
{
    private readonly BrowserStateStore stateStore;
    private readonly BrowserAutomationClientFactory automationClientFactory;
    private readonly BrowserScriptRunner scriptRunner;
    private readonly CmgDslParser parser;
    private readonly CmgTestPlanner planner;
    private readonly CmgActionLowerer lowerer;
    private readonly CmgValidator validator;
    private readonly CmgApiRequestRunner apiRequestRunner;
    private readonly CmgStorageStateRunner storageStateRunner;
    private readonly CmgVisualAssertionRunner visualAssertionRunner;
    private readonly CmgUploadRunner uploadRunner;

    public CmgRunService(
        BrowserStateStore stateStore,
        BrowserAutomationClientFactory automationClientFactory,
        BrowserScriptRunner scriptRunner,
        CmgDslParser parser,
        CmgTestPlanner planner,
        CmgActionLowerer lowerer,
        CmgValidator validator,
        CmgApiRequestRunner apiRequestRunner,
        CmgStorageStateRunner storageStateRunner,
        CmgVisualAssertionRunner visualAssertionRunner,
        CmgUploadRunner uploadRunner)
    {
        this.stateStore = stateStore;
        this.automationClientFactory = automationClientFactory;
        this.scriptRunner = scriptRunner;
        this.parser = parser;
        this.planner = planner;
        this.lowerer = lowerer;
        this.validator = validator;
        this.apiRequestRunner = apiRequestRunner;
        this.storageStateRunner = storageStateRunner;
        this.visualAssertionRunner = visualAssertionRunner;
        this.uploadRunner = uploadRunner;
    }

    public CmgRunResult Run(string path, CmgRunOptions options)
    {
        var files = ResolveFiles(path);
        if (files.Count is 0)
        {
            return CmgRunResult.Fail($"No CMG script files matched '{path}'.");
        }

        if (options.ListOnly)
        {
            return ListTests(files, options);
        }

        var state = stateStore.Load(options.BrowserKind, options.BrowserPort);
        if (state is null)
        {
            return CmgRunResult.Fail($"No CMG-controlled {options.BrowserKind.DisplayName()} instance is running.");
        }

        var tests = new List<CmgTestResult>();
        var output = new List<string>();
        foreach (var file in files)
        {
            if (!RunFile(file, state.RemoteDebuggingUrl, options, tests, output))
            {
                break;
            }
        }

        WriteReports(options, tests);
        CmgTraceWriter.Write(options.TraceDirectory, tests);
        return new CmgRunResult(tests.All(test => test.Success), output, tests, null);
    }

    private bool RunFile(string file, string remoteDebuggingUrl, CmgRunOptions options, List<CmgTestResult> tests, List<string> output)
    {
        var parse = parser.Parse(file, File.ReadAllText(file));
        if (!parse.Success || parse.Document is null)
        {
            tests.Add(new CmgTestResult(Path.GetFileName(file), file, false, [], parse.Error, null, []) { Project = options.ProjectName });
            output.Add(TestOutput("FAIL", Path.GetFileName(file), options));
            return ContinueAfterFailureLimit(options, tests, output);
        }

        var plannedTests = SelectFocusedTests(planner.Plan(parse.Document).Where(test => ShouldRun(test, options)).ToArray());
        var repeatedTests = RepeatTests(plannedTests, options.RepeatEach);
        var selectedTests = ApplyOnceHooks(ApplyShard(repeatedTests, options).ToArray());
        return RunSelectedTests(selectedTests.ToArray(), remoteDebuggingUrl, options, tests, output);
    }

    private CmgTestResult RunTestWithRetries(CmgTestCase test, string remoteDebuggingUrl, CmgRunOptions options)
    {
        CmgTestResult? last = null;
        for (var attempt = 0; attempt <= options.Retries; attempt++)
        {
            last = RunTest(test, remoteDebuggingUrl, options, attempt + 1);
            if (last.Success)
            {
                return last;
            }
        }

        return last ?? new CmgTestResult(test.Name, test.SourcePath, false, [], "Test did not run.", null, []);
    }

    private CmgTestResult RunTest(CmgTestCase test, string remoteDebuggingUrl, CmgRunOptions options, int attempt)
    {
        var executor = new CmgVisualSegmentExecutor(
            scriptRunner,
            automationClientFactory.Create(options.BrowserKind),
            lowerer,
            apiRequestRunner,
            storageStateRunner,
            visualAssertionRunner,
            uploadRunner);
        return executor.Run(test, remoteDebuggingUrl, options, attempt) with
        {
            Tags = test.Options.TryGetValue("tag", out var tag) ? tag : string.Empty,
            Annotations = test.Annotations,
            Project = options.ProjectName
        };
    }

    private static CmgTestResult SkippedTest(CmgTestCase test, CmgRunOptions options)
    {
        var reason = test.Options.TryGetValue("reason", out var value) ? value : "Skipped by test option.";
        return new CmgTestResult(test.Name, test.SourcePath, true, [], reason, null, [])
        {
            Tags = test.Options.TryGetValue("tag", out var tag) ? tag : string.Empty,
            Status = "skipped",
            Project = options.ProjectName,
            Annotations = test.Annotations
        };
    }

    private static bool IsSkipped(CmgTestCase test) => IsTruthyOption(test, "skip");

    private static bool IsTruthyOption(CmgTestCase test, string name) =>
        test.Options.TryGetValue(name, out var value) &&
        (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("yes", StringComparison.OrdinalIgnoreCase));

    private static bool ShouldRun(CmgTestCase test, CmgRunOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Grep) &&
            !test.Name.Contains(options.Grep, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(options.Tag) ||
            (test.Options.TryGetValue("tag", out var tag) &&
            tag.Split(',', StringSplitOptions.TrimEntries).Contains(options.Tag, StringComparer.OrdinalIgnoreCase));
    }

    private static IEnumerable<CmgTestCase> ApplyShard(IReadOnlyList<CmgTestCase> tests, CmgRunOptions options)
    {
        for (var index = 0; index < tests.Count; index++)
        {
            if (index % options.ShardCount == options.ShardIndex - 1)
            {
                yield return tests[index];
            }
        }
    }

}
