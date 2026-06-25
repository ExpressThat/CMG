namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteMouseClickVariant(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);

        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        var name = action.Name.ToLowerInvariant();
        var eventName = name is "rightclick" or "contextclick" ? "contextmenu" : "dblclick";
        var button = eventName == "contextmenu" ? 2 : 0;

        automationClient.Hover(remoteDebuggingUrl, selector);
        automationClient.Evaluate(remoteDebuggingUrl, BuildMouseEventExpression(selector, eventName, button));

        return [$"MOUSE_EVENT {action.LineNumber:000} {eventName} {selector}"];
    }

    private static string BuildMouseEventExpression(string selector, string eventName, int button)
    {
        var buttons = button == 2 ? 2 : 1;
        return "(() => { "
            + $"const element = document.querySelector({QuoteScriptString(selector)}); "
            + $"if (!element) throw new Error('No element matched selector {selector}'); "
            + "const rect = element.getBoundingClientRect(); "
            + "const options = { bubbles: true, cancelable: true, "
            + $"button: {button}, buttons: {buttons}, "
            + "clientX: rect.left + rect.width / 2, clientY: rect.top + rect.height / 2 }; "
            + $"element.dispatchEvent(new MouseEvent('{eventName}', options)); "
            + "return true; })()";
    }
}
