using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private readonly BrowserScriptRunner scriptRunner;
    private readonly IBrowserAutomationClient automationClient;
    private readonly CmgActionLowerer lowerer;
    private readonly CmgApiRequestRunner apiRequestRunner;
    private readonly CmgStorageStateRunner storageStateRunner;
    private readonly CmgVisualAssertionRunner visualAssertionRunner;
    private readonly CmgUploadRunner uploadRunner;

    public CmgVisualSegmentExecutor(
        BrowserScriptRunner scriptRunner,
        IBrowserAutomationClient automationClient,
        CmgActionLowerer lowerer,
        CmgApiRequestRunner apiRequestRunner,
        CmgStorageStateRunner storageStateRunner,
        CmgVisualAssertionRunner visualAssertionRunner,
        CmgUploadRunner uploadRunner)
    {
        this.scriptRunner = scriptRunner;
        this.automationClient = automationClient;
        this.lowerer = lowerer;
        this.apiRequestRunner = apiRequestRunner;
        this.storageStateRunner = storageStateRunner;
        this.visualAssertionRunner = visualAssertionRunner;
        this.uploadRunner = uploadRunner;
    }

    public CmgTestResult Run(CmgTestCase test, string remoteDebuggingUrl, CmgRunOptions options, int attempt)
    {
        var output = new List<string>();
        var gifs = new List<string>();
        var pending = new List<string>();
        var pendingLineMap = new Dictionary<int, int>();
        var steps = new List<CmgStepResult>();
        var commandGif = BuildGifPath(test, options, attempt);
        var suppressGifBlocks = commandGif is not null;
        var timeouts = BuildTimeoutOptions(test, options);
        var baseUrl = CmgNavigationOptions.BaseUrl(test, options);

        foreach (var action in CmgVariables.FromRunOptions(options).Concat(test.Actions))
        {
            if (IsRecordingBlock(action.Kind) && !suppressGifBlocks)
            {
                var flush = RunLines(pending, pendingLineMap, remoteDebuggingUrl, gif: null, timeouts, baseUrl);
                if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps);
                }

                var gif = ResolveGifPath(test, action, options);
                if (gif is not null)
                {
                    gifs.Add(gif.FullName);
                }

                if (!RunActions(action.Children, remoteDebuggingUrl, gif, timeouts, baseUrl, output, steps, out error))
                {
                    return Fail(test, output, error, gifs, steps);
                }

                continue;
            }

            if (action.Kind.Equals("apiRequest", StringComparison.OrdinalIgnoreCase))
            {
                var flush = RunLines(pending, pendingLineMap, remoteDebuggingUrl, gif: null, timeouts, baseUrl);
                if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps);
                }

                var apiStep = apiRequestRunner.Run(ApplyRunTimeoutDefault(action, options));
                output.AddRange(apiStep.Output);
                steps.Add(apiStep);
                if (!apiStep.Success)
                {
                    return Fail(test, output, apiStep.Error, gifs, steps);
                }

                continue;
            }

            if (action.Kind.Equals("storageState", StringComparison.OrdinalIgnoreCase))
            {
                var flush = RunLines(pending, pendingLineMap, remoteDebuggingUrl, gif: null, timeouts, baseUrl);
                if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps);
                }

                var step = storageStateRunner.Run(action, remoteDebuggingUrl, automationClient);
                output.AddRange(step.Output);
                steps.Add(step);
                if (!step.Success)
                {
                    return Fail(test, output, step.Error, gifs, steps);
                }

                continue;
            }

            if (action.Kind.Equals("expectScreenshot", StringComparison.OrdinalIgnoreCase))
            {
                var flush = RunLines(pending, pendingLineMap, remoteDebuggingUrl, gif: null, timeouts, baseUrl);
                if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps);
                }

                var step = visualAssertionRunner.Run(action, remoteDebuggingUrl, automationClient);
                output.AddRange(step.Output);
                steps.Add(step);
                if (!step.Success)
                {
                    return Fail(test, output, step.Error, gifs, steps);
                }

                continue;
            }

            if (action.Kind.Equals("uploadFiles", StringComparison.OrdinalIgnoreCase))
            {
                var flush = RunLines(pending, pendingLineMap, remoteDebuggingUrl, gif: null, timeouts, baseUrl);
                if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps);
                }

                var step = uploadRunner.Run(action, remoteDebuggingUrl, automationClient);
                output.AddRange(step.Output);
                steps.Add(step);
                if (!step.Success)
                {
                    return Fail(test, output, step.Error, gifs, steps);
                }

                continue;
            }

            var lines = suppressGifBlocks && IsRecordingBlock(action.Kind)
                ? lowerer.LowerRecordingBlock(action)
                : lowerer.Lower(action);
            AddPending(pending, pendingLineMap, action, lines);
            if (!IsRecordingBlock(action.Kind))
            {
                steps.Add(new CmgStepResult(action.LineNumber, action.Kind, true, [], null, null));
            }
        }

        var final = RunLines(pending, pendingLineMap, remoteDebuggingUrl, commandGif, timeouts, baseUrl);
        if (!AppendResult(final.Result, final.LineMap, output, steps, test.Actions.LastOrDefault(), commandGif, out var finalError))
        {
            return Fail(test, output, finalError, gifs, steps);
        }

        if (commandGif is not null)
        {
            gifs.Add(commandGif.FullName);
        }

        return new CmgTestResult(test.Name, test.SourcePath, true, output, null, string.Join(';', gifs), steps) { Annotations = test.Annotations };
    }

    private static bool AppendResult(
        ScriptRunResult result,
        IReadOnlyDictionary<int, int> lineMap,
        List<string> output,
        List<CmgStepResult> steps,
        CmgNode? action,
        FileInfo? gif,
        out string? error)
    {
        output.AddRange(result.StdoutLines);
        AttachStepOutput(steps, result.StdoutLines, lineMap, action);
        error = result.Error;
        if (!result.Success && !result.Skipped && action is not null)
        {
            AttachStepFailure(steps, result.Error, result.StdoutLines, lineMap, action, gif);
        }

        return result.Success;
    }

    private static CmgTestResult Fail(
        CmgTestCase test,
        IReadOnlyList<string> output,
        string? error,
        IReadOnlyList<string> gifs,
        IReadOnlyList<CmgStepResult> steps)
    {
        if (output.Any(line => line.StartsWith("SKIP ", StringComparison.Ordinal)))
        {
            return new CmgTestResult(test.Name, test.SourcePath, true, output, error, string.Join(';', gifs), steps) { Status = "skipped", Annotations = test.Annotations };
        }

        return new CmgTestResult(test.Name, test.SourcePath, false, output, error, string.Join(';', gifs), steps) { Annotations = test.Annotations };
    }

    private static bool IsRecordingBlock(string name) =>
        name.Equals("gif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("recordVideo", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("screencast", StringComparison.OrdinalIgnoreCase);
}
