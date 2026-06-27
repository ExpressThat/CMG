using System.Text.RegularExpressions;

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
        var matcher = TextMatcher(action);
        var text = string.Empty;

        do
        {
            text = automationClient.GetElementText(remoteDebuggingUrl, selector);
            if (matcher(text, action.Arguments[1]) == shouldContain)
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
        return $"Expected text '{action.Arguments[1]}' {mode}{suffix} using {TextMatchMode(action)} match. Actual text: '{text}'.";
    }

    private static Func<string, string, bool> TextMatcher(BrowserScriptAction action)
    {
        var comparison = GetBoolOption(action, "ignoreCase") ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return TextMatchMode(action) switch
        {
            "exact" => (actual, expected) => string.Equals(actual, expected, comparison),
            "regex" or "matches" => (actual, expected) => RegexMatches(actual, expected, GetBoolOption(action, "ignoreCase")),
            _ => (actual, expected) => actual.Contains(expected, comparison)
        };
    }

    private static string TextMatchMode(BrowserScriptAction action)
    {
        var mode = (action.Options.GetValueOrDefault("match") ?? action.Options.GetValueOrDefault("mode") ?? "contains").ToLowerInvariant();
        if (mode is "contains" or "exact" or "regex" or "matches")
        {
            return mode;
        }

        throw new ScriptExecutionException($"{action.Name} option match= must be contains, exact, or regex.");
    }

    private static bool RegexMatches(string actual, string pattern, bool ignoreCase)
    {
        try
        {
            return Regex.IsMatch(actual, pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }
        catch (ArgumentException exception)
        {
            throw new ScriptExecutionException($"Invalid text regex '{pattern}': {exception.Message}");
        }
    }

    private static bool IsNegativeTextAssertion(string name) =>
        name.ToLowerInvariant() is "expectnotext" or "expectnottext" or "notcontains" or
            "notcontainstext" or "tonotcontaintext" or "tohavenotext" or "tohavenottext";

    private static string[] BodyTextAssertionNames() =>
        ["contains", "tocontaintext", "notcontains", "tonotcontaintext"];
}
