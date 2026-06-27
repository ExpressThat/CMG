namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteHover(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeSelectorArgument(action);
        RequireArgumentCount(action, 1, 1);
        var selector = ResolveSelector(remoteDebuggingUrl, automationClient, action);
        if (!HasHoverOptions(action))
        {
            automationClient.Hover(remoteDebuggingUrl, selector);
            return [];
        }

        automationClient.Evaluate(remoteDebuggingUrl, BuildHoverExpression(selector, HoverSpec.From(action)));
        return [];
    }

    private static bool HasHoverOptions(BrowserScriptAction action) =>
        action.Options.Keys.Any(key => key is "modifiers" or "x" or "y");

    private static string BuildHoverExpression(string selector, HoverSpec spec) =>
        "(() => { "
        + $"const element = {CMG.Browser.BrowserDomScripts.Query(selector)}; "
        + $"if (!element) throw new Error('No element matched selector {selector}'); "
        + "const rect = element.getBoundingClientRect(); "
        + "const PointerCtor = window.PointerEvent || MouseEvent; "
        + $"const clientX = rect.left + {spec.XExpression}; const clientY = rect.top + {spec.YExpression}; "
        + $"const init = {{ bubbles: true, cancelable: true, button: 0, buttons: 0, clientX, clientY, {spec.ModifiersInit} }}; "
        + "element.dispatchEvent(new PointerCtor('pointerover', init)); "
        + "element.dispatchEvent(new MouseEvent('mouseover', init)); "
        + "element.dispatchEvent(new PointerCtor('pointermove', init)); "
        + "element.dispatchEvent(new MouseEvent('mousemove', init)); "
        + "return true; })()";

    private sealed record HoverSpec(string XExpression, string YExpression, string ModifiersInit)
    {
        public static HoverSpec From(BrowserScriptAction action) =>
            new(
                CoordinateExpression(action, "x", "rect.width / 2"),
                CoordinateExpression(action, "y", "rect.height / 2"),
                BuildModifiersInit(action));

        private static string CoordinateExpression(BrowserScriptAction action, string name, string fallback) =>
            action.Options.TryGetValue(name, out var value) ? NumberOption(value, $"hover option {name}=") : fallback;

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
                throw new ScriptExecutionException("hover option modifiers= supports Alt, Control, Meta, and Shift.");
            }

            return $"altKey: {modifiers.Contains("alt").ToString().ToLowerInvariant()}, "
                + $"ctrlKey: {(modifiers.Contains("control") || modifiers.Contains("ctrl")).ToString().ToLowerInvariant()}, "
                + $"shiftKey: {modifiers.Contains("shift").ToString().ToLowerInvariant()}, "
                + $"metaKey: {(modifiers.Contains("meta") || modifiers.Contains("cmd") || modifiers.Contains("command")).ToString().ToLowerInvariant()}";
        }
    }
}
