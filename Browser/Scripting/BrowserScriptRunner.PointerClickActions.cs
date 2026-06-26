namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteClick(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        if (!HasClickOptions(action))
        {
            automationClient.Click(remoteDebuggingUrl, selector);
            return [];
        }

        var spec = ClickSpec.From(action);
        automationClient.Hover(remoteDebuggingUrl, selector);
        automationClient.Evaluate(remoteDebuggingUrl, BuildClickExpression(selector, spec));
        return [$"MOUSE_EVENT {action.LineNumber:000} {spec.OutputEvent} {selector}"];
    }

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

    private static bool HasClickOptions(BrowserScriptAction action) =>
        action.Options.Keys.Any(key => key is "button" or "clickCount" or "count" or "delay" or "modifiers" or "x" or "y");

    private static string BuildMouseEventExpression(string selector, string eventName, int button)
    {
        var buttons = button == 2 ? 2 : 1;
        return "(() => { "
            + $"const element = {CMG.Browser.BrowserDomScripts.Query(selector)}; "
            + $"if (!element) throw new Error('No element matched selector {selector}'); "
            + "const rect = element.getBoundingClientRect(); "
            + "const options = { bubbles: true, cancelable: true, "
            + $"button: {button}, buttons: {buttons}, "
            + "clientX: rect.left + rect.width / 2, clientY: rect.top + rect.height / 2 }; "
            + $"element.dispatchEvent(new MouseEvent('{eventName}', options)); "
            + "return true; })()";
    }

    private static string BuildClickExpression(string selector, ClickSpec spec)
    {
        var delay = spec.DelayMilliseconds;
        return "(async () => { "
            + $"const element = {CMG.Browser.BrowserDomScripts.Query(selector)}; "
            + $"if (!element) throw new Error('No element matched selector {selector}'); "
            + "const rect = element.getBoundingClientRect(); "
            + "const PointerCtor = window.PointerEvent || MouseEvent; "
            + $"const clientX = rect.left + {spec.XExpression}; const clientY = rect.top + {spec.YExpression}; "
            + $"const init = {{ bubbles: true, cancelable: true, button: {spec.Button}, buttons: {spec.Buttons}, clientX, clientY, {spec.ModifiersInit} }}; "
            + "const wait = ms => new Promise(resolve => setTimeout(resolve, ms)); "
            + $"for (let i = 1; i <= {spec.ClickCount}; i++) {{ "
            + "element.dispatchEvent(new PointerCtor('pointerdown', init)); "
            + "element.dispatchEvent(new MouseEvent('mousedown', init)); "
            + "element.dispatchEvent(new PointerCtor('pointerup', { ...init, buttons: 0 })); "
            + "element.dispatchEvent(new MouseEvent('mouseup', { ...init, buttons: 0 })); "
            + $"{spec.PrimaryEventScript} "
            + $"if ({delay} > 0 && i < {spec.ClickCount}) await wait({delay}); "
            + "} "
            + $"{spec.DoubleClickScript} "
            + "return true; })()";
    }

    private sealed record ClickSpec(
        int Button,
        int Buttons,
        int ClickCount,
        int DelayMilliseconds,
        string XExpression,
        string YExpression,
        string ModifiersInit,
        string OutputEvent,
        string PrimaryEventScript,
        string DoubleClickScript)
    {
        public static ClickSpec From(BrowserScriptAction action)
        {
            var buttonName = (action.Options.GetValueOrDefault("button") ?? "left").ToLowerInvariant();
            var (button, buttons, output, primary) = buttonName switch
            {
                "left" => (0, 1, "click", "element.dispatchEvent(new MouseEvent('click', { ...init, detail: i }));"),
                "right" => (2, 2, "contextmenu", "element.dispatchEvent(new MouseEvent('contextmenu', { ...init, buttons: 0, detail: i }));"),
                "middle" => (1, 4, "auxclick", "element.dispatchEvent(new MouseEvent('auxclick', { ...init, buttons: 0, detail: i }));"),
                _ => throw new ScriptExecutionException("click option button= must be left, right, or middle.")
            };
            var count = action.Options.ContainsKey("clickCount")
                ? GetIntOption(action, "clickCount", 1)
                : GetIntOption(action, "count", 1);
            if (count < 1)
            {
                throw new ScriptExecutionException("click option clickCount= must be one or greater.");
            }
            var delay = GetIntOption(action, "delay", 0);
            if (delay < 0)
            {
                throw new ScriptExecutionException("click option delay= must be zero or greater.");
            }

            return new(
                button,
                buttons,
                count,
                delay,
                CoordinateExpression(action, "x", "rect.width / 2"),
                CoordinateExpression(action, "y", "rect.height / 2"),
                BuildModifiersInit(action),
                count > 1 && button == 0 ? "dblclick" : output,
                primary,
                count > 1 && button == 0 ? "element.dispatchEvent(new MouseEvent('dblclick', { ...init, buttons: 0, detail: 2 }));" : string.Empty);
        }

        private static string CoordinateExpression(BrowserScriptAction action, string name, string fallback) =>
            action.Options.TryGetValue(name, out var value) ? NumberOption(value, $"click option {name}=") : fallback;

        private static string NumberOption(string value, string name) =>
            double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) && number >= 0
                ? number.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : throw new ScriptExecutionException($"{name} must be zero or greater.");

        private static string BuildModifiersInit(BrowserScriptAction action)
        {
            var modifiers = (action.Options.GetValueOrDefault("modifiers") ?? string.Empty)
                .Split([',', '+'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => value.ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var allowed = new HashSet<string>(["alt", "control", "ctrl", "shift", "meta", "cmd", "command"], StringComparer.OrdinalIgnoreCase);
            if (modifiers.Any(modifier => !allowed.Contains(modifier)))
            {
                throw new ScriptExecutionException("click option modifiers= supports Alt, Control, Meta, and Shift.");
            }
            return $"altKey: {modifiers.Contains("alt").ToString().ToLowerInvariant()}, "
                + $"ctrlKey: {(modifiers.Contains("control") || modifiers.Contains("ctrl")).ToString().ToLowerInvariant()}, "
                + $"shiftKey: {modifiers.Contains("shift").ToString().ToLowerInvariant()}, "
                + $"metaKey: {(modifiers.Contains("meta") || modifiers.Contains("cmd") || modifiers.Contains("command")).ToString().ToLowerInvariant()}";
        }
    }
}
