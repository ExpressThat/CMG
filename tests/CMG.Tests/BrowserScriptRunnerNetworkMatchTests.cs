using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerNetworkMatchTests
{
    [Fact]
    public void RunText_WaitForResponseSupportsRegexAndIgnoreCase()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/API/Profile/42","status":200}}""");

        var result = Runner().RunText("waitForResponse \"/api/profile/\\d+\" match=regex ignoreCase=true", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("match: 'regex'", client.LastExpression);
        Assert.Contains("ignoreCase: true", client.LastExpression);
        Assert.Contains("new RegExp", client.LastExpression);
    }

    [Fact]
    public void RunText_WaitForRequestSupportsExactMatch()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/api/profile","method":"GET"}}""");

        var result = Runner().RunText("waitForRequest \"/api/profile\" match=exact", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("match: 'exact'", client.LastExpression);
        Assert.Contains("normalizeUrl(url) === expectedPattern", client.LastExpression);
    }

    [Fact]
    public void RunText_WaitForRequestRejectsInvalidMatchMode()
    {
        var result = Runner().RunText("waitForRequest \"/api\" match=fuzzy", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("match= must be contains, exact, or regex", result.Error);
    }

    [Fact]
    public void RunText_WaitForResponseRejectsInvalidRegex()
    {
        var result = Runner().RunText("waitForResponse \"[\" match=regex", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Invalid network regex '['", result.Error);
    }

    [Fact]
    public void RunText_WaitForEventResponsePassesNetworkMatchOptions()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/API/Profile","status":200}}""");

        var result = Runner().RunText("waitForEvent response \"/api/profile\" match=exact ignoreCase=true", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("match: 'exact'", client.LastExpression);
        Assert.Contains("ignoreCase: true", client.LastExpression);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
