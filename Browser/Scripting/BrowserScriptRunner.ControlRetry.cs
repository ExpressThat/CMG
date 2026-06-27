namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteRetry(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        if (action.Children.Count is 0)
        {
            throw new ScriptExecutionException($"{RetryName(action)} requires a block body.");
        }

        var max = RetryMax(action);
        var delay = RetryDelay(action);
        var outputName = RetryOutputName(action);
        ScriptActionFailedException? last = null;

        for (var attempt = 1; attempt <= max; attempt++)
        {
            try
            {
                context.PushExecutionContext($"{RetryName(action)}[{attempt}/{max}]", () =>
                    ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
                if (attempt > 1)
                {
                    output.Add($"{outputName} {action.LineNumber:000} success attempt={attempt}");
                }

                return;
            }
            catch (ScriptActionFailedException exception)
            {
                last = exception;
                if (attempt >= max)
                {
                    break;
                }

                output.Add($"{outputName} {action.LineNumber:000} attempt={attempt} failed={exception.Message}");
                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }
        }

        throw new ScriptExecutionException($"{RetryName(action)} exhausted {max} attempt(s). Last error: {last?.Message ?? "unknown failure"}");
    }

    private static int RetryMax(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 1);
        var hasOption = action.Options.TryGetValue("max", out var optionValue);
        if (hasOption && action.Arguments.Count > 0)
        {
            throw new ScriptExecutionException($"{RetryName(action)} accepts either a positional count or max=, not both.");
        }

        var max = hasOption
            ? ParseRetryInt(action, optionValue!, "max")
            : action.Arguments.Count is 1 ? ParseRetryInt(action, action.Arguments[0], "max") : 3;
        return max > 0
            ? max
            : throw new ScriptExecutionException($"{RetryName(action)} max must be greater than 0.");
    }

    private static int RetryDelay(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("delay", out var value))
        {
            return 0;
        }

        var delay = ParseRetryInt(action, value, "delay");
        return delay >= 0
            ? delay
            : throw new ScriptExecutionException($"{RetryName(action)} delay must be 0 or greater.");
    }

    private static int ParseRetryInt(BrowserScriptAction action, string value, string name)
    {
        if (!int.TryParse(value, out var parsed))
        {
            throw new ScriptExecutionException($"{RetryName(action)} {name} must be a whole number.");
        }

        return parsed;
    }

    private static string RetryName(BrowserScriptAction action) =>
        action.Name.Equals("toPass", StringComparison.OrdinalIgnoreCase) ? "toPass" : "retry";

    private static string RetryOutputName(BrowserScriptAction action) =>
        action.Name.Equals("toPass", StringComparison.OrdinalIgnoreCase) ? "TO_PASS" : "RETRY";
}
