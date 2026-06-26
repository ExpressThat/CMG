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
            "computedstyle" => ComputedStyle(remoteDebuggingUrl, automationClient, action),
            "property" => Property(remoteDebuggingUrl, automationClient, action),
            "count" or "locatorcount" => ElementCount(remoteDebuggingUrl, automationClient, action),
            "boundingbox" => BoundingBox(remoteDebuggingUrl, automationClient, action),
            "alltextcontents" or "allinnertexts" => AllText(remoteDebuggingUrl, automationClient, action),
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

    private static IReadOnlyList<string> ComputedStyle(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        var expression = $"getComputedStyle(element).getPropertyValue({QuoteScriptString(action.Arguments[1])}) ?? ''";
        return ElementEvaluate(remoteDebuggingUrl, automationClient, action, "STYLE", expression, 2);
    }

    private static IReadOnlyList<string> Property(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        var expression = "(() => { const path = "
            + QuoteScriptString(action.Arguments[1])
            + ".split('.'); let value = element; for (const part of path) value = value?.[part]; "
            + "return typeof value === 'string' ? value : JSON.stringify(value ?? ''); })()";
        return ElementEvaluate(remoteDebuggingUrl, automationClient, action, "PROPERTY", expression, 2);
    }

    private static IReadOnlyList<string> ElementCount(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        var script = $"{CMG.Browser.BrowserDomScripts.QueryAll(selector)}.length";
        return [$"COUNT {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, script)}"];
    }

    private static IReadOnlyList<string> BoundingBox(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        var script = BuildSelectorEvaluateScript(
            selector,
            "element => { const r = element.getBoundingClientRect(); return JSON.stringify({ x: r.x, y: r.y, width: r.width, height: r.height }); }",
            many: false);
        return [$"BOUNDING_BOX {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, script)}"];
    }

    private static IReadOnlyList<string> AllText(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        var property = action.Name.Equals("allInnerTexts", StringComparison.OrdinalIgnoreCase) ? "innerText" : "textContent";
        var script = $"JSON.stringify(Array.from({CMG.Browser.BrowserDomScripts.QueryAll(selector)}).map(element => element.{property} ?? ''))";
        return [$"TEXTS {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, script)}"];
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
