using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteFill(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptGifRecorder? recorder)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 2, 2);

        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        automationClient.Clear(remoteDebuggingUrl, selector);
        if (recorder is null)
        {
            automationClient.Type(remoteDebuggingUrl, selector, action.Arguments[1]);
            return [];
        }

        recorder.CaptureClickPulse();
        automationClient.TypeProgressively(remoteDebuggingUrl, selector, action.Arguments[1], recorder.CaptureTypingFrame);
        return [];
    }

    private static IReadOnlyList<string> ExecuteElementDomAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);

        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        automationClient.Evaluate(remoteDebuggingUrl, BuildElementDomExpression(selector, action.Name));
        return [];
    }

    private static string BuildElementDomExpression(string selector, string actionName)
    {
        var body = actionName.ToLowerInvariant() switch
        {
            "check" => "element.checked = true; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true }));",
            "uncheck" => "element.checked = false; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true }));",
            "focus" => "element.focus({ preventScroll: true });",
            "blur" => "element.blur();",
            "selecttext" => "element.focus({ preventScroll: true }); element.select?.();",
            _ => throw new ScriptExecutionException($"Unknown element action '{actionName}'.")
        };

        return "(() => { "
            + $"const element = {CMG.Browser.BrowserDomScripts.Query(selector)}; "
            + $"if (!element) throw new Error('No element matched selector {selector}'); "
            + body
            + " return true; })()";
    }
}
