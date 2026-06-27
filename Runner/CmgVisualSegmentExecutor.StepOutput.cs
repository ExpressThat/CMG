namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static void AttachStepOutput(
        List<CmgStepResult> steps,
        IReadOnlyList<string> lines,
        IReadOnlyDictionary<int, int> lineMap)
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
}
