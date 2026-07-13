using System.Text.RegularExpressions;

namespace CMG.Runner;

public static partial class CmgJsonReportWriter
{
    public static IReadOnlyList<CmgStepResult> PublicSteps(IEnumerable<CmgStepResult> steps)
    {
        return PublicStepPairs(steps)
            .Select(pair => pair.Step with
            {
                Sequence = pair.PublicSequence,
                Output = CleanOutputForReports(pair.Step.Output)
                    .Select(line => RewriteOutputSequence(line, pair.PublicSequence))
                    .ToArray()
            })
            .ToArray();
    }

    public static IEnumerable<string> PublicOutput(IEnumerable<string> lines, IEnumerable<CmgStepResult> steps)
    {
        var publicStepPairs = PublicStepPairs(steps);
        var sequenceByRawSequence = publicStepPairs
            .Where(pair => pair.Step.Sequence > 0)
            .ToDictionary(pair => pair.Step.Sequence, pair => pair.PublicSequence);
        var sequenceByLine = publicStepPairs
            .GroupBy(pair => pair.Step.LineNumber)
            .ToDictionary(group => group.Key, group => group.First().PublicSequence);
        int? currentSequence = null;

        foreach (var line in CleanOutputForReports(lines))
        {
            if (TryReadLeadingSequence(line, out var rawSequence) &&
                sequenceByRawSequence.TryGetValue(rawSequence, out var sequenceFromRaw))
            {
                currentSequence = sequenceFromRaw;
                yield return RewriteOutputSequence(line, sequenceFromRaw);
                continue;
            }

            if (TryReadStructuredSourceLine(line, out var sourceLine) &&
                sequenceByLine.TryGetValue(sourceLine, out var sequenceFromLine))
            {
                currentSequence = sequenceFromLine;
                yield return RewriteOutputSequence(line, sequenceFromLine);
                continue;
            }

            yield return currentSequence is int sequence
                ? RewriteOutputSequence(line, sequence)
                : line;
        }
    }

    private static IReadOnlyList<PublicStepPair> PublicStepPairs(IEnumerable<CmgStepResult> steps)
    {
        var pairs = new List<PublicStepPair>();
        var nextSequence = 1;
        foreach (var step in steps)
        {
            if (IsPlannedPlaceholder(step) || IsInternalStep(step))
            {
                continue;
            }

            pairs.Add(new PublicStepPair(step, nextSequence++));
        }

        return pairs;
    }

    private static bool IsInternalStep(CmgStepResult step) =>
        (string.IsNullOrWhiteSpace(step.Action) ? step.Name : step.Action)
            .Equals("evaluate", StringComparison.OrdinalIgnoreCase) &&
        step.Output.Any(line => IsGeneratedEvaluatePassLine(line, out _));

    private static bool IsPlannedPlaceholder(CmgStepResult step) =>
        step.Sequence is 0 &&
        step.Output.Count is 0 &&
        step.Error is null &&
        step.GifPath is null;

    private static string RewriteOutputSequence(string line, int sequence) =>
        LeadingSequenceRegex().Replace(line, match => $"{match.Groups["label"].Value} {sequence:000}", 1);

    private static bool TryReadStructuredSourceLine(string line, out int lineNumber)
    {
        lineNumber = 0;
        var lineToken = line.IndexOf(" line=", StringComparison.Ordinal);
        if (lineToken < 0)
        {
            return false;
        }

        var start = lineToken + " line=".Length;
        var end = line.IndexOf(' ', start);
        var length = end < 0 ? line.Length - start : end - start;
        return length > 0 && int.TryParse(line.AsSpan(start, length), out lineNumber);
    }

    [GeneratedRegex(@"^(?<label>[A-Z_]+) \d{3}")]
    private static partial Regex LeadingSequenceRegex();

    private sealed record PublicStepPair(CmgStepResult Step, int PublicSequence);
}
