namespace CMG.Runner;

public static class CmgLocator
{
    public static CmgResolvedLocator Resolve(string locator, int lineNumber)
    {
        if (locator.StartsWith("css=", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator(locator[4..], []);
        }

        if (locator.StartsWith("testid=", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator($"[data-testid=\"{EscapeCss(locator[7..])}\"]", []);
        }

        if (locator.StartsWith("placeholder=", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator($"[placeholder=\"{EscapeCss(locator[12..])}\"]", []);
        }

        if (locator.StartsWith("alt=", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator($"[alt=\"{EscapeCss(locator[4..])}\"]", []);
        }

        if (locator.StartsWith("title=", StringComparison.OrdinalIgnoreCase))
        {
            return new CmgResolvedLocator($"[title=\"{EscapeCss(locator[6..])}\"]", []);
        }

        if (locator.Contains('=', StringComparison.Ordinal))
        {
            var marker = $"__cmg_locator_{lineNumber}";
            return new CmgResolvedLocator($"[data-cmg-locator-id=\"{marker}\"]", [BuildMarkerScript(locator, marker)]);
        }

        return new CmgResolvedLocator(locator, []);
    }

    public static string ToCssSelector(string locator) => Resolve(locator, lineNumber: 0).Selector;

    public static bool IsSupported(string locator) =>
        !locator.Contains('=', StringComparison.Ordinal) ||
        locator.StartsWith("css=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("testid=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("text=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("role=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("label=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("placeholder=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("alt=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("title=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("xpath=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("first=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("last=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("nth=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("has=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("hasNot=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("hasText=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("hasNotText=", StringComparison.OrdinalIgnoreCase) ||
        locator.StartsWith("visible=", StringComparison.OrdinalIgnoreCase);

    public static string UnsupportedReason(string locator) => $"Locator '{locator}' is not supported.";

    public static IReadOnlyList<string> PrefixExpressions(string locator, int lineNumber)
    {
        if (!locator.Contains('=', StringComparison.Ordinal) ||
            locator.StartsWith("css=", StringComparison.OrdinalIgnoreCase) ||
            locator.StartsWith("testid=", StringComparison.OrdinalIgnoreCase) ||
            locator.StartsWith("placeholder=", StringComparison.OrdinalIgnoreCase) ||
            locator.StartsWith("alt=", StringComparison.OrdinalIgnoreCase) ||
            locator.StartsWith("title=", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        return [BuildMarkerExpression(locator, $"__cmg_locator_{lineNumber}")];
    }

    private static string BuildMarkerScript(string locator, string marker)
    {
        return $"evaluate \"{EscapeScript(BuildMarkerExpression(locator, marker))}\"";
    }

    private static string BuildMarkerExpression(string locator, string marker)
    {
        var kind = locator[..locator.IndexOf('=')].ToLowerInvariant();
        var value = locator[(locator.IndexOf('=') + 1)..];
        const string helpers = "const implicitRole = e => e.tagName === 'BUTTON' ? 'button' : e.tagName === 'A' && e.hasAttribute('href') ? 'link' : e.tagName === 'INPUT' || e.tagName === 'TEXTAREA' ? 'textbox' : ''; const IsVisible = e => { const r = e.getBoundingClientRect(); const s = getComputedStyle(e); return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none'; };";
        return $"(() => {{ {helpers} const element = {BuildElementExpression(kind, value)}; if (!element) throw new Error('No element matched locator {locator}'); element.setAttribute('data-cmg-locator-id', '{marker}'); return true; }})()";
    }

    private static string BuildElementExpression(string kind, string value) =>
        kind switch
        {
            "text" => $"Array.from(document.querySelectorAll('body *')).filter(e => (e.innerText || e.textContent || '').includes({QuoteJs(value)})).sort((a, b) => a.querySelectorAll('*').length - b.querySelectorAll('*').length || (a.innerText || a.textContent || '').length - (b.innerText || b.textContent || '').length)[0]",
            "role" => $"Array.from(document.querySelectorAll('body *')).find(e => ((e.getAttribute('role') || implicitRole(e)) === {QuoteJs(value)}))",
            "label" => $"Array.from(document.querySelectorAll('label')).find(l => (l.innerText || '').includes({QuoteJs(value)}))?.control",
            "xpath" => $"document.evaluate({QuoteJs(value)}, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue",
            "first" => $"document.querySelector({QuoteJs(value)})",
            "last" => $"Array.from(document.querySelectorAll({QuoteJs(value)})).at(-1)",
            "nth" => BuildNthExpression(value),
            "has" => BuildHasExpression(value, expected: true),
            "hasnot" => BuildHasExpression(value, expected: false),
            "hastext" => BuildHasTextExpression(value),
            "hasnottext" => BuildHasNotTextExpression(value),
            "visible" => $"Array.from(document.querySelectorAll({QuoteJs(value)})).find(IsVisible)",
            _ => "null"
        };

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
