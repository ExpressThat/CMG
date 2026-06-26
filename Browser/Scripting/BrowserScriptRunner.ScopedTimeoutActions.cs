namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteScopedTimeout(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        var previous = new ScriptTimeoutOptions(context.DefaultTimeout, context.NavigationTimeout, context.AssertionTimeout);
        ApplyScopedTimeout(action, context);
        try
        {
            ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output);
        }
        finally
        {
            context.DefaultTimeout = previous.DefaultTimeout;
            context.NavigationTimeout = previous.NavigationTimeout;
            context.AssertionTimeout = previous.AssertionTimeout;
        }
    }

    private static void ApplyScopedTimeout(BrowserScriptAction action, ScriptExecutionContext context)
    {
        if (action.Name.Equals("withTimeout", StringComparison.OrdinalIgnoreCase))
        {
            RequireArgumentCount(action, 0, 1);
            if (action.Arguments.Count is 0 && !HasScopedTimeoutOption(action))
            {
                throw new ScriptExecutionException("withTimeout requires a positional timeout or at least one timeout option.");
            }

            if (action.Arguments.Count is 1) context.DefaultTimeout = ParsePositiveInt(action.Arguments[0], action.Name);
            ApplyOptionalTimeout(action, "default", value => context.DefaultTimeout = value);
            ApplyOptionalTimeout(action, "timeout", value => context.DefaultTimeout = value);
            ApplyOptionalTimeout(action, "navigation", value => context.NavigationTimeout = value);
            ApplyOptionalTimeout(action, "assertion", value => context.AssertionTimeout = value);
            ApplyOptionalTimeout(action, "expect", value => context.AssertionTimeout = value);
            return;
        }

        RequireArgumentCount(action, 1, 1);
        var timeout = ParsePositiveInt(action.Arguments[0], action.Name);
        if (action.Name.Equals("withNavigationTimeout", StringComparison.OrdinalIgnoreCase))
        {
            context.NavigationTimeout = timeout;
        }
        else if (action.Name.Equals("withAssertionTimeout", StringComparison.OrdinalIgnoreCase) ||
                 action.Name.Equals("withExpectTimeout", StringComparison.OrdinalIgnoreCase))
        {
            context.AssertionTimeout = timeout;
        }
        else
        {
            context.DefaultTimeout = timeout;
        }
    }

    private static bool HasScopedTimeoutOption(BrowserScriptAction action) =>
        action.Options.ContainsKey("default") ||
        action.Options.ContainsKey("timeout") ||
        action.Options.ContainsKey("navigation") ||
        action.Options.ContainsKey("assertion") ||
        action.Options.ContainsKey("expect");

    private static void ApplyOptionalTimeout(BrowserScriptAction action, string name, Action<int> apply)
    {
        if (action.Options.TryGetValue(name, out var value))
        {
            apply(ParsePositiveInt(value, name));
        }
    }
}
