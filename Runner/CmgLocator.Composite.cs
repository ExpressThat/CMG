namespace CMG.Runner;

public static partial class CmgLocator
{
    private static string BuildOrExpression(string value)
    {
        return SplitLocatorValue(value) is { } parts
            ? $"document.querySelector({QuoteJs(parts.Left)}) ?? document.querySelector({QuoteJs(parts.Right)})"
            : "(() => { throw new Error('Locator or= requires <selector>|<selector>.'); })()";
    }

    private static string BuildAndExpression(string value)
    {
        return SplitLocatorValue(value) is { } parts
            ? $"Array.from(document.querySelectorAll({QuoteJs(parts.Left)})).find(e => e.matches({QuoteJs(parts.Right)}))"
            : "(() => { throw new Error('Locator and= requires <selector>|<selector>.'); })()";
    }

    private static string BuildStrictExpression(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "(() => { throw new Error('Locator strict= requires <selector>.'); })()";
        }

        return $"(() => {{ const matches = Array.from(document.querySelectorAll({QuoteJs(value)})); if (matches.length !== 1) throw new Error('Locator strict= expected exactly one match for {value}, got ' + matches.length + '.'); return matches[0]; }})()";
    }

    private static string BuildInsideExpression(string value)
    {
        return SplitLocatorValue(value) is { } parts
            ? $"document.querySelector({QuoteJs(parts.Left)})?.querySelector({QuoteJs(parts.Right)})"
            : "(() => { throw new Error('Locator inside= requires <container-selector>|<target-selector>.'); })()";
    }

    private static string BuildClosestExpression(string value)
    {
        return SplitLocatorValue(value) is { } parts
            ? $"document.querySelector({QuoteJs(parts.Left)})?.closest({QuoteJs(parts.Right)})"
            : "(() => { throw new Error('Locator closest= requires <child-selector>|<ancestor-selector>.'); })()";
    }

    private static string BuildParentExpression(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "(() => { throw new Error('Locator parent= requires <child-selector>.'); })()";
        }

        return SplitLocatorValue(value) is { } parts
            ? $"((e) => e?.parentElement?.matches({QuoteJs(parts.Right)}) ? e.parentElement : null)(document.querySelector({QuoteJs(parts.Left)}))"
            : $"document.querySelector({QuoteJs(value)})?.parentElement";
    }

    private static string BuildSiblingExpression(string value, bool next)
    {
        var name = next ? "next" : "previous";
        var property = next ? "nextElementSibling" : "previousElementSibling";
        if (string.IsNullOrWhiteSpace(value))
        {
            return $"(() => {{ throw new Error('Locator {name}= requires <selector>.'); }})()";
        }

        return SplitLocatorValue(value) is { } parts
            ? $"((e) => {{ for (let n = e?.{property}; n; n = n.{property}) {{ if (n.matches({QuoteJs(parts.Right)})) return n; }} return null; }})(document.querySelector({QuoteJs(parts.Left)}))"
            : $"document.querySelector({QuoteJs(value)})?.{property}";
    }
}
