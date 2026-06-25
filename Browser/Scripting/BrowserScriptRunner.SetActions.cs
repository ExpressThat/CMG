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
            context.Variables[action.Arguments[0]] = action.Arguments[1];
            return [];
        }

        RequireArgumentCount(action, 1, 1);
        var output = new List<string>();
        foreach (var child in action.Children)
        {
            recorder?.BeforeAction(child);
            var lines = ExecuteAction(remoteDebuggingUrl, automationClient, child, context, recorder);
            recorder?.AfterAction(child);
            output.AddRange(lines);
        }

        var payload = ExtractSetPayload(action, output);
        context.Variables[action.Arguments[0]] = payload;
        output.Add($"SET {action.LineNumber:000} {action.Arguments[0]} {payload}");
        return output;
    }

    private static string ExtractSetPayload(BrowserScriptAction action, IReadOnlyList<string> output)
    {
        var line = output.LastOrDefault(line => !string.IsNullOrWhiteSpace(line));
        if (line is null)
        {
            throw new ScriptExecutionException($"set '{action.Arguments[0]}' block did not produce output.");
        }

        var first = line.IndexOf(' ');
        var second = first < 0 ? -1 : line.IndexOf(' ', first + 1);
        if (second < 0 || second + 1 >= line.Length)
        {
            throw new ScriptExecutionException($"set '{action.Arguments[0]}' could not read a payload from '{line}'.");
        }

        return line[(second + 1)..];
    }
}
