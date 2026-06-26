namespace CMG.Browser;

public enum BrowserAppKind
{
    Electron,
    WebView2
}

public sealed record BrowserAppDebugOptions(
    int Port,
    string Host,
    int ConnectTimeoutMilliseconds);

public static class BrowserAppKindParser
{
    public static bool TryParse(string? value, out BrowserAppKind kind)
    {
        kind = BrowserAppKind.Electron;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "electron" => Parsed(BrowserAppKind.Electron, out kind),
            "webview2" or "web-view2" or "tauri" or "infiniframe" => Parsed(BrowserAppKind.WebView2, out kind),
            _ => false
        };
    }

    private static bool Parsed(BrowserAppKind value, out BrowserAppKind kind)
    {
        kind = value;
        return true;
    }
}
