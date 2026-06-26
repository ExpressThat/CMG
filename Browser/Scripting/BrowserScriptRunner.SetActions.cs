using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private IReadOnlyList<string> ExecuteSet(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        ScriptGifRecorder? recorder)
    {
        if (action.Children.Count is 0)
        {
            RequireArgumentCount(action, 2, 2);
            context.SetVariable(action.Arguments[0], action.Arguments[1]);
            return [];
        }

        RequireArgumentCount(action, 1, 1);
        var output = new List<string>();
        ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output);

        var payload = ExtractBlockPayload($"set '{action.Arguments[0]}'", output);
        context.SetVariable(action.Arguments[0], payload);
        output.Add($"SET {action.LineNumber:000} {action.Arguments[0]} {payload}");
        return output;
    }

    private static string ExtractBlockPayload(string blockName, IReadOnlyList<string> output)
    {
        var line = output.LastOrDefault(IsSetPayloadLine);
        if (line is null)
        {
            throw new ScriptExecutionException($"{blockName} block did not produce output.");
        }

        var first = line.IndexOf(' ');
        var second = first < 0 ? -1 : line.IndexOf(' ', first + 1);
        if (second < 0 || second + 1 >= line.Length)
        {
            throw new ScriptExecutionException($"{blockName} could not read a payload from '{line}'.");
        }

        return line[(second + 1)..];
    }

    private static bool IsSetPayloadLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var label = line.Split(' ', 2)[0];
        return !label.Equals("PASS", StringComparison.Ordinal) &&
            !label.Equals("MACRO", StringComparison.Ordinal) &&
            !label.Equals("RETRY", StringComparison.Ordinal) &&
            !label.Equals("GIF_BLOCK_SUPPRESSED", StringComparison.Ordinal);
    }
}
