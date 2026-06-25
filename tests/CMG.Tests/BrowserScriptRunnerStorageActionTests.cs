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
    public void RunText_LocalStorageSetValidatesValue()
    {
        var result = Runner().RunText("localStorage set \"token\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("expects a key and value", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
