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

    public CmgRunService(
        BrowserStateStore stateStore,
        BrowserAutomationClientFactory automationClientFactory,
        BrowserScriptRunner scriptRunner,
        CmgDslParser parser,
        CmgTestPlanner planner,
        CmgActionLowerer lowerer)
    {
        this.stateStore = stateStore;
        this.automationClientFactory = automationClientFactory;
        this.scriptRunner = scriptRunner;
        this.parser = parser;
        this.planner = planner;
        this.lowerer = lowerer;
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

        foreach (var test in planner.Plan(parse.Document))
        {
            var result = RunTest(test, remoteDebuggingUrl, options);
            tests.Add(result);
            output.Add($"TEST {(result.Success ? "PASS" : "FAIL")} {result.Name}");
        }
    }

    private CmgTestResult RunTest(CmgTestCase test, string remoteDebuggingUrl, CmgRunOptions options)
    {
        var executor = new CmgVisualSegmentExecutor(scriptRunner, automationClientFactory.Create(options.BrowserKind), lowerer);
        var result = executor.Run(test, remoteDebuggingUrl, options);
        return result;
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
