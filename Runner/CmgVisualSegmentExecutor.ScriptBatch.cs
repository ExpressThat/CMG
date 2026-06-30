using CMG.Browser.Scripting;
using System.Text.RegularExpressions;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static readonly Regex StructuredLineRegex = new(@"^(?<prefix>[A-Z_]+ \d{3}) line=(?<line>\d+)(?<suffix>.*)$", RegexOptions.Compiled);

    private CmgScriptBatchRun RunLines(
        List<string> lines,
        Dictionary<int, int> lineMap,
        string remoteDebuggingUrl,
        FileInfo? gif,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        CMG.Browser.Scripting.Recording.GifQuality gifQuality,
        CMG.Browser.Scripting.Recording.ScriptPointerMotionOptions? pointerMotion,
        CMG.Browser.ClickPulseStyle clickPulse,
        int holdAfterActionMilliseconds,
        int holdOnFailureMilliseconds,
        string? gifTimelinePath,
        int frameDelayMilliseconds = CMG.Browser.Scripting.Recording.ScriptRecordingOptions.DefaultFrameDelayMilliseconds)
    {
        if (lines.Count is 0)
        {
            return new CmgScriptBatchRun(ScriptRunResult.Ok([]), new Dictionary<int, int>());
        }

        var script = string.Join(Environment.NewLine, lines);
        var map = new Dictionary<int, int>(lineMap);
        lines.Clear();
        lineMap.Clear();
        return new CmgScriptBatchRun(MapScriptResult(scriptRunner.RunText(script, remoteDebuggingUrl, automationClient, gif, trace: null, timeouts, baseUrl, gifQuality: gifQuality, pointerMotion: pointerMotion, clickPulse: clickPulse, holdAfterActionMilliseconds: holdAfterActionMilliseconds, holdOnFailureMilliseconds: holdOnFailureMilliseconds, gifTimelinePath: gifTimelinePath, frameDelayMilliseconds: frameDelayMilliseconds), map), map);
    }

    private static ScriptRunResult MapScriptResult(ScriptRunResult result, IReadOnlyDictionary<int, int> lineMap)
    {
        var mappedSteps = result.StepRecords
            .Select(step => step with
            {
                LineNumber = lineMap.GetValueOrDefault(step.LineNumber, step.LineNumber),
                Output = step.Output.Select(line => RewriteStructuredLine(line, lineMap)).ToArray()
            })
            .ToArray();
        return result with
        {
            StdoutLines = result.StdoutLines.Select(line => RewriteStructuredLine(line, lineMap)).ToArray(),
            Steps = mappedSteps
        };
    }

    private static string RewriteStructuredLine(string line, IReadOnlyDictionary<int, int> lineMap)
    {
        var match = StructuredLineRegex.Match(line);
        if (!match.Success || !int.TryParse(match.Groups["line"].Value, out var lineNumber))
        {
            return line;
        }

        var sourceLine = lineMap.GetValueOrDefault(lineNumber, lineNumber);
        return $"{match.Groups["prefix"].Value} line={sourceLine}{match.Groups["suffix"].Value}";
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
