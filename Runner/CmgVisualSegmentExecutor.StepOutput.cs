namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static void AttachStepOutput(
        List<CmgStepResult> steps,
        IReadOnlyList<string> lines,
        IReadOnlyDictionary<int, int> lineMap,
        CmgNode? action)
    {
        foreach (var line in lines)
        {
            if (!TryReadOutputLineNumber(line, out var lineNumber))
            {
                continue;
            }

            var sourceLine = lineMap.GetValueOrDefault(lineNumber, lineNumber);
            var index = steps.FindLastIndex(step => step.LineNumber == sourceLine);
            if (index < 0)
            {
                if (action is not null)
                {
                    steps.Add(new CmgStepResult(sourceLine, FindNodeKindByLine(action, sourceLine) ?? action.Kind, true, [line], null, null));
                }

                continue;
            }

            var step = steps[index];
            steps[index] = step with { Output = step.Output.Concat([line]).ToArray() };
        }
    }

    private static bool TryReadOutputLineNumber(string line, out int lineNumber)
    {
        lineNumber = 0;
        foreach (var token in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Length == 3 && int.TryParse(token, out lineNumber))
            {
                return true;
            }
        }

        return false;
    }

    private static void AttachStepFailure(
        List<CmgStepResult> steps,
        string? error,
        IReadOnlyList<string> output,
        IReadOnlyDictionary<int, int> lineMap,
        CmgNode fallback,
        FileInfo? gif)
    {
        var sourceLine = TryReadErrorLineNumber(error, out var lineNumber)
            ? lineMap.GetValueOrDefault(lineNumber, lineNumber)
            : fallback.LineNumber;
        var index = steps.FindLastIndex(step => step.LineNumber == sourceLine);
        if (index < 0)
        {
            steps.Add(new CmgStepResult(sourceLine, FindNodeKindByLine(fallback, sourceLine) ?? fallback.Kind, false, output, error, gif?.FullName));
            return;
        }

        var step = steps[index];
        steps[index] = step with { Success = false, Error = error, Output = step.Output.Concat(output).ToArray(), GifPath = gif?.FullName };
    }

    private static bool TryReadErrorLineNumber(string? error, out int lineNumber)
    {
        lineNumber = 0;
        if (string.IsNullOrWhiteSpace(error) || !error.StartsWith("Line ", StringComparison.Ordinal))
        {
            return false;
        }

        var colon = error.IndexOf(':', StringComparison.Ordinal);
        return colon > "Line ".Length &&
            int.TryParse(error["Line ".Length..colon], out lineNumber);
    }

    private static string? FindNodeKindByLine(CmgNode node, int lineNumber)
    {
        if (node.LineNumber == lineNumber)
        {
            return node.Kind;
        }

        foreach (var child in node.Children)
        {
            var match = FindNodeKindByLine(child, lineNumber);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}
