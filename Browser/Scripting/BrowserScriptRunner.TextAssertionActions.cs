namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteAssertText(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        action = NormalizeTextAssertion(action);
        RequireArgumentCount(action, 2, 2);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        var timeout = GetIntOption(action, "timeout", 0);
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        var shouldContain = !IsNegativeTextAssertion(action.Name);
        var text = string.Empty;

        do
        {
            text = automationClient.GetElementText(remoteDebuggingUrl, selector);
            if (text.Contains(action.Arguments[1], StringComparison.Ordinal) == shouldContain)
            {
                return [];
            }

            if (timeout <= 0)
            {
                break;
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException(TextAssertionFailure(action, text, timeout, shouldContain));
    }

    private static BrowserScriptAction NormalizeTextAssertion(BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        var name = action.Name.ToLowerInvariant();
        if (!BodyTextAssertionNames().Contains(name) || action.Arguments.Count != 1)
        {
            return action;
        }

        return action with { Arguments = ["body", action.Arguments[0]] };
    }

    private static string TextAssertionFailure(BrowserScriptAction action, string text, int timeout, bool shouldContain)
    {
        var mode = shouldContain ? "was not found" : "was still found";
        var suffix = timeout > 0 ? $" within {timeout}ms" : string.Empty;
        return $"Expected text '{action.Arguments[1]}' {mode}{suffix}. Actual text: '{text}'.";
    }

    private static bool IsNegativeTextAssertion(string name) =>
        name.ToLowerInvariant() is "expectnotext" or "expectnottext" or "notcontains" or
            "notcontainstext" or "tonotcontaintext" or "tohavenotext" or "tohavenottext";

    private static string[] BodyTextAssertionNames() =>
        ["contains", "tocontaintext", "notcontains", "tonotcontaintext"];
}
