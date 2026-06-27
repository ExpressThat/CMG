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

    private void AddPending(
        List<string> pending,
        Dictionary<int, int> lineMap,
        CmgNode action,
        IReadOnlyList<string> lines)
    {
        if (action.Kind.Equals("step", StringComparison.OrdinalIgnoreCase) && action.Children.Count > 0 && lines.Count > 0)
        {
            pending.Add(lines[0]);
            lineMap[pending.Count] = action.LineNumber;
            foreach (var child in action.Children)
            {
                AddPending(pending, lineMap, child, lowerer.Lower(child));
            }

            return;
        }

        foreach (var line in lines)
        {
            pending.Add(line);
            lineMap[pending.Count] = action.LineNumber;
        }
    }

    private sealed record CmgScriptBatchRun(ScriptRunResult Result, IReadOnlyDictionary<int, int> LineMap);
}
