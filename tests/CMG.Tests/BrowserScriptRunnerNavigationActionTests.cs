using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerNavigationActionTests
{
    [Fact]
    public void RunText_ReloadNavigatesCurrentUrl()
    {
        var result = Runner().RunText("reload", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("RELOADED 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_ReloadCanWaitForProviderLoadState()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("reload waitUntil=domcontentloaded timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("interactive", client.LastExpression);
        Assert.Contains("RELOADED 001 {} waitUntil=domcontentloaded state={}", result.StdoutLines);
    }

    [Fact]
    public void RunText_ReloadValidatesWaitUntil()
    {
        var result = Runner().RunText("reload waitUntil=paint", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("load, domcontentloaded, networkidle, or commit", result.Error);
    }

    [Fact]
    public void RunText_GoBackUsesHistoryScript()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("goBack timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("history.back()", client.LastExpression);
        Assert.Contains("BACK 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForUrlReturnsMatchedUrl()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("https://example.test/checkout");
        var result = Runner().RunText("waitForUrl \"/checkout\" timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("location.href", client.LastExpression);
        Assert.Contains("URL 001 https://example.test/checkout", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForUrlReportsTimeoutReason()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("https://example.test/cart");
        var result = Runner().RunText("waitForUrl \"/checkout\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("URL did not match /checkout within 1ms using contains match", result.Error);
    }

    [Fact]
    public void RunText_WaitForUrlSupportsRegexIgnoreCaseMatch()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("https://example.test/Checkout/42");
        var result = Runner().RunText("waitForUrl \"checkout/\\\\d+\" match=regex ignoreCase=true timeout=250", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("URL 001 https://example.test/Checkout/42", result.StdoutLines);
    }

    [Fact]
    public void RunText_ExpectTitleReturnsTitleOutput()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Dashboard");
        var result = Runner().RunText("expectTitle \"Dashboard\" match=exact", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("document.title", client.LastExpression);
        Assert.Contains("TITLE 001 Dashboard", result.StdoutLines);
    }

    [Fact]
    public void RunText_ExpectTitleRejectsInvalidMatchMode()
    {
        var result = Runner().RunText("expectTitle \"Dashboard\" match=fuzzy", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("match= must be contains, exact, or regex", result.Error);
    }

    [Fact]
    public void RunText_ExpectUrlReportsActualValueOnFailure()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("https://example.test/cart");
        var result = Runner().RunText("expectUrl \"/checkout\" match=exact", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Expected URL to match /checkout using exact match, got https://example.test/cart", result.Error);
    }

    [Fact]
    public void RunText_WaitForTitleReturnsTitleOutput()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Dashboard Home");
        var result = Runner().RunText("waitForTitle \"Dashboard\" timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("document.title", client.LastExpression);
        Assert.Contains("TITLE 001 Dashboard Home", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForTitleReportsTimeoutReason()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Dashboard Home");
        var result = Runner().RunText("waitForTitle \"Missing\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Title did not match Missing within 1ms using contains match", result.Error);
    }

    [Fact]
    public void RunText_WaitForLoadStateSupportsNetworkIdle()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForLoadState networkidle", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("networkidle", client.LastExpression);
        Assert.Contains("LOAD_STATE 001 {}", result.StdoutLines);
    }

    [Theory]
    [InlineData("waitForNetworkIdle")]
    [InlineData("networkIdle")]
    public void RunText_WaitForNetworkIdleUsesNetworkIdleLoadState(string action)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText($"{action} timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("networkidle", client.LastExpression);
        Assert.Contains("NETWORK_IDLE 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForNetworkIdleRejectsArguments()
    {
        var result = Runner().RunText("waitForNetworkIdle extra", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Expected 0 positional argument(s)", result.Error);
    }

    [Fact]
    public void RunText_WaitForLoadStateValidatesState()
    {
        var result = Runner().RunText("waitForLoadState paint", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("loading, interactive, complete, load, or networkidle", result.Error);
    }

    [Fact]
    public void RunText_WaitForNavigationUsesPollingScript()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForNavigation \"/checkout\" waitUntil=domcontentloaded timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("Navigation did not reach", client.LastExpression);
        Assert.Contains("domcontentloaded", client.LastExpression);
        Assert.Contains("NAVIGATION 001 {}", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForNavigationValidatesWaitUntil()
    {
        var result = Runner().RunText("waitForNavigation waitUntil=paint", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("load, domcontentloaded, networkidle, or commit", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
