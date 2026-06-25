using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Runner;

public interface ICmgRunService
{
    CmgRunResult Run(string path, CmgRunOptions options);
}

public sealed class CmgRunService : ICmgRunService
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

    public CmgRunService(
        BrowserStateStore stateStore,
        BrowserAutomationClientFactory automationClientFactory,
        BrowserScriptRunner scriptRunner,
        CmgDslParser parser,
        CmgTestPlanner planner,
        CmgActionLowerer lowerer,
        CmgValidator validator,
        CmgApiRequestRunner apiRequestRunner,
        CmgStorageStateRunner storageStateRunner)
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
    }

    public CmgRunResult Run(string path, CmgRunOptions options)
    {
        var files = ResolveFiles(path);
        if (files.Count is 0)
        {
            return CmgRunResult.Fail($"No CMG script files matched '{path}'.");
        }

        var state = stateStore.Load(options.BrowserKind);
        if (state is null)
        {
            return CmgRunResult.Fail($"No CMG-controlled {options.BrowserKind.DisplayName()} instance is running.");
        }

        var tests = new List<CmgTestResult>();
        var output = new List<string>();
        foreach (var file in files)
        {
            RunFile(file, state.RemoteDebuggingUrl, options, tests, output);
        }

        WriteReports(options, tests);
        CmgTraceWriter.Write(options.TraceDirectory, tests);
        return new CmgRunResult(tests.All(test => test.Success), output, tests, null);
    }

    private void RunFile(string file, string remoteDebuggingUrl, CmgRunOptions options, List<CmgTestResult> tests, List<string> output)
    {
        var parse = parser.Parse(file, File.ReadAllText(file));
        if (!parse.Success || parse.Document is null)
        {
            tests.Add(new CmgTestResult(Path.GetFileName(file), file, false, [], parse.Error, null, []));
            output.Add($"TEST FAIL {Path.GetFileName(file)}");
            return;
        }

        var plannedTests = planner.Plan(parse.Document).Where(test => ShouldRun(test, options)).ToArray();
        foreach (var test in ApplyShard(plannedTests, options))
        {
            var validation = validator.Validate(test);
            if (!validation.Success)
            {
                tests.Add(new CmgTestResult(
                    test.Name,
                    test.SourcePath,
                    false,
                    [],
                    validation.Error,
                    null,
                    [new CmgStepResult(validation.LineNumber, validation.Action, false, [], validation.Error, null)]));
                output.Add($"TEST FAIL {test.Name}");
                continue;
            }

            var result = RunTestWithRetries(test, remoteDebuggingUrl, options);
            tests.Add(result);
            output.Add($"TEST {(result.Success ? "PASS" : "FAIL")} {result.Name}");
        }
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
            storageStateRunner);
        return executor.Run(test, remoteDebuggingUrl, options, attempt) with
        {
            Tags = test.Options.TryGetValue("tag", out var tag) ? tag : string.Empty
        };
    }

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
        var safeName = string.Concat(test.Name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        return new FileInfo(Path.Combine(options.GifDirectory.FullName, $"{safeName}.gif"));
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
}
