using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerWaitActionTests
{
    [Fact]
    public void RunText_WaitForFunctionEvaluatesPollingScript()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":"true"}""");
        var result = Runner().RunText("waitForFunction \"window.ready === true\" timeout=250", "debug", client);

        Assert.True(result.Success, $"{result.Error} | {string.Join(" | ", result.StdoutLines)}");
        Assert.Contains("window.ready === true", client.LastExpression);
        Assert.Contains("FUNCTION 001 true", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForFunctionReportsTimeoutFailure()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":false,"error":"waitForFunction did not become truthy within 1ms."}""");

        var result = Runner().RunText("waitForFunction \"false\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("did not become truthy", result.Error);
    }

    [Fact]
    public void RunText_WaitForSelectorUsesWaitForElement()
    {
        var result = Runner().RunText("waitForSelector \"#ready\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error);
        Assert.Contains("SELECTOR 001 #ready", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForSelectorNormalizesRichLocator()
    {
        var result = Runner().RunText("waitForSelector text=Ready", "debug", new FakeAutomationClient());

        Assert.True(result.Success, $"{result.Error} | {string.Join(" | ", result.StdoutLines)}");
        Assert.Contains("SELECTOR 001 text=Ready", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForSelectorSupportsVisibleState()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"attached":true,"visible":true}""");
        var result = Runner().RunText("waitForSelector \"#ready\" state=visible timeout=250", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("SELECTOR 001 #ready state=visible", result.StdoutLines);
        Assert.Contains("getBoundingClientRect", client.LastExpression);
    }

    [Fact]
    public void RunText_WaitForSelectorSupportsDetachedState()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"attached":false,"visible":false}""");
        var result = Runner().RunText("waitForSelector \"#toast\" state=detached timeout=250", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("SELECTOR 001 #toast state=detached", result.StdoutLines);
    }

    [Fact]
    public void RunText_WaitForSelectorReportsStateTimeout()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"attached":true,"visible":false}""");
        var result = Runner().RunText("waitForSelector \"#ready\" state=visible timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("did not reach state visible within 1ms", result.Error);
        Assert.Contains("attached=true, visible=false", result.Error);
    }

    [Fact]
    public void RunText_WaitForSelectorValidatesState()
    {
        var result = Runner().RunText("waitForSelector \"#ready\" state=stable", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("attached, detached, visible, or hidden", result.Error);
    }

    [Fact]
    public void RunText_WaitForTimeoutReturnsOutput()
    {
        var result = Runner().RunText("waitForTimeout 1", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("WAIT_TIMEOUT 001 1", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
