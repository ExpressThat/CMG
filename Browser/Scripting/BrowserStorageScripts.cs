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

    public static string Cookie(string operation, string key, string value) =>
        operation switch
        {
            "get" when key.Length is 0 => "document.cookie",
            "get" => $$"""
                (() => {
                  const match = document.cookie.split(';').map(x => x.trim()).find(x => x.startsWith({{Quote(key + "=")}}));
                  return match ? match.slice({{key.Length + 1}}) : '';
                })()
                """,
            "set" => $"(() => {{ document.cookie = {Quote($"{key}={value}; path=/")}; return true; }})()",
            "remove" => $"(() => {{ document.cookie = {Quote($"{key}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/")}; return true; }})()",
            "clear" => """
                (() => {
                  for (const cookie of document.cookie.split(';')) {
                    const name = cookie.split('=')[0]?.trim();
                    if (name) document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
                  }
                  return true;
                })()
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
