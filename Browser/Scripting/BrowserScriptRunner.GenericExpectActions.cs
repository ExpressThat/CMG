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
            return [$"{ExpectLabel(action)} {action.LineNumber:000} true"];
        }

        var condition = string.Join(' ', action.Arguments);
        var message = action.Options.GetValueOrDefault("message") ??
            action.Options.GetValueOrDefault("reason") ??
            $"Expected condition to pass: {condition}";
        if (IsSoftExpect(action))
        {
            var failure = $"Line {action.LineNumber}: {message}";
            context.SoftFailures.Add(failure);
            return [$"{ExpectLabel(action)} {action.LineNumber:000} false {message}"];
        }

        throw new ScriptExecutionException(message);
    }

    private static bool IsSoftExpect(BrowserScriptAction action) =>
        action.Name.Equals("softExpect", StringComparison.OrdinalIgnoreCase) ||
        action.Name.Equals("softAssert", StringComparison.OrdinalIgnoreCase) ||
        action.Name.Equals("expect.soft", StringComparison.OrdinalIgnoreCase) ||
        action.Name.Equals("assert.soft", StringComparison.OrdinalIgnoreCase);

    private static string ExpectLabel(BrowserScriptAction action) =>
        IsSoftExpect(action) ? "SOFT_EXPECT" : "EXPECT";
}
