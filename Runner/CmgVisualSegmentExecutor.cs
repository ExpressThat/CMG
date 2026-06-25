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
        var steps = new List<CmgStepResult>();
        var commandGif = BuildGifPath(test, options, attempt);
        var suppressGifBlocks = commandGif is not null;

        foreach (var action in test.Actions)
        {
            if (IsRecordingBlock(action.Kind) && !suppressGifBlocks)
            {
                var flush = RunLines(pending, remoteDebuggingUrl, gif: null);
                if (!AppendResult(flush, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps);
                }

                var gif = ResolveGifPath(test, action, options);
                if (gif is not null)
                {
                    gifs.Add(gif.FullName);
                }

                if (!RunActions(action.Children, remoteDebuggingUrl, gif, output, steps, out error))
                {
                    return Fail(test, output, error, gifs, steps);
                }

                continue;
            }

            if (action.Kind.Equals("apiRequest", StringComparison.OrdinalIgnoreCase))
            {
                var flush = RunLines(pending, remoteDebuggingUrl, gif: null);
                if (!AppendResult(flush, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps);
                }

                var apiStep = apiRequestRunner.Run(action);
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
                var flush = RunLines(pending, remoteDebuggingUrl, gif: null);
                if (!AppendResult(flush, output, steps, action, gif: null, out var error))
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
                var flush = RunLines(pending, remoteDebuggingUrl, gif: null);
                if (!AppendResult(flush, output, steps, action, gif: null, out var error))
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
                var flush = RunLines(pending, remoteDebuggingUrl, gif: null);
                if (!AppendResult(flush, output, steps, action, gif: null, out var error))
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

            var lines = lowerer.Lower(action);
            pending.AddRange(lines);
            if (!IsRecordingBlock(action.Kind))
            {
                steps.Add(new CmgStepResult(action.LineNumber, action.Kind, true, [], null, null));
            }
        }

        var final = RunLines(pending, remoteDebuggingUrl, commandGif);
        if (!AppendResult(final, output, steps, test.Actions.LastOrDefault(), commandGif, out var finalError))
        {
            return Fail(test, output, finalError, gifs, steps);
        }

        if (commandGif is not null)
        {
            gifs.Add(commandGif.FullName);
        }

        return new CmgTestResult(test.Name, test.SourcePath, true, output, null, string.Join(';', gifs), steps);
    }

    private ScriptRunResult RunLines(List<string> lines, string remoteDebuggingUrl, FileInfo? gif)
    {
        if (lines.Count is 0)
        {
            return ScriptRunResult.Ok([]);
        }

        var script = string.Join(Environment.NewLine, lines);
        lines.Clear();
        return scriptRunner.RunText(script, remoteDebuggingUrl, automationClient, gif);
    }

    private static bool AppendResult(
        ScriptRunResult result,
        List<string> output,
        List<CmgStepResult> steps,
        CmgNode? action,
        FileInfo? gif,
        out string? error)
    {
        output.AddRange(result.StdoutLines);
        error = result.Error;
        if (!result.Success && action is not null)
        {
            steps.Add(new CmgStepResult(action.LineNumber, action.Kind, false, result.StdoutLines, result.Error, gif?.FullName));
        }

        return result.Success;
    }

    private static CmgTestResult Fail(
        CmgTestCase test,
        IReadOnlyList<string> output,
        string? error,
        IReadOnlyList<string> gifs,
        IReadOnlyList<CmgStepResult> steps) =>
        new(test.Name, test.SourcePath, false, output, error, string.Join(';', gifs), steps);

    private static FileInfo? ResolveGifPath(CmgTestCase test, CmgNode action, CmgRunOptions options)
    {
        if (action.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output))
        {
            return new FileInfo(output);
        }

        var name = action.Arguments.Count > 0 ? action.Arguments[0] : test.Name;
        var safeName = string.Concat(name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        var directory = options.GifDirectory?.FullName ?? Directory.GetCurrentDirectory();
        return new FileInfo(Path.Combine(directory, $"{safeName}.gif"));
    }

    private static FileInfo? BuildGifPath(CmgTestCase test, CmgRunOptions options, int attempt)
    {
        if (options.GifDirectory is null)
        {
            return null;
        }

        var path = CmgRunService.BuildGifPath(test, options);
        if (attempt <= 1 || path is null)
        {
            return path;
        }

        var name = Path.GetFileNameWithoutExtension(path.Name);
        return new FileInfo(Path.Combine(path.DirectoryName ?? string.Empty, $"{name}-attempt-{attempt}.gif"));
    }

    private static bool IsRecordingBlock(string name) =>
        name.Equals("gif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("recordVideo", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("screencast", StringComparison.OrdinalIgnoreCase);
}
