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
        locator.StartsWith("xpath=", StringComparison.OrdinalIgnoreCase);

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
        const string roleHelper = "const implicitRole = e => e.tagName === 'BUTTON' ? 'button' : e.tagName === 'A' && e.hasAttribute('href') ? 'link' : e.tagName === 'INPUT' || e.tagName === 'TEXTAREA' ? 'textbox' : '';";
        return $"(() => {{ {roleHelper} const element = {BuildElementExpression(kind, value)}; if (!element) throw new Error('No element matched locator {locator}'); element.setAttribute('data-cmg-locator-id', '{marker}'); return true; }})()";
    }

    private static string BuildElementExpression(string kind, string value) =>
        kind switch
        {
            "text" => $"Array.from(document.querySelectorAll('body *')).filter(e => (e.innerText || e.textContent || '').includes({QuoteJs(value)})).sort((a, b) => a.querySelectorAll('*').length - b.querySelectorAll('*').length || (a.innerText || a.textContent || '').length - (b.innerText || b.textContent || '').length)[0]",
            "role" => $"Array.from(document.querySelectorAll('body *')).find(e => ((e.getAttribute('role') || implicitRole(e)) === {QuoteJs(value)}))",
            "label" => $"Array.from(document.querySelectorAll('label')).find(l => (l.innerText || '').includes({QuoteJs(value)}))?.control",
            "xpath" => $"document.evaluate({QuoteJs(value)}, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue",
            _ => "null"
        };

    private static string QuoteJs(string value) => $"'{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal)}'";

    private static string EscapeCss(string value) => value.Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string EscapeScript(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}

public sealed record CmgResolvedLocator(string Selector, IReadOnlyList<string> PrefixLines);
