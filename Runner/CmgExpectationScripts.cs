using CMG.Browser;

namespace CMG.Runner;

public static class CmgExpectationScripts
{
    public static IReadOnlyList<string> Element(CmgNode action, string mode)
    {
        var plan = Expressions(action, mode);
        return plan.Error is null
            ? [.. plan.PrefixExpressions.Select(expression => Line(expression)), Line(plan.Expression)]
            : [Line(Fail(plan.Error))];
    }

    public static CmgExpectationPlan Expressions(CmgNode action, string mode)
    {
        action = NormalizeLocatorArgument(action);
        if (action.Arguments.Count < RequiredArgumentCount(mode))
        {
            return CmgExpectationPlan.Fail($"{action.Kind} received too few arguments.");
        }

        var resolved = CmgLocator.Resolve(action.Arguments[0], action.LineNumber);
        return new CmgExpectationPlan(
            CmgLocator.PrefixExpressions(action.Arguments[0], action.LineNumber),
            Build(action, mode, resolved.Selector),
            null);
    }

    private static string Build(CmgNode action, string mode, string selector)
    {
        var timeout = Timeout(action);
        if (mode.Equals("count", StringComparison.Ordinal))
        {
            return BuildCount(action, selector, timeout);
        }
        if (IsStateMode(mode))
        {
            return BuildState(action, mode, selector);
        }

        var body = mode switch
        {
            "value" => $"const actual = element.value ?? ''; const ok = actual.includes({QuoteJs(action.Arguments[1])}); const message = 'Expected value to contain {action.Arguments[1]}, got ' + actual + '.';",
            "values" => $"const actual = Array.from(element.selectedOptions || []).map(option => option.value); const expected = {ExpectedValuesJs(action)}; const ok = actual.length === expected.length && expected.every((value, index) => actual[index] === value); const message = 'Expected values ' + expected.join(',') + ', got ' + actual.join(',') + '.';",
            "attribute" => $"const actual = element.getAttribute({QuoteJs(action.Arguments[1])}); const ok = actual?.includes({QuoteJs(action.Arguments[2])}); const message = 'Expected attribute {action.Arguments[1]} to contain {action.Arguments[2]}, got ' + actual + '.';",
            "class" => $"const actual = element.className || ''; const ok = String(actual).split(/\\s+/).includes({QuoteJs(action.Arguments[1])}) || String(actual).includes({QuoteJs(action.Arguments[1])}); const message = 'Expected class to contain {action.Arguments[1]}, got ' + actual + '.';",
            "id" => $"const actual = element.id || ''; const ok = actual === {QuoteJs(action.Arguments[1])}; const message = 'Expected id to be {action.Arguments[1]}, got ' + actual + '.';",
            "css" => $"const actual = getComputedStyle(element).getPropertyValue({QuoteJs(action.Arguments[1])}).trim(); const ok = actual === {QuoteJs(action.Arguments[2])} || actual.includes({QuoteJs(action.Arguments[2])}); const message = 'Expected CSS {action.Arguments[1]} to contain {action.Arguments[2]}, got ' + actual + '.';",
            "property" => $"const actual = {QuoteJs(action.Arguments[1])}.split('.').reduce((value, key) => value?.[key], element); const ok = String(actual ?? '').includes({QuoteJs(action.Arguments[2])}); const message = 'Expected property {action.Arguments[1]} to contain {action.Arguments[2]}, got ' + String(actual ?? '') + '.';",
            "accessiblename" => $"const actual = accessibleName(element); const ok = actual.includes({QuoteJs(action.Arguments[1])}); const message = 'Expected accessible name to contain {action.Arguments[1]}, got ' + actual + '.';",
            "role" => $"const actual = element.getAttribute('role') || implicitRole(element); const ok = actual === {QuoteJs(action.Arguments[1])}; const message = 'Expected role to be {action.Arguments[1]}, got ' + actual + '.';",
            _ => $"const actual = Boolean(element.checked); const expected = {ExpectedChecked(action, mode).ToString().ToLowerInvariant()}; const ok = actual === expected; const message = 'Expected checked to be ' + expected + ', got ' + actual + '.';"
        };

        return $"(() => {{ {AccessibilityHelpers()} const element = {BrowserDomScripts.Query(selector)}; if (!element) throw new Error('No element matched selector {selector}.'); {body} if (!ok) throw new Error(message); return true; }})()";
    }

    private static string BuildState(CmgNode action, string mode, string selector)
    {
        var check = mode switch
        {
            "visible" => "const style = getComputedStyle(element); const rect = element.getBoundingClientRect(); const ok = rect.width > 0 && rect.height > 0 && style.visibility !== 'hidden' && style.display !== 'none' && Number(style.opacity || '1') !== 0; const message = 'Expected element to be visible.';",
            "hidden" => "const style = getComputedStyle(element); const rect = element.getBoundingClientRect(); const ok = rect.width === 0 || rect.height === 0 || style.visibility === 'hidden' || style.display === 'none' || Number(style.opacity || '1') === 0; const message = 'Expected element to be hidden.';",
            "enabled" => "const ok = !element.matches(':disabled,[aria-disabled=\"true\"]'); const message = 'Expected element to be enabled.';",
            "disabled" => "const ok = element.matches(':disabled,[aria-disabled=\"true\"]'); const message = 'Expected element to be disabled.';",
            "attached" => "const ok = element.isConnected; const message = 'Expected element to be attached.';",
            "editable" => "const formEditable = element.matches('input,textarea,select') && !element.matches(':disabled,[readonly],[aria-disabled=\"true\"]'); const ok = formEditable || element.isContentEditable; const message = 'Expected element to be editable.';",
            "empty" => "const value = 'value' in element ? element.value : element.textContent; const ok = String(value ?? '').length === 0; const message = 'Expected element to be empty.';",
            "focused" => "const ok = document.activeElement === element; const message = 'Expected element to be focused.';",
            "inviewport" => "const rect = element.getBoundingClientRect(); const ok = rect.bottom > 0 && rect.right > 0 && rect.top < innerHeight && rect.left < innerWidth; const message = 'Expected element to intersect the viewport.';",
            _ => string.Empty
        };
        if (mode.Equals("detached", StringComparison.Ordinal))
        {
            return $"(() => {{ const element = {BrowserDomScripts.Query(selector)}; if (!element || !element.isConnected) return true; throw new Error('Expected element to be detached.'); }})()";
        }
        if (mode.Equals("hidden", StringComparison.Ordinal))
        {
            return $"(() => {{ const element = {BrowserDomScripts.Query(selector)}; if (!element || !element.isConnected) return true; {check} if (!ok) throw new Error(message); return true; }})()";
        }

        return $"(() => {{ const element = {BrowserDomScripts.Query(selector)}; if (!element) throw new Error('No element matched selector {selector}.'); {check} if (!ok) throw new Error(message); return true; }})()";
    }

    private static string BuildCount(CmgNode action, string selector, int timeout) =>
        $$"""
        new Promise((resolve, reject) => {
          const selector = {{QuoteJs(selector)}};
          const expected = {{ExpectedCount(action)}};
          const deadline = Date.now() + {{timeout}};
          const poll = () => {
            const actual = window.__cmgQueryAll?.(selector)?.length ?? document.querySelectorAll(selector).length;
            if (actual === expected) { resolve(true); return; }
            if (Date.now() >= deadline) { reject(new Error('Expected ' + expected + ' elements for ' + selector + ', got ' + actual + '.')); return; }
            setTimeout(poll, 50);
          };
          poll();
        })
        """;

    private static int RequiredArgumentCount(string mode) => mode switch
    {
        "attribute" or "css" or "property" => 3,
        "checked" or "unchecked" => 1,
        "visible" or "hidden" or "enabled" or "disabled" or "attached" or "detached" or
        "editable" or "empty" or "focused" or "inviewport" => 1,
        _ => 2
    };

    private static bool ExpectedChecked(CmgNode action, string mode) =>
        !mode.Equals("unchecked", StringComparison.Ordinal) &&
        (action.Arguments.Count < 2 || !action.Arguments[1].Equals("false", StringComparison.OrdinalIgnoreCase));

    private static int ExpectedCount(CmgNode action) =>
        int.TryParse(action.Arguments[1], out var count) ? count : -1;

    private static string ExpectedValuesJs(CmgNode action) =>
        "[" + string.Join(", ", action.Arguments.Skip(1).Select(QuoteJs)) + "]";

    private static string AccessibilityHelpers() =>
        "const implicitRole = e => e.tagName === 'BUTTON' ? 'button' : e.tagName === 'A' && e.hasAttribute('href') ? 'link' : e.tagName === 'IMG' ? 'img' : e.tagName === 'INPUT' || e.tagName === 'TEXTAREA' ? 'textbox' : e.tagName === 'SELECT' ? 'combobox' : /^H[1-6]$/.test(e.tagName) ? 'heading' : ''; const accessibleName = e => e.getAttribute('aria-label') || e.getAttribute('alt') || e.getAttribute('title') || e.innerText || e.textContent || '';";

    private static int Timeout(CmgNode action) =>
        action.Options.TryGetValue("timeout", out var value) && int.TryParse(value, out var timeout) ? timeout : 0;

    private static bool IsStateMode(string mode) =>
        mode is "visible" or "hidden" or "enabled" or "disabled" or "attached" or "detached" or
        "editable" or "empty" or "focused" or "inviewport";

    private static CmgNode NormalizeLocatorArgument(CmgNode action)
    {
        var locator = action.Options.FirstOrDefault(pair => IsLocatorOption(pair.Key));
        if (string.IsNullOrWhiteSpace(locator.Key))
        {
            return action;
        }

        return action with { Arguments = [$"{locator.Key}={locator.Value}", .. action.Arguments] };
    }

    private static bool IsLocatorOption(string key) =>
        key is "css" or "testid" or "testId" or "data-testid" or "text" or "textExact" or "textRegex" or
            "role" or "roleRegex" or "label" or "labelExact" or "labelRegex" or "placeholder" or "placeholderExact" or
            "placeholderRegex" or "alt" or "altExact" or "altRegex" or "title" or "titleExact" or "titleRegex" or "xpath" or
            "first" or "last" or "nth" or "has" or "hasNot" or "hasText" or "hasNotText" or "visible" or
            "or" or "and" or "strict" or "inside" or "closest" or "parent" or "next" or "previous" or
            "shadow" or "shadowText";

    private static string Fail(string message) =>
        $"(() => {{ throw new Error({QuoteJs(message)}); }})()";

    private static string Line(string expression) => $"evaluate {Quote(expression)}";

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal)}\"";

    private static string QuoteJs(string value) => Quote(value);
}

public sealed record CmgExpectationPlan(
    IReadOnlyList<string> PrefixExpressions,
    string Expression,
    string? Error)
{
    public static CmgExpectationPlan Fail(string error) => new([], string.Empty, error);
}
