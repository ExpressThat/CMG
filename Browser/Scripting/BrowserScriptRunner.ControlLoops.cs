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
            var control = ExecuteLoopIteration(remoteDebuggingUrl, automationClient, action, context, recorder, output, [pair]);
            if (control == "break") break;
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
            var control = ExecuteLoopIteration(remoteDebuggingUrl, automationClient, action, context, recorder, output, [(variable, value)]);
            if (control == "break") break;
        }
    }

    private void ExecuteRepeat(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 1, 2);
        var variable = action.Arguments.Count is 2 ? action.Arguments[0] : "index";
        var count = ParseLoopInt(action.Arguments.Count is 2 ? action.Arguments[1] : action.Arguments[0], "count");
        if (count < 0) throw new ScriptExecutionException("repeat count must be 0 or greater.");
        for (var index = 0; index < count; index++)
        {
            var control = ExecuteLoopIteration(remoteDebuggingUrl, automationClient, action, context, recorder, output, [(variable, index.ToString())]);
            if (control == "break") break;
        }
    }

    private void ExecuteWhile(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 1, int.MaxValue);
        var max = GetIntOption(action, "max", 100);
        var iterations = 0;
        while (EvaluateCondition(remoteDebuggingUrl, automationClient, action, context))
        {
            if (iterations++ >= max)
            {
                throw new ScriptExecutionException($"while exceeded max={max} iteration(s).");
            }

            var control = ExecuteLoopIteration(remoteDebuggingUrl, automationClient, action, context, recorder, output, [("index", (iterations - 1).ToString())]);
            if (control == "break") break;
        }
    }

    private string ExecuteLoopIteration(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output,
        IEnumerable<(string Key, string Value)> variables)
    {
        try
        {
            WithVariables(context, variables, () =>
                ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
            return string.Empty;
        }
        catch (LoopControlException exception) when (exception.Kind is "break" or "continue")
        {
            return exception.Kind;
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
