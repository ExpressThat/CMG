namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private IReadOnlyList<string> ExecuteEvaluateAssertion(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 0);
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        string actual;
        while (true)
        {
            actual = automationClient.Evaluate(remoteDebuggingUrl, action.Arguments[0]);
            if (MatchesEvaluateAssertion(action, actual))
            {
                return [$"EXPECT_EVAL {action.LineNumber:000} {actual}"];
            }

            if (timeout <= 0 || DateTimeOffset.UtcNow >= deadline)
            {
                break;
            }

            Thread.Sleep(100);
        }

        throw new ScriptExecutionException(EvaluateAssertionFailure(action, actual, timeout));
    }

    private static bool MatchesEvaluateAssertion(BrowserScriptAction action, string actual)
    {
        if (TryExpected(action, "equals", out var expected) || TryExpected(action, "eq", out expected))
        {
            return string.Equals(actual, expected, StringComparison.Ordinal);
        }

        if (TryExpected(action, "contains", out expected))
        {
            return actual.Contains(expected, StringComparison.Ordinal);
        }

        return IsTruthy(actual);
    }

    private static string EvaluateAssertionFailure(BrowserScriptAction action, string actual, int timeout)
    {
        var wait = timeout > 0 ? $" within {timeout}ms" : string.Empty;
        if (TryExpected(action, "equals", out var expected) || TryExpected(action, "eq", out expected))
        {
            return $"Expected evaluated value to equal '{expected}'{wait}. Actual: '{actual}'.";
        }

        if (TryExpected(action, "contains", out expected))
        {
            return $"Expected evaluated value to contain '{expected}'{wait}. Actual: '{actual}'.";
        }

        return $"Expected evaluated value to be truthy{wait}. Actual: '{actual}'.";
    }

    private static bool TryExpected(BrowserScriptAction action, string name, out string value) =>
        action.Options.TryGetValue(name, out value!);

    private static bool IsTruthy(string value) =>
        value.Length > 0 &&
        !value.Equals("false", StringComparison.OrdinalIgnoreCase) &&
        value is not "0" and not "null" and not "undefined";
}
