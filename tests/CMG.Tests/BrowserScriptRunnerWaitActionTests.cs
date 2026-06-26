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
    public void RunText_SetDefaultTimeoutAppliesToLaterWaits()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("setDefaultTimeout 750\nwaitForSelector \"#ready\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(750, client.LastWaitTimeout);
        Assert.Contains("DEFAULT_TIMEOUT 001 750", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetDefaultTimeoutRejectsNegativeValues()
    {
        var result = Runner().RunText("setDefaultTimeout -1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("setDefaultTimeout= must be zero or greater", result.Error);
    }

    [Fact]
    public void RunText_ExplicitTimeoutOverridesDefaultTimeout()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("setDefaultTimeout 750\nwaitForSelector \"#ready\" timeout=125", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(125, client.LastWaitTimeout);
    }

    [Fact]
    public void RunText_CommandTimeoutOptionsApplyBeforeFirstAction()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText(
            "waitForSelector \"#ready\"",
            "debug",
            client,
            timeouts: new ScriptTimeoutOptions(DefaultTimeout: 900));

        Assert.True(result.Success, result.Error);
        Assert.Equal(900, client.LastWaitTimeout);
    }

    [Fact]
    public void RunText_WithTimeoutAppliesOnlyInsideBlock()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        setDefaultTimeout 100
        withTimeout 900 {
          waitForSelector "#inside"
        }
        waitForSelector "#outside"
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(100, client.LastWaitTimeout);
        Assert.Contains("SELECTOR 003 #inside", result.StdoutLines);
        Assert.Contains("SELECTOR 005 #outside", result.StdoutLines);
    }

    [Fact]
    public void RunText_WithTimeoutCanScopeSpecificTimeoutFamilies()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        withTimeout default=300 navigation=700 {
          waitForSelector "#ready"
          navigate "https://example.test" waitUntil=load
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(300, client.LastWaitTimeout);
        Assert.Contains("Date.now() + 700", client.LastExpression);
    }

    [Fact]
    public void RunText_WithTimeoutRestoresDefaultsAfterFailure()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"attached":true,"visible":false}""");
        var result = Runner().RunText("""
        setDefaultTimeout 100
        try {
          withTimeout 5 {
            waitForSelector "#toast" state=visible
          }
        } catch error {
          waitForSelector "#fallback"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal(100, client.LastWaitTimeout);
    }

    [Fact]
    public void RunText_WithTimeoutRequiresTimeoutValue()
    {
        var result = Runner().RunText("withTimeout { waitForSelector \"#ready\" }", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("withTimeout requires a positional timeout", result.Error);
    }

    [Fact]
    public void RunText_NavigationTimeoutAppliesToNavigationActions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText(
            "setDefaultNavigationTimeout 600\nnavigate \"https://example.test\" waitUntil=load",
            "debug",
            client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("Date.now() + 600", client.LastExpression);
        Assert.Contains("DEFAULT_NAVIGATION_TIMEOUT 001 600", result.StdoutLines);
    }

    [Fact]
    public void RunText_AssertionTimeoutOptionAppliesToAssertions()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Loading");
        client.TextResponses.Enqueue("Ready");
        var result = Runner().RunText(
            "assertText \"#status\" \"Ready\"",
            "debug",
            client,
            timeouts: new ScriptTimeoutOptions(DefaultTimeout: 100, AssertionTimeout: 250));

        Assert.True(result.Success, result.Error);
        Assert.Contains("PASS 001 assertText #status Ready", result.StdoutLines);
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
