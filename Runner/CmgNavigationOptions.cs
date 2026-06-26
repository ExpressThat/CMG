namespace CMG.Runner;

internal static class CmgNavigationOptions
{
    public static string? BaseUrl(CmgTestCase test, CmgRunOptions options)
    {
        if (test.Options.TryGetValue("baseUrl", out var baseUrl) && !string.IsNullOrWhiteSpace(baseUrl))
        {
            return baseUrl;
        }

        if (test.Options.TryGetValue("baseURL", out var baseUrlAlias) && !string.IsNullOrWhiteSpace(baseUrlAlias))
        {
            return baseUrlAlias;
        }

        return options.BaseUrl;
    }
}
