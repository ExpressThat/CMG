namespace CMG.Browser.Scripting;

public static class BrowserStorageScripts
{
    public static string WebStorage(string storage, string operation, string key, string value) =>
        operation switch
        {
            "get" => $"{storage}.getItem({Quote(key)}) ?? ''",
            "set" => $"(() => {{ {storage}.setItem({Quote(key)}, {Quote(value)}); return true; }})()",
            "remove" => $"(() => {{ {storage}.removeItem({Quote(key)}); return true; }})()",
            "clear" => $"(() => {{ {storage}.clear(); return true; }})()",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    public static string Cookie(
        string operation,
        string key,
        string value,
        IReadOnlyDictionary<string, string>? options = null) =>
        operation switch
        {
            "get" when key.Length is 0 => "document.cookie",
            "get" => $$"""
                (() => {
                  const match = document.cookie.split(';').map(x => x.trim()).find(x => x.startsWith({{Quote(key + "=")}}));
                  return match ? match.slice({{key.Length + 1}}) : '';
                })()
                """,
            "set" => $"(() => {{ document.cookie = {Quote($"{key}={value}{CookieAttributes(options, includeExpiry: true)}")}; return true; }})()",
            "remove" => $"(() => {{ document.cookie = {Quote($"{key}=; expires=Thu, 01 Jan 1970 00:00:00 GMT{CookieAttributes(options, includeExpiry: false)}")}; return true; }})()",
            "clear" => $$"""
                (() => {
                  for (const cookie of document.cookie.split(';')) {
                    const name = cookie.split('=')[0]?.trim();
                    if (name) document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT{{CookieAttributes(options, includeExpiry: false)}}`;
                  }
                  return true;
                })()
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    private static string CookieAttributes(IReadOnlyDictionary<string, string>? options, bool includeExpiry)
    {
        var attributes = new List<string> { $"path={ValueOrDefault(options, "path", "/")}" };
        AddOption(attributes, options, "domain");
        if (includeExpiry)
        {
            AddOption(attributes, options, "expires");
            AddOption(attributes, options, "maxAge", "Max-Age");
            AddOption(attributes, options, "sameSite", "SameSite");
            if (bool.TryParse(ValueOrDefault(options, "secure", "false"), out var secure) && secure)
            {
                attributes.Add("Secure");
            }
        }

        return "; " + string.Join("; ", attributes);
    }

    private static void AddOption(
        List<string> attributes,
        IReadOnlyDictionary<string, string>? options,
        string key,
        string? attribute = null)
    {
        if (options?.TryGetValue(key, out var value) is true && value.Length > 0)
        {
            attributes.Add($"{attribute ?? key}={value}");
        }
    }

    private static string ValueOrDefault(IReadOnlyDictionary<string, string>? options, string key, string defaultValue) =>
        options?.TryGetValue(key, out var value) is true && value.Length > 0 ? value : defaultValue;

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
