namespace CMG.Runner;

public static partial class CmgLocator
{
    public static CmgResolvedLocator Resolve(string locator, int lineNumber)
    {
        if (CmgLocatorKeys.TryParse(locator, out var key, out var value) && key.Equals("css", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator(value, []);
        }

        if (key.Equals("testid", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator($"[data-testid=\"{EscapeCss(value)}\"]", []);
        }

        if (key.Equals("placeholder", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator($"[placeholder=\"{EscapeCss(value)}\"]", []);
        }

        if (key.Equals("alt", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator($"[alt=\"{EscapeCss(value)}\"]", []);
        }

        if (key.Equals("title", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator($"[title=\"{EscapeCss(value)}\"]", []);
        }

        if (locator.Contains('=', StringComparison.Ordinal))
        {
            var marker = $"__cmg_locator_{lineNumber}";
            var normalized = CmgLocatorKeys.TryParse(locator, out key, out value) ? CmgLocatorKeys.Format(key, value) : locator;
            return new CmgResolvedLocator($"[data-cmg-locator-id=\"{marker}\"]", [BuildMarkerScript(normalized, marker)]);
        }

        return new CmgResolvedLocator(locator, []);
    }

    public static string ToCssSelector(string locator) => Resolve(locator, lineNumber: 0).Selector;

    public static bool IsSupported(string locator) =>
        !locator.Contains('=', StringComparison.Ordinal) || CmgLocatorKeys.TryParse(locator, out _, out _);

    public static string UnsupportedReason(string locator) => $"Locator '{locator}' is not supported.";

    public static IReadOnlyList<string> PrefixExpressions(string locator, int lineNumber)
    {
        if (!CmgLocatorKeys.TryParse(locator, out var key, out var value) || CmgLocatorKeys.IsSimpleSelectorKey(key))
        {
            return [];
        }

        return [BuildMarkerExpression(CmgLocatorKeys.Format(key, value), $"__cmg_locator_{lineNumber}")];
    }

    private static string BuildMarkerScript(string locator, string marker)
    {
        return $"evaluate \"{EscapeScript(BuildMarkerExpression(locator, marker))}\"";
    }

    private static string BuildMarkerExpression(string locator, string marker)
    {
        var kind = CmgLocatorKeys.Normalize(locator[..locator.IndexOf('=')]).ToLowerInvariant();
        var value = locator[(locator.IndexOf('=') + 1)..];
        const string helpers = "const implicitRole = e => e.tagName === 'BUTTON' ? 'button' : e.tagName === 'A' && e.hasAttribute('href') ? 'link' : e.tagName === 'INPUT' || e.tagName === 'TEXTAREA' ? 'textbox' : ''; const accessibleName = e => e.getAttribute('aria-label') || e.getAttribute('alt') || e.getAttribute('title') || e.innerText || e.textContent || ''; const IsVisible = e => { const r = e.getBoundingClientRect(); const s = getComputedStyle(e); return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none'; };";
        return $"(() => {{ {InstallQueryRegistry()} {helpers} const element = {BuildElementExpression(kind, value)}; if (!element) throw new Error('No element matched locator {locator}'); window.__cmgLocatorElements['{marker}'] = element; element.setAttribute?.('data-cmg-locator-id', '{marker}'); return true; }})()";
    }

    private static string BuildElementExpression(string kind, string value) =>
        kind switch
        {
            "text" => $"Array.from(document.querySelectorAll('body *')).filter(e => (e.innerText || e.textContent || '').includes({QuoteJs(value)})).sort((a, b) => a.querySelectorAll('*').length - b.querySelectorAll('*').length || (a.innerText || a.textContent || '').length - (b.innerText || b.textContent || '').length)[0]",
            "textexact" => $"Array.from(document.querySelectorAll('body *')).filter(e => ((e.innerText || e.textContent || '').trim() === {QuoteJs(value)})).sort((a, b) => a.querySelectorAll('*').length - b.querySelectorAll('*').length || (a.innerText || a.textContent || '').length - (b.innerText || b.textContent || '').length)[0]",
            "textregex" => $"Array.from(document.querySelectorAll('body *')).filter(e => new RegExp({QuoteJs(value)}).test(e.innerText || e.textContent || '')).sort((a, b) => a.querySelectorAll('*').length - b.querySelectorAll('*').length || (a.innerText || a.textContent || '').length - (b.innerText || b.textContent || '').length)[0]",
            "role" => BuildRoleExpression(value),
            "roleregex" => BuildRoleRegexExpression(value),
            "label" => $"Array.from(document.querySelectorAll('label')).find(l => (l.innerText || '').includes({QuoteJs(value)}))?.control",
            "labelexact" => $"Array.from(document.querySelectorAll('label')).find(l => (l.innerText || '').trim() === {QuoteJs(value)})?.control",
            "labelregex" => $"Array.from(document.querySelectorAll('label')).find(l => new RegExp({QuoteJs(value)}).test(l.innerText || ''))?.control",
            "placeholderexact" => $"Array.from(document.querySelectorAll('[placeholder]')).find(e => e.getAttribute('placeholder') === {QuoteJs(value)})",
            "placeholderregex" => $"Array.from(document.querySelectorAll('[placeholder]')).find(e => new RegExp({QuoteJs(value)}).test(e.getAttribute('placeholder') || ''))",
            "altexact" => $"Array.from(document.querySelectorAll('[alt]')).find(e => e.getAttribute('alt') === {QuoteJs(value)})",
            "altregex" => $"Array.from(document.querySelectorAll('[alt]')).find(e => new RegExp({QuoteJs(value)}).test(e.getAttribute('alt') || ''))",
            "titleexact" => $"Array.from(document.querySelectorAll('[title]')).find(e => e.getAttribute('title') === {QuoteJs(value)})",
            "titleregex" => $"Array.from(document.querySelectorAll('[title]')).find(e => new RegExp({QuoteJs(value)}).test(e.getAttribute('title') || ''))",
            "xpath" => $"document.evaluate({QuoteJs(value)}, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue",
            "first" => $"document.querySelector({QuoteJs(value)})",
            "last" => $"Array.from(document.querySelectorAll({QuoteJs(value)})).at(-1)",
            "nth" => BuildNthExpression(value),
            "has" => BuildHasExpression(value, expected: true),
            "hasnot" => BuildHasExpression(value, expected: false),
            "hastext" => BuildHasTextExpression(value),
            "hasnottext" => BuildHasNotTextExpression(value),
            "visible" => $"Array.from(document.querySelectorAll({QuoteJs(value)})).find(IsVisible)",
            "or" => BuildOrExpression(value),
            "and" => BuildAndExpression(value),
            "strict" => BuildStrictExpression(value),
            "inside" => BuildInsideExpression(value),
            "closest" => BuildClosestExpression(value),
            "parent" => BuildParentExpression(value),
            "next" => BuildSiblingExpression(value, next: true),
            "previous" => BuildSiblingExpression(value, next: false),
            "shadow" => BuildShadowExpression(value),
            "shadowtext" => BuildShadowTextExpression(value),
            _ => "null"
        };

    private static string BuildRoleExpression(string value)
    {
        if (SplitLocatorValue(value) is not { } parts)
        {
            return $"Array.from(document.querySelectorAll('body *')).find(e => ((e.getAttribute('role') || implicitRole(e)) === {QuoteJs(value)}))";
        }

        return $"Array.from(document.querySelectorAll('body *')).find(e => ((e.getAttribute('role') || implicitRole(e)) === {QuoteJs(parts.Left)}) && accessibleName(e).includes({QuoteJs(parts.Right)}))";
    }

    private static string BuildRoleRegexExpression(string value) =>
        SplitLocatorValue(value) is { } parts
            ? $"Array.from(document.querySelectorAll('body *')).find(e => ((e.getAttribute('role') || implicitRole(e)) === {QuoteJs(parts.Left)}) && new RegExp({QuoteJs(parts.Right)}).test(accessibleName(e)))"
            : "(() => { throw new Error('Locator roleRegex= requires <role>|<name-regex>.'); })()";

    private static string BuildNthExpression(string value)
    {
        return SplitLocatorValue(value) is { } parts
            ? $"Array.from(document.querySelectorAll({QuoteJs(parts.Left)}))[Number({QuoteJs(parts.Right)})]"
            : "(() => { throw new Error('Locator nth= requires <selector>|<index>.'); })()";
    }

    private static string BuildHasTextExpression(string value)
    {
        return SplitLocatorValue(value) is { } parts
            ? $"Array.from(document.querySelectorAll({QuoteJs(parts.Left)})).find(e => (e.innerText || e.textContent || '').includes({QuoteJs(parts.Right)}))"
            : "(() => { throw new Error('Locator hasText= requires <selector>|<text>.'); })()";
    }

    private static string BuildHasNotTextExpression(string value)
    {
        return SplitLocatorValue(value) is { } parts
            ? $"Array.from(document.querySelectorAll({QuoteJs(parts.Left)})).find(e => !(e.innerText || e.textContent || '').includes({QuoteJs(parts.Right)}))"
            : "(() => { throw new Error('Locator hasNotText= requires <selector>|<text>.'); })()";
    }

    private static string BuildHasExpression(string value, bool expected)
    {
        var name = expected ? "has" : "hasNot";
        var comparison = expected ? string.Empty : "!";
        return SplitLocatorValue(value) is { } parts
            ? $"Array.from(document.querySelectorAll({QuoteJs(parts.Left)})).find(e => {comparison}e.querySelector({QuoteJs(parts.Right)}))"
            : $"(() => {{ throw new Error('Locator {name}= requires <selector>|<child-selector>.'); }})()";
    }

    private static string BuildShadowExpression(string value)
    {
        return SplitLocatorValue(value) is { } parts
            ? $"document.querySelector({QuoteJs(parts.Left)})?.shadowRoot?.querySelector({QuoteJs(parts.Right)})"
            : "(() => { throw new Error('Locator shadow= requires <host-selector>|<inner-selector>.'); })()";
    }

    private static string BuildShadowTextExpression(string value)
    {
        return SplitLocatorValue(value) is { } parts
            ? $"Array.from(document.querySelector({QuoteJs(parts.Left)})?.shadowRoot?.querySelectorAll('*') || []).find(e => (e.innerText || e.textContent || '').includes({QuoteJs(parts.Right)}))"
            : "(() => { throw new Error('Locator shadowText= requires <host-selector>|<text>.'); })()";
    }

    private static string InstallQueryRegistry() =>
        "window.__cmgLocatorElements ||= {}; window.__cmgQuery = selector => { const id = selector.match(/^\\[data-cmg-locator-id=\"([^\"]+)\"\\]$/)?.[1]; return id && window.__cmgLocatorElements[id]?.isConnected ? window.__cmgLocatorElements[id] : document.querySelector(selector); }; window.__cmgQueryAll = selector => { const hit = window.__cmgQuery(selector); const id = selector.match(/^\\[data-cmg-locator-id=\"([^\"]+)\"\\]$/)?.[1]; return id ? (hit ? [hit] : []) : Array.from(document.querySelectorAll(selector)); };";

    private static (string Left, string Right)? SplitLocatorValue(string value)
    {
        var index = value.LastIndexOf('|');
        if (index <= 0 || index == value.Length - 1)
        {
            return null;
        }

        return (value[..index], value[(index + 1)..]);
    }

    private static string QuoteJs(string value) => $"'{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal)}'";

    private static string EscapeCss(string value) => value.Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string EscapeScript(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}

public sealed record CmgResolvedLocator(string Selector, IReadOnlyList<string> PrefixLines);
