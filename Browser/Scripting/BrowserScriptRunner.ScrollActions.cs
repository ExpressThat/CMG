using CMG.Browser.Scripting.Recording;
using CMG.Runner;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteScrollAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptGifRecorder? recorder)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "scrollto" => ScrollTo(remoteDebuggingUrl, automationClient, action),
            "scrollby" => ScrollBy(remoteDebuggingUrl, automationClient, action),
            "wheel" => Wheel(remoteDebuggingUrl, automationClient, action, recorder),
            _ => throw new ScriptExecutionException($"Unknown scroll action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> ScrollTo(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        var target = ReadScrollTarget(action);
        var selector = ResolveScrollSelector(remoteDebuggingUrl, automationClient, action.Options.GetValueOrDefault("selector"), action.LineNumber);
        automationClient.Evaluate(remoteDebuggingUrl, ScrollScript(selector, target, absolute: true));
        return [$"SCROLL_TO {action.LineNumber:000} {target.X},{target.Y}"];
    }

    private static IReadOnlyList<string> ScrollBy(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        var target = ReadScrollTarget(action);
        var selector = ResolveScrollSelector(remoteDebuggingUrl, automationClient, action.Options.GetValueOrDefault("selector"), action.LineNumber);
        automationClient.Evaluate(remoteDebuggingUrl, ScrollScript(selector, target, absolute: false));
        return [$"SCROLL_BY {action.LineNumber:000} {target.X},{target.Y}"];
    }

    private static IReadOnlyList<string> Wheel(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptGifRecorder? recorder)
    {
        var delta = new ScrollTarget(GetSignedOption(action, "deltaX", 0), GetSignedOption(action, "deltaY", 100));
        if (action.Arguments.Count > 0 || action.Options.ContainsKey("x") || action.Options.ContainsKey("selector"))
        {
            recorder?.MoveMouse(ToWheelPointerAction(action), dragging: false);
        }

        var selector = ResolveScrollSelector(remoteDebuggingUrl, automationClient, ReadWheelSelector(action), action.LineNumber);
        automationClient.Evaluate(remoteDebuggingUrl, WheelScript(action, selector, delta));
        return [$"WHEEL {action.LineNumber:000} {delta.X},{delta.Y}"];
    }

    private static ScrollTarget ReadScrollTarget(BrowserScriptAction action)
    {
        if (action.Arguments.Count is 1)
        {
            return action.Arguments[0].ToLowerInvariant() switch
            {
                "top" or "left" => new(0, 0),
                "bottom" => new(0, int.MaxValue),
                "right" => new(int.MaxValue, 0),
                _ => throw new ScriptExecutionException($"{action.Name} alias must be top, bottom, left, or right.")
            };
        }

        if (action.Arguments.Count is 2)
        {
            return new(ParseSigned(action.Arguments[0], "x"), ParseSigned(action.Arguments[1], "y"));
        }

        if (action.Arguments.Count > 2)
        {
            throw new ScriptExecutionException($"{action.Name} expects an alias, x/y arguments, or x= y= options.");
        }

        return new(GetSignedOption(action, "x", 0), GetSignedOption(action, "y", 0));
    }

    private static string ScrollScript(string? selector, ScrollTarget target, bool absolute)
    {
        var method = absolute ? "scrollTo" : "scrollBy";
        var element = string.IsNullOrWhiteSpace(selector) ? "window" : $"document.querySelector({QuoteScriptString(selector)})";
        var guard = string.IsNullOrWhiteSpace(selector) ? string.Empty : $"if (!target) throw new Error('No element matched selector {selector}'); ";
        return $"(() => {{ const target = {element}; {guard} target.{method}({target.X}, {target.Y}); return true; }})()";
    }

    private static string WheelScript(BrowserScriptAction action, string? selector, ScrollTarget delta)
    {
        var point = action.Options.ContainsKey("x")
            ? $"{{ x: {GetSignedOption(action, "x", 0)}, y: {GetSignedOption(action, "y", 0)} }}"
            : "null";
        return $"(() => {{ const point = {point}; const target = {WheelTarget(selector)}; const init = {{ bubbles: true, cancelable: true, deltaX: {delta.X}, deltaY: {delta.Y}, clientX: point?.x ?? 0, clientY: point?.y ?? 0 }}; target.dispatchEvent(new WheelEvent('wheel', init)); target.scrollBy?.({delta.X}, {delta.Y}); if (target === document) window.scrollBy({delta.X}, {delta.Y}); return true; }})()";
    }

    private static string WheelTarget(string? selector) =>
        string.IsNullOrWhiteSpace(selector)
            ? "document"
            : $"document.querySelector({QuoteScriptString(selector)}) || (() => {{ throw new Error('No element matched selector {selector}'); }})()";

    private static string? ReadWheelSelector(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("selector", out var selector))
        {
            return selector;
        }

        return action.Arguments.Count is 1 && !IsWheelAlias(action.Arguments[0]) ? action.Arguments[0] : null;
    }

    private static BrowserScriptAction ToWheelPointerAction(BrowserScriptAction action)
    {
        var selector = ReadWheelSelector(action);
        if (selector is null)
        {
            return action;
        }

        var options = new Dictionary<string, string>(action.Options, StringComparer.OrdinalIgnoreCase)
        {
            ["selector"] = selector,
            ["edge"] = action.Options.GetValueOrDefault("edge") ?? "center"
        };
        return action with { Arguments = [], Options = options };
    }

    private static bool IsWheelAlias(string value) =>
        value.ToLowerInvariant() is "center" or "top" or "bottom" or "left" or "right" or "topleft" or "topright" or "bottomleft" or "bottomright";

    private static string? ResolveScrollSelector(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, string? selector, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(selector))
        {
            return null;
        }

        foreach (var expression in CmgLocator.PrefixExpressions(selector, lineNumber))
        {
            automationClient.Evaluate(remoteDebuggingUrl, expression);
        }

        return CmgLocator.Resolve(selector, lineNumber).Selector;
    }

    private static int GetSignedOption(BrowserScriptAction action, string name, int defaultValue) =>
        action.Options.TryGetValue(name, out var value) ? ParseSigned(value, name) : defaultValue;

    private static int ParseSigned(string value, string name) =>
        int.TryParse(value, out var number) ? number : throw new ScriptExecutionException($"{name} must be a whole number.");

    private readonly record struct ScrollTarget(int X, int Y);
}
