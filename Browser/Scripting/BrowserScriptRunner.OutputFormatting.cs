namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static string FormatStepLine(string label, int sequence, BrowserScriptAction action, ScriptExecutionContext context, string payload)
    {
        var payloadField = string.IsNullOrWhiteSpace(payload) ? string.Empty : $" {payload}";
        var contextField = string.IsNullOrWhiteSpace(context.CurrentContext) ? string.Empty : $" context={QuoteField(context.CurrentContext)}";
        return $"{label} {sequence:000} line={action.LineNumber}{contextField} action={action.Name}{payloadField}".TrimEnd();
    }

    private IEnumerable<string> FormatPayloadLines(IReadOnlyList<string> lines, int sequence, BrowserScriptAction action, ScriptExecutionContext context)
    {
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(context.CurrentContext) &&
                TryReadPayloadLine(line, out var label, out var payload))
            {
                yield return FormatStepLine(label, sequence, action, context, payload);
                continue;
            }

            yield return line;
        }
    }

    private static bool TryReadPayloadLine(string line, out string label, out string payload)
    {
        label = string.Empty;
        payload = string.Empty;
        var first = line.IndexOf(' ');
        var second = first < 0 ? -1 : line.IndexOf(' ', first + 1);
        if (first <= 0 || second <= first || second + 1 >= line.Length)
        {
            return false;
        }

        if (!int.TryParse(line.AsSpan(first + 1, second - first - 1), out _))
        {
            return false;
        }

        if (line.IndexOf(" line=", second, StringComparison.Ordinal) >= 0)
        {
            return false;
        }

        label = line[..first];
        payload = line[(second + 1)..];
        return true;
    }

    private static string QuoteField(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
