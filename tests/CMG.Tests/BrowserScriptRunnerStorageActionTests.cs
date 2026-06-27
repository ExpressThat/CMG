using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerStorageActionTests
{
    [Fact]
    public void RunText_LocalStorageSetUsesBrowserStorageScript()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("localStorage set \"token\" \"abc\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("localStorage.setItem", client.LastExpression);
        Assert.Contains("LOCAL_STORAGE 001 set token", result.StdoutLines);
    }

    [Fact]
    public void RunText_SessionStorageGetReturnsOutput()
    {
        var result = Runner().RunText("sessionStorage get \"token\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("SESSION_STORAGE 001 get token {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_CookieClearReturnsOutput()
    {
        var result = Runner().RunText("cookie clear", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("COOKIE 001 clear", result.StdoutLines);
    }

    [Fact]
    public void RunText_CookieSetPassesCookieOptions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("cookie set \"mode\" \"demo\" path=\"/app\" sameSite=\"Lax\" secure=\"true\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("path=/app", client.LastExpression);
        Assert.Contains("SameSite=Lax", client.LastExpression);
        Assert.Contains("Secure", client.LastExpression);
    }

    [Fact]
    public void RunText_CookieRejectsUnsupportedOptionForGet()
    {
        var result = Runner().RunText("cookie get \"mode\" sameSite=\"Lax\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("cookie get does not support option 'sameSite'", result.Error);
    }

    [Fact]
    public void RunText_CookieRejectsInvalidSameSite()
    {
        var result = Runner().RunText("cookie set \"mode\" \"demo\" sameSite=\"Maybe\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("cookie sameSite expects Strict, Lax, or None", result.Error);
    }

    [Fact]
    public void RunText_LocalStorageSetValidatesValue()
    {
        var result = Runner().RunText("localStorage set \"token\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("expects a key and value", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
