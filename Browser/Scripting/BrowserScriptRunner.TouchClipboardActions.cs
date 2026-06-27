namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteTap(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        Recording.ScriptGifRecorder? recorder)
    {
        action = NormalizeSelectorArgument(action);
        var hasCoordinates = action.Options.ContainsKey("x") || action.Options.ContainsKey("y");
        if (hasCoordinates)
        {
            var point = BrowserMouseTargetResolver.Resolve(remoteDebuggingUrl, automationClient, action);
            recorder?.MoveMouse(action, dragging: false);
            automationClient.Evaluate(remoteDebuggingUrl, BuildTapExpression(point));
            return [$"TAP {action.LineNumber:000} {FormatPoint(point)}"];
        }

        RequireArgumentCount(action, 1, 1);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        automationClient.Evaluate(remoteDebuggingUrl, BuildTapExpression(selector));
        return [$"TAP {action.LineNumber:000} {selector}"];
    }

    private static IReadOnlyList<string> ExecuteClipboardAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "setclipboard" or "writeclipboard" => SetClipboard(remoteDebuggingUrl, automationClient, action),
            "readclipboard" => ReadClipboard(remoteDebuggingUrl, automationClient, action),
            "clearclipboard" => ClearClipboard(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown clipboard action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> SetClipboard(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        automationClient.Evaluate(remoteDebuggingUrl, ClipboardExpression(action.Arguments[0]));
        return [$"CLIPBOARD_SET {action.LineNumber:000} {action.Arguments[0].Length}"];
    }

    private static IReadOnlyList<string> ReadClipboard(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        return [$"CLIPBOARD {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, ClipboardReadExpression())}"];
    }

    private static IReadOnlyList<string> ClearClipboard(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.Evaluate(remoteDebuggingUrl, ClipboardExpression(string.Empty));
        return [$"CLIPBOARD_CLEARED {action.LineNumber:000}"];
    }

    private static string BuildTapExpression(string selector) =>
        $"(() => {{ const element = {CMG.Browser.BrowserDomScripts.Query(selector)}; if (!element) throw new Error('No element matched selector {selector}'); const rect = element.getBoundingClientRect(); {TapBody("element", "rect.left + rect.width / 2", "rect.top + rect.height / 2")} }})()";

    private static string BuildTapExpression(ElementPoint point) =>
        $"(() => {{ const x = {point.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}; const y = {point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}; const element = document.elementFromPoint(x, y); if (!element) throw new Error('No element at tap target'); {TapBody("element", "x", "y")} }})()";

    private static string TapBody(string element, string x, string y) =>
        $"const init = {{ bubbles: true, cancelable: true, composed: true, clientX: {x}, clientY: {y}, pointerType: 'touch', isPrimary: true }}; "
        + "const PointerCtor = window.PointerEvent || window.MouseEvent; "
        + $"{element}.dispatchEvent(new PointerCtor('pointerdown', init)); "
        + $"try {{ const touch = new Touch({{ identifier: Date.now(), target: {element}, clientX: {x}, clientY: {y} }}); {element}.dispatchEvent(new TouchEvent('touchstart', {{ bubbles: true, cancelable: true, touches: [touch], targetTouches: [touch], changedTouches: [touch] }})); {element}.dispatchEvent(new TouchEvent('touchend', {{ bubbles: true, cancelable: true, touches: [], targetTouches: [], changedTouches: [touch] }})); }} catch {{ }} "
        + $"{element}.dispatchEvent(new PointerCtor('pointerup', init)); {element}.click(); return true;";

    private static string ClipboardExpression(string value) =>
        $"(() => {{ window.__cmgClipboard = {QuoteScriptString(value)}; Object.defineProperty(navigator, 'clipboard', {{ configurable: true, value: {{ writeText: text => {{ window.__cmgClipboard = String(text); return Promise.resolve(); }}, readText: () => Promise.resolve(window.__cmgClipboard || '') }} }}); return true; }})()";

    private static string ClipboardReadExpression() =>
        "(() => window.__cmgClipboard || '')()";
}
