namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteElementGetter(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        return action.Name.ToLowerInvariant() switch
        {
            "textcontent" or "innertext" => ElementText(remoteDebuggingUrl, automationClient, action),
            "inputvalue" => ElementEvaluate(remoteDebuggingUrl, automationClient, action, "VALUE", "element.value ?? ''", 1),
            "getattribute" => GetAttribute(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown element getter '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> ElementText(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        return [$"TEXT {action.LineNumber:000} {automationClient.GetElementText(remoteDebuggingUrl, selector)}"];
    }

    private static IReadOnlyList<string> GetAttribute(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        var expression = $"element.getAttribute({QuoteScriptString(action.Arguments[1])}) ?? ''";
        return ElementEvaluate(remoteDebuggingUrl, automationClient, action, "ATTRIBUTE", expression, 2);
    }

    private static IReadOnlyList<string> ElementEvaluate(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string label,
        string expression,
        int maxArguments)
    {
        RequireArgumentCount(action, maxArguments, maxArguments);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        var script = BuildSelectorEvaluateScript(selector, expression, many: false);
        return [$"{label} {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, script)}"];
    }
}
