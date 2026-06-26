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
}
