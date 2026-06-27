namespace CMG.Browser;

public enum BrowserKind
{
    InvalidSelection,
    Chrome,
    Edge,
    Firefox
}

public static class BrowserKindExtensions
{
    public static string DisplayName(this BrowserKind browserKind) =>
        browserKind switch
        {
            BrowserKind.Edge => "Edge",
            BrowserKind.Firefox => "Firefox",
            _ => "Chrome"
        };

    public static string StateName(this BrowserKind browserKind) =>
        browserKind switch
        {
            BrowserKind.Edge => "edge",
            BrowserKind.Firefox => "firefox",
            _ => "chrome"
        };

    public static string CommandOptionPrefix(this BrowserKind browserKind) =>
        browserKind switch
        {
            BrowserKind.Edge => "--edge ",
            BrowserKind.Firefox => "--firefox ",
            _ => string.Empty
        };

    public static bool UsesFirefoxBiDi(this BrowserKind browserKind) =>
        browserKind is BrowserKind.Firefox;

    public static bool UsesChromiumDevTools(this BrowserKind browserKind) =>
        browserKind is BrowserKind.Chrome or BrowserKind.Edge;

    public static int DefaultRemoteDebuggingPort(this BrowserKind browserKind) =>
        browserKind switch
        {
            BrowserKind.Edge => 9224,
            BrowserKind.Firefox => 9223,
            _ => 9222
        };
}
