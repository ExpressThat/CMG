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

        return action with { Arguments = [$"{locator.Key}={locator.Value}", .. action.Arguments] };
    }

    private static string SelectorArgument(BrowserScriptAction action, int argumentIndex)
    {
        action = NormalizeSelectorArgument(action);
        return argumentIndex < action.Arguments.Count ? action.Arguments[argumentIndex] : string.Empty;
    }

    private static bool IsLocatorOption(string key) =>
        key is "css" or "testid" or "text" or "role" or "label" or "placeholder" or "alt" or "title" or "xpath";
}
