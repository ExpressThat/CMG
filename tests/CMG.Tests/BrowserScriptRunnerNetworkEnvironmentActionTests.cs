using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerNetworkEnvironmentActionTests
{
    [Fact]
    public void RunText_SetExtraHttpHeadersInstallsCurrentAndFuturePatch()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("setExtraHTTPHeaders \"X-CMG\" \"yes\" \"Accept\" \"application/json\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgExtraHeaders", client.LastExpression);
        Assert.Contains("__cmgExtraHeaders", client.LastInitScript);
        Assert.Contains("HEADERS_SET 001 2", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetExtraHttpHeadersRequiresHeaderPairs()
    {
        var result = Runner().RunText("setExtraHTTPHeaders", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("header pairs", result.Error);
    }

    [Fact]
    public void RunText_ClearExtraHttpHeadersClearsPatch()
    {
        var result = Runner().RunText("clearExtraHTTPHeaders", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("HEADERS_CLEARED 001", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetHttpCredentialsInstallsAuthorizationPatch()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("setHttpCredentials user secret", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("Authorization", client.LastExpression);
        Assert.Contains("Basic dXNlcjpzZWNyZXQ=", client.LastExpression);
        Assert.Contains("HTTP_CREDENTIALS_SET 001 user", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetHttpCredentialsValidatesUsername()
    {
        var result = Runner().RunText("setHttpCredentials \" \" secret", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("username cannot be empty", result.Error);
    }

    [Fact]
    public void RunText_ClearHttpCredentialsRemovesAuthorizationHeader()
    {
        var result = Runner().RunText("clearHttpCredentials", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("HTTP_CREDENTIALS_CLEARED 001", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetProxyInstallsFetchAndXhrRewrite()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("setProxy \"https://proxy.local/?url=\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgProxyPrefix", client.LastExpression);
        Assert.Contains("__cmgProxyPrefix", client.LastInitScript);
        Assert.Contains("PROXY_SET 001 https://proxy.local/?url=", result.StdoutLines);
    }

    [Fact]
    public void RunText_ClearProxyClearsRewrite()
    {
        var result = Runner().RunText("clearProxy", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("PROXY_CLEARED 001", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetOfflineValidatesBoolean()
    {
        var result = Runner().RunText("setOffline maybe", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("expects true or false", result.Error);
    }

    [Fact]
    public void RunText_SetOfflineInstallsPatch()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("setOffline true", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgOffline = true", client.LastExpression);
        Assert.Contains("OFFLINE 001 true", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
