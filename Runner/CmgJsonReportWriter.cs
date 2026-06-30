using System.Text;
using System.Text.Json;

namespace CMG.Runner;

public static partial class CmgJsonReportWriter
{
    public static string Write(IReadOnlyList<CmgTestResult> tests)
    {
        var builder = new StringBuilder();
        using var writer = new Utf8JsonWriter(new StringBuilderBuffer(builder), new JsonWriterOptions { Indented = true });
        writer.WriteStartArray();
        foreach (var test in tests)
        {
            writer.WriteStartObject();
            writer.WriteString("name", test.Name);
            writer.WriteString("project", test.Project);
            writer.WriteString("sourcePath", test.SourcePath);
            writer.WriteString("status", Status(test));
            writer.WriteBoolean("success", test.Success);
            writer.WriteString("error", test.Error);
            writer.WriteString("gifPath", test.GifPath);
            WriteGifMetadata(writer, test);
            writer.WriteString("tags", test.Tags);
            writer.WriteStartArray("annotations");
            foreach (var annotation in test.Annotations)
            {
                writer.WriteStartObject();
                writer.WriteString("type", annotation.Type);
                writer.WriteString("description", annotation.Description);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            var publicSteps = PublicSteps(test.Steps);
            writer.WriteStartArray("output");
            foreach (var line in PublicOutput(test.Output, test.Steps))
            {
                writer.WriteStringValue(line);
            }

            writer.WriteEndArray();
            writer.WriteStartArray("steps");
            foreach (var step in publicSteps)
            {
                writer.WriteStartObject();
                writer.WriteNumber("sequence", step.Sequence);
                writer.WriteNumber("lineNumber", step.LineNumber);
                writer.WriteString("name", step.Name);
                writer.WriteString("action", string.IsNullOrWhiteSpace(step.Action) ? step.Name : step.Action);
                writer.WriteString("context", step.Context);
                writer.WriteBoolean("success", step.Success);
                writer.WriteString("error", step.Error);
                writer.WriteString("gifPath", step.GifPath);
                writer.WriteStartArray("output");
                foreach (var line in CleanOutputForReports(step.Output))
                {
                    writer.WriteStringValue(line);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.Flush();
        return builder.ToString();
    }

    private static string Status(CmgTestResult test) =>
        string.IsNullOrWhiteSpace(test.Status) ? test.Success ? "passed" : "failed" : test.Status;

    public static IEnumerable<string> CleanOutputForReports(IEnumerable<string> lines)
    {
        var generatedEvaluateLines = new HashSet<int>();
        var suppressNextEvaluatePayload = false;
        foreach (var line in lines)
        {
            if (IsGeneratedEvaluatePassLine(line, out var lineNumber))
            {
                generatedEvaluateLines.Add(lineNumber);
                suppressNextEvaluatePayload = true;
                continue;
            }

            if (suppressNextEvaluatePayload && IsEvaluatePayloadLine(line, out _))
            {
                suppressNextEvaluatePayload = false;
                continue;
            }

            suppressNextEvaluatePayload = false;
            if (IsEvaluatePayloadLine(line, out lineNumber) && generatedEvaluateLines.Contains(lineNumber))
            {
                continue;
            }

            yield return line;
        }
    }

    private static bool IsGeneratedEvaluatePassLine(string line, out int lineNumber)
    {
        lineNumber = 0;
        return (line.StartsWith("PASS ", StringComparison.Ordinal) &&
                TryReadLineNumber(line, "PASS ".Length, out lineNumber) &&
                line.Length > "PASS 000 ".Length &&
                line["PASS 000 ".Length..].StartsWith("evaluate ", StringComparison.Ordinal) &&
                IsGeneratedEvaluatePayload(line)) ||
            (TryReadStructuredSequence(line, out lineNumber) &&
                line.Contains(" action=evaluate", StringComparison.Ordinal) &&
                IsGeneratedEvaluatePayload(line));
    }

    private static bool IsGeneratedEvaluatePayload(string line) =>
        line.Contains("new Promise((resolve, reject)", StringComparison.Ordinal) ||
        line.Contains("__cmg_locator_", StringComparison.Ordinal) ||
        line.Contains("__cmgLocatorElements", StringComparison.Ordinal) ||
        line.Contains("(() =>", StringComparison.Ordinal) ||
        line.Contains("(async () =>", StringComparison.Ordinal);

    private static bool IsEvaluatePayloadLine(string line, out int lineNumber)
    {
        lineNumber = 0;
        return TryReadStructuredSequence(line, out lineNumber) ||
            line.StartsWith("EVALUATE ", StringComparison.Ordinal) && TryReadLineNumber(line, "EVALUATE ".Length, out lineNumber) ||
            line.StartsWith("FRAME_EVALUATE ", StringComparison.Ordinal) && TryReadLineNumber(line, "FRAME_EVALUATE ".Length, out lineNumber) ||
            line.StartsWith("WORKER_EVALUATE ", StringComparison.Ordinal) && TryReadLineNumber(line, "WORKER_EVALUATE ".Length, out lineNumber);
    }

    private static bool TryReadStructuredSequence(string line, out int sequence)
    {
        sequence = 0;
        return TryReadLeadingSequence(line, out sequence) &&
            line.Contains(" line=", StringComparison.Ordinal);
    }

    private static bool TryReadLeadingSequence(string line, out int sequence)
    {
        sequence = 0;
        var first = line.IndexOf(' ');
        return first >= 0 && TryReadLineNumber(line, first + 1, out sequence);
    }

    private static bool TryReadLineNumber(string line, int start, out int lineNumber)
    {
        lineNumber = 0;
        return line.Length >= start + 3 &&
            int.TryParse(line.AsSpan(start, 3), out lineNumber);
    }

}
