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
            TypeWithoutRecorder(remoteDebuggingUrl, automationClient, action, selector);
            return [];
        }

        recorder.CaptureClickPulse();
        automationClient.TypeProgressively(remoteDebuggingUrl, selector, action.Arguments[1], GetTypingDelay(action), recorder.CaptureTypingFrame);
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

    private static IReadOnlyList<string> ExecuteHighlight(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);

        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        var duration = GetIntOption(action, "duration", 1200);
        var color = action.Options.TryGetValue("color", out var rawColor) ? rawColor : "#f59e0b";
        var text = action.Options.TryGetValue("message", out var rawText) ? rawText : string.Empty;
        automationClient.Evaluate(remoteDebuggingUrl, BuildHighlightExpression(selector, color, text, duration));
        return [$"HIGHLIGHT {action.LineNumber:000} {selector} duration={duration}"];
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
            + "if (!element) throw new Error('No element matched selector ' + " + QuoteScriptString(selector) + "); "
            + body
            + " return true; })()";
    }

    private static string BuildHighlightExpression(string selector, string color, string message, int duration)
    {
        return "(() => { "
            + $"const element = {CMG.Browser.BrowserDomScripts.Query(selector)}; "
            + $"if (!element) throw new Error('No element matched selector {selector}'); "
            + "const rect = element.getBoundingClientRect(); "
            + "const overlay = document.createElement('div'); "
            + "overlay.setAttribute('data-cmg-highlight', 'true'); "
            + "overlay.style.cssText = 'position:fixed;pointer-events:none;z-index:2147483644;box-sizing:border-box;border:3px solid "
            + Css(color)
            + ";box-shadow:0 0 0 3px rgba(255,255,255,.75),0 10px 30px rgba(0,0,0,.25);border-radius:6px;background:rgba(245,158,11,.08);'; "
            + "overlay.style.left = `${rect.left}px`; overlay.style.top = `${rect.top}px`; "
            + "overlay.style.width = `${Math.max(1, rect.width)}px`; overlay.style.height = `${Math.max(1, rect.height)}px`; "
            + "if (" + QuoteScriptString(message) + ") { const tag = document.createElement('div'); tag.textContent = "
            + QuoteScriptString(message)
            + "; tag.style.cssText = 'position:absolute;left:0;top:-28px;padding:4px 8px;border-radius:6px;background:"
            + Css(color)
            + ";color:white;font:600 12px/1.3 system-ui,sans-serif;white-space:nowrap;'; overlay.appendChild(tag); } "
            + "document.documentElement.appendChild(overlay); "
            + $"setTimeout(() => overlay.remove(), {duration}); return true; "
            + "})()";
    }

    private static string Css(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal);
}
