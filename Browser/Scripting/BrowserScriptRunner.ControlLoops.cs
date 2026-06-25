namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteFor(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 1, 3);
        var variable = action.Arguments.Count >= 3 ? action.Arguments[0] : "index";
        var start = action.Arguments.Count >= 3 ? ParseLoopInt(action.Arguments[1], "start") : 0;
        var end = ParseLoopInt(action.Arguments.Count >= 3 ? action.Arguments[2] : action.Arguments[0], "end");
        var step = action.Options.TryGetValue("step", out var stepValue) ? ParseLoopInt(stepValue, "step") : 1;
        if (step is 0)
        {
            throw new ScriptExecutionException("for option step= cannot be 0.");
        }

        var values = Range(start, end, step).Select(value => (variable, value.ToString()));
        foreach (var pair in values)
        {
            WithVariables(context, [pair], () =>
                ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
        }
    }

    private void ExecuteForEach(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 2, int.MaxValue);
        var variable = action.Arguments[0];
        foreach (var value in action.Arguments.Skip(1))
        {
            WithVariables(context, [(variable, value)], () =>
                ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
        }
    }

    private static IEnumerable<int> Range(int start, int end, int step)
    {
        if (step > 0)
        {
            for (var value = start; value < end; value += step) yield return value;
            yield break;
        }

        for (var value = start; value > end; value += step) yield return value;
    }

    private static int ParseLoopInt(string value, string name)
    {
        if (!int.TryParse(value, out var parsed))
        {
            throw new ScriptExecutionException($"for {name} must be a whole number.");
        }

        return parsed;
    }
}
