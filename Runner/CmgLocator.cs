namespace CMG.Runner;

public static class CmgLocator
{
    public static string ToCssSelector(string locator)
    {
        if (locator.StartsWith("css=", StringComparison.OrdinalIgnoreCase))
        {
            return locator[4..];
        }

        if (locator.StartsWith("testid=", StringComparison.OrdinalIgnoreCase))
        {
            return $"[data-testid='{EscapeCss(locator[7..])}']";
        }

        return locator;
    }

    public static bool IsSupported(string locator) =>
        !locator.StartsWith("text=", StringComparison.OrdinalIgnoreCase) &&
        !locator.StartsWith("role=", StringComparison.OrdinalIgnoreCase) &&
        !locator.StartsWith("label=", StringComparison.OrdinalIgnoreCase) &&
        !locator.StartsWith("placeholder=", StringComparison.OrdinalIgnoreCase) &&
        !locator.StartsWith("alt=", StringComparison.OrdinalIgnoreCase) &&
        !locator.StartsWith("title=", StringComparison.OrdinalIgnoreCase) &&
        !locator.StartsWith("xpath=", StringComparison.OrdinalIgnoreCase);

    public static string UnsupportedReason(string locator) =>
        $"Locator '{locator}' is planned for full parity but is not implemented in this slice.";

    private static string EscapeCss(string value) => value.Replace("'", "\\'", StringComparison.Ordinal);
}
