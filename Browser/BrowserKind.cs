namespace CMG.Browser;

public enum BrowserKind
{
    Chrome,
    Firefox
}

public static class BrowserKindExtensions
{
    public static string DisplayName(this BrowserKind browserKind) =>
        browserKind is BrowserKind.Firefox ? "Firefox" : "Chrome";

    public static string StateName(this BrowserKind browserKind) =>
        browserKind is BrowserKind.Firefox ? "firefox" : "chrome";
}
