namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private IReadOnlyList<string> ExecuteGenericExpect(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder)
    {
        RequireArgumentCount(action, 1, int.MaxValue);
        if (EvaluateCondition(remoteDebuggingUrl, automationClient, action, context, recorder))
        {
            return [$"EXPECT {action.LineNumber:000} true"];
        }

        var condition = string.Join(' ', action.Arguments);
        var message = action.Options.GetValueOrDefault("message") ??
            action.Options.GetValueOrDefault("reason") ??
            $"Expected condition to pass: {condition}";
        throw new ScriptExecutionException(message);
    }
}
