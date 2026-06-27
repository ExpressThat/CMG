using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserStorageScriptsTests
{
    [Fact]
    public void WebStorageBuildsSetAndGetExpressions()
    {
        Assert.Contains("localStorage.setItem", BrowserStorageScripts.WebStorage("localStorage", "set", "token", "abc"));
        Assert.Contains("localStorage.getItem", BrowserStorageScripts.WebStorage("localStorage", "get", "token", string.Empty));
    }

    [Fact]
    public void CookieBuildsNamedGetSetAndClearExpressions()
    {
        Assert.Contains("document.cookie", BrowserStorageScripts.Cookie("set", "mode", "demo"));
        Assert.Contains("startsWith", BrowserStorageScripts.Cookie("get", "mode", string.Empty));
        Assert.Contains("expires=Thu, 01 Jan 1970", BrowserStorageScripts.Cookie("clear", string.Empty, string.Empty));
    }

    [Fact]
    public void CookieBuildsAttributeExpression()
    {
        var expression = BrowserStorageScripts.Cookie(
            "set",
            "mode",
            "demo",
            new Dictionary<string, string>
            {
                ["path"] = "/app",
                ["domain"] = "example.test",
                ["maxAge"] = "60",
                ["sameSite"] = "Strict",
                ["secure"] = "true"
            });

        Assert.Contains("path=/app", expression);
        Assert.Contains("domain=example.test", expression);
        Assert.Contains("Max-Age=60", expression);
        Assert.Contains("SameSite=Strict", expression);
        Assert.Contains("Secure", expression);
    }
}
