namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteDispatchEvent(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 2, 2);

        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        automationClient.Evaluate(remoteDebuggingUrl, BuildDispatchEventExpression(selector, action));
        return [$"EVENT {action.LineNumber:000} {action.Arguments[1]} {selector}"];
    }

    private static string BuildDispatchEventExpression(string selector, BrowserScriptAction action)
    {
        var bubbles = GetBoolEventOption(action, "bubbles", defaultValue: true);
        var cancelable = GetBoolEventOption(action, "cancelable", defaultValue: true);
        var eventName = QuoteScriptString(action.Arguments[1]);
        var detail = action.Options.GetValueOrDefault("detail");
        var options = $"{{ bubbles: {bubbles.ToString().ToLowerInvariant()}, cancelable: {cancelable.ToString().ToLowerInvariant()}";
        options += detail is null ? " }" : $", detail: JSON.parse({QuoteScriptString(detail)}) }}";
        var eventType = detail is null ? "Event" : "CustomEvent";

        return "(() => { "
            + $"const element = {CMG.Browser.BrowserDomScripts.Query(selector)}; "
            + $"if (!element) throw new Error('No element matched selector {selector}'); "
            + $"element.dispatchEvent(new {eventType}({eventName}, {options})); "
            + "return true; })()";
    }

    private static bool GetBoolEventOption(BrowserScriptAction action, string name, bool defaultValue)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            return defaultValue;
        }

        return bool.TryParse(value, out var parsed)
            ? parsed
            : throw new ScriptExecutionException($"{action.Name} option {name}= must be true or false.");
    }
}
