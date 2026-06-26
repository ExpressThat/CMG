using CMG.Runner;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static string ResolveSelector(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        int argumentIndex = 0)
    {
        var selector = SelectorArgument(action, argumentIndex);
        foreach (var expression in CmgLocator.PrefixExpressions(selector, action.LineNumber))
        {
            automationClient.Evaluate(remoteDebuggingUrl, expression);
        }

        return CmgLocator.Resolve(selector, action.LineNumber).Selector;
    }

    private static BrowserScriptAction NormalizeSelectorArgument(BrowserScriptAction action)
    {
        var locator = action.Options.FirstOrDefault(pair => IsLocatorOption(pair.Key));
        if (string.IsNullOrWhiteSpace(locator.Key))
        {
            return action;
        }

        var locatorArgument = $"{locator.Key}={locator.Value}";
        if (action.Arguments.Count > 0 && action.Arguments[0].Equals(locatorArgument, StringComparison.Ordinal))
        {
            return action;
        }

        return action with { Arguments = [locatorArgument, .. action.Arguments] };
    }

    private static string SelectorArgument(BrowserScriptAction action, int argumentIndex)
    {
        action = NormalizeSelectorArgument(action);
        return argumentIndex < action.Arguments.Count ? action.Arguments[argumentIndex] : string.Empty;
    }

    private static bool IsLocatorOption(string key) =>
        key is "css" or "testid" or "testId" or "data-testid" or "text" or "textExact" or "textRegex" or
            "role" or "roleRegex" or "label" or "labelExact" or "labelRegex" or "placeholder" or "placeholderExact" or
            "placeholderRegex" or "alt" or "altExact" or "altRegex" or "title" or "titleExact" or "titleRegex" or "xpath" or
            "first" or "last" or "nth" or "has" or "hasNot" or "hasText" or "hasNotText" or "visible" or
            "or" or "and" or "shadow" or "shadowText";
}
