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
            throw new ScriptExecutionException("retry requires a block body.");
        }

        var max = RetryMax(action);
        var delay = RetryDelay(action);
        ScriptActionFailedException? last = null;

        for (var attempt = 1; attempt <= max; attempt++)
        {
            try
            {
                ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output);
                if (attempt > 1)
                {
                    output.Add($"RETRY {action.LineNumber:000} success attempt={attempt}");
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

                output.Add($"RETRY {action.LineNumber:000} attempt={attempt} failed={exception.Message}");
                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }
        }

        throw new ScriptExecutionException($"retry exhausted {max} attempt(s). Last error: {last?.Message ?? "unknown failure"}");
    }

    private static int RetryMax(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 1);
        var hasOption = action.Options.TryGetValue("max", out var optionValue);
        if (hasOption && action.Arguments.Count > 0)
        {
            throw new ScriptExecutionException("retry accepts either a positional count or max=, not both.");
        }

        var max = hasOption
            ? ParseRetryInt(optionValue!, "max")
            : action.Arguments.Count is 1 ? ParseRetryInt(action.Arguments[0], "max") : 3;
        return max > 0
            ? max
            : throw new ScriptExecutionException("retry max must be greater than 0.");
    }

    private static int RetryDelay(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("delay", out var value))
        {
            return 0;
        }

        var delay = ParseRetryInt(value, "delay");
        return delay >= 0
            ? delay
            : throw new ScriptExecutionException("retry delay must be 0 or greater.");
    }

    private static int ParseRetryInt(string value, string name)
    {
        if (!int.TryParse(value, out var parsed))
        {
            throw new ScriptExecutionException($"retry {name} must be a whole number.");
        }

        return parsed;
    }
}
