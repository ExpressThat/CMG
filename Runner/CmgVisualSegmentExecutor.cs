using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Runner;

public sealed class CmgVisualSegmentExecutor
{
    private readonly BrowserScriptRunner scriptRunner;
    private readonly IBrowserAutomationClient automationClient;
    private readonly CmgActionLowerer lowerer;

    public CmgVisualSegmentExecutor(
        BrowserScriptRunner scriptRunner,
        IBrowserAutomationClient automationClient,
        CmgActionLowerer lowerer)
    {
        this.scriptRunner = scriptRunner;
        this.automationClient = automationClient;
        this.lowerer = lowerer;
    }

    public CmgTestResult Run(CmgTestCase test, string remoteDebuggingUrl, CmgRunOptions options)
    {
        var output = new List<string>();
        var gifs = new List<string>();
        var pending = new List<string>();
        var commandGif = BuildGifPath(test, options);
        var suppressGifBlocks = commandGif is not null;

        foreach (var action in test.Actions)
        {
            if (action.Kind.Equals("gif", StringComparison.OrdinalIgnoreCase) && !suppressGifBlocks)
            {
                var flush = RunLines(pending, remoteDebuggingUrl, gif: null);
                if (!AppendResult(flush, output, out var error))
                {
                    return Fail(test, output, error, gifs);
                }

                var gif = ResolveGifPath(test, action, options);
                var block = action.Children.SelectMany(lowerer.Lower).ToList();
                var blockResult = RunLines(block, remoteDebuggingUrl, gif);
                if (gif is not null)
                {
                    gifs.Add(gif.FullName);
                }

                if (!AppendResult(blockResult, output, out error))
                {
                    return Fail(test, output, error, gifs);
                }

                continue;
            }

            pending.AddRange(lowerer.Lower(action));
        }

        var final = RunLines(pending, remoteDebuggingUrl, commandGif);
        if (!AppendResult(final, output, out var finalError))
        {
            return Fail(test, output, finalError, gifs);
        }

        if (commandGif is not null)
        {
            gifs.Add(commandGif.FullName);
        }

        return new CmgTestResult(test.Name, test.SourcePath, true, output, null, string.Join(';', gifs));
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

    private static bool AppendResult(ScriptRunResult result, List<string> output, out string? error)
    {
        output.AddRange(result.StdoutLines);
        error = result.Error;
        return result.Success;
    }

    private static CmgTestResult Fail(CmgTestCase test, IReadOnlyList<string> output, string? error, IReadOnlyList<string> gifs) =>
        new(test.Name, test.SourcePath, false, output, error, string.Join(';', gifs));

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

    private static FileInfo? BuildGifPath(CmgTestCase test, CmgRunOptions options) =>
        options.GifDirectory is null ? null : CmgRunService.BuildGifPath(test, options);
}
