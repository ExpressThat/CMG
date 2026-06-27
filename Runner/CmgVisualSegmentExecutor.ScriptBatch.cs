using CMG.Browser.Scripting;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private CmgScriptBatchRun RunLines(
        List<string> lines,
        Dictionary<int, int> lineMap,
        string remoteDebuggingUrl,
        FileInfo? gif,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl)
    {
        if (lines.Count is 0)
        {
            return new CmgScriptBatchRun(ScriptRunResult.Ok([]), new Dictionary<int, int>());
        }

        var script = string.Join(Environment.NewLine, lines);
        var map = new Dictionary<int, int>(lineMap);
        lines.Clear();
        lineMap.Clear();
        return new CmgScriptBatchRun(scriptRunner.RunText(script, remoteDebuggingUrl, automationClient, gif, timeouts, baseUrl), map);
    }

    private static void AddPending(
        List<string> pending,
        Dictionary<int, int> lineMap,
        CmgNode action,
        IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            pending.Add(line);
            lineMap[pending.Count] = action.LineNumber;
        }
    }

    private sealed record CmgScriptBatchRun(ScriptRunResult Result, IReadOnlyDictionary<int, int> LineMap);
}
