namespace CMG.Runner;

public static class CmgLocatorKeys
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["testId"] = "testid",
        ["data-testid"] = "testid",
        ["getByText"] = "text",
        ["getByTextExact"] = "textExact",
        ["getByExactText"] = "textExact",
        ["getByTextRegex"] = "textRegex",
        ["getByRole"] = "role",
        ["getByRoleRegex"] = "roleRegex",
        ["getByLabel"] = "label",
        ["getByLabelText"] = "label",
        ["getByLabelExact"] = "labelExact",
        ["getByLabelTextExact"] = "labelExact",
        ["getByLabelRegex"] = "labelRegex",
        ["getByLabelTextRegex"] = "labelRegex",
        ["getByPlaceholder"] = "placeholder",
        ["getByPlaceholderText"] = "placeholder",
        ["getByPlaceholderExact"] = "placeholderExact",
        ["getByPlaceholderTextExact"] = "placeholderExact",
        ["getByPlaceholderRegex"] = "placeholderRegex",
        ["getByPlaceholderTextRegex"] = "placeholderRegex",
        ["getByAltText"] = "alt",
        ["getByAltTextExact"] = "altExact",
        ["getByAltTextRegex"] = "altRegex",
        ["getByTitle"] = "title",
        ["getByTitleExact"] = "titleExact",
        ["getByTitleRegex"] = "titleRegex",
        ["getByTestId"] = "testid"
    };

    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        "css", "testid", "text", "textExact", "textRegex", "role", "roleRegex",
        "label", "labelExact", "labelRegex", "placeholder", "placeholderExact",
        "placeholderRegex", "alt", "altExact", "altRegex", "title", "titleExact",
        "titleRegex", "xpath", "first", "last", "nth", "has", "hasNot", "hasText",
        "hasNotText", "visible", "or", "and", "strict", "inside", "closest",
        "parent", "next", "previous", "shadow", "shadowText"
    };

    public static bool TryParse(string locator, out string key, out string value)
    {
        key = string.Empty;
        value = string.Empty;
        var index = locator.IndexOf('=');
        if (index <= 0)
        {
            return false;
        }

        key = Normalize(locator[..index]);
        value = locator[(index + 1)..];
        return Supported.Contains(key);
    }

    public static bool IsLocatorOption(string key) => Supported.Contains(Normalize(key));

    public static string Normalize(string key) => Aliases.TryGetValue(key, out var alias) ? alias : key;

    public static bool IsSimpleSelectorKey(string key) =>
        Normalize(key) is "css" or "testid" or "placeholder" or "alt" or "title";

    public static string Format(string key, string value) => $"{Normalize(key)}={value}";
}
