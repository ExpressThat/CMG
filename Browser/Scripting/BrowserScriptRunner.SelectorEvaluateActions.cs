namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteSelectorEvaluate(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 2, 2);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        var many = action.Name.Equals("evaluateAll", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Equals("evalAll", StringComparison.OrdinalIgnoreCase);
        var script = BuildSelectorEvaluateScript(selector, action.Arguments[1], many);
        return [$"EVALUATE {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, script)}"];
    }

    private static string BuildSelectorEvaluateScript(string selector, string expression, bool many)
    {
        var targetName = many ? "elements" : "element";
        var targetLookup = many
            ? $"Array.from(document.querySelectorAll({QuoteScriptString(selector)}))"
            : $"document.querySelector({QuoteScriptString(selector)})";
        var missingCheck = many
            ? $"if ({targetName}.length === 0) throw new Error('No elements matched selector {selector}'); "
            : $"if (!{targetName}) throw new Error('No element matched selector {selector}'); ";

        return "(() => { "
            + $"const {targetName} = {targetLookup}; "
            + missingCheck
            + $"const value = ({expression}); "
            + $"return typeof value === 'function' ? value({targetName}) : value; "
            + "})()";
    }
}
