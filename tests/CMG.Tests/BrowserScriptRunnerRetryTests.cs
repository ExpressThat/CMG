using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerRetryTests
{
    [Fact]
    public void RunText_RetryRepeatsUntilSuccess()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Waiting");
        client.TextResponses.Enqueue("Ready");

        var result = Runner().RunText("""
        retry max=2 {
          assertText "#status" "Ready"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.Contains("attempt=1 failed", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line == "RETRY 001 success attempt=2");
        Assert.Equal("#status", client.LastElementTextSelector);
    }

    [Fact]
    public void RunText_RetrySupportsPositionalCount()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Nope");
        client.TextResponses.Enqueue("Ready");

        var result = Runner().RunText("""
        retry 2 {
          assertText "#status" "Ready"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.StdoutLines, line => line == "RETRY 001 success attempt=2");
    }

    [Fact]
    public void RunText_RetryFailsWithLastReason()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Waiting");
        client.TextResponses.Enqueue("Still waiting");

        var result = Runner().RunText("""
        retry max=2 {
          assertText "#status" "Ready"
        }
        """, "debug", client);

        Assert.False(result.Success);
        Assert.Contains("retry exhausted 2 attempt(s)", result.Error);
        Assert.Contains("Expected text 'Ready'", result.Error);
    }

    [Fact]
    public void RunText_RetryRejectsInvalidMax()
    {
        var result = Runner().RunText("""
        retry max=0 {
          caption "never"
        }
        """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("retry max must be greater than 0", result.Error);
    }

    [Fact]
    public void RunText_RetryRequiresBlock()
    {
        var result = Runner().RunText("retry 2", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("retry requires a block body", result.Error);
    }

    [Fact]
    public void RunText_ToPassRepeatsUntilSuccess()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Waiting");
        client.TextResponses.Enqueue("Ready");

        var result = Runner().RunText("""
        toPass max=2 {
          assertText "#status" "Ready"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.StdoutLines, line => line.Contains("TO_PASS 001 attempt=1 failed", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line == "TO_PASS 001 success attempt=2");
    }

    [Fact]
    public void RunText_ToPassFailsWithNamedReason()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Waiting");
        client.TextResponses.Enqueue("Still waiting");

        var result = Runner().RunText("""
        toPass max=2 {
          assertText "#status" "Ready"
        }
        """, "debug", client);

        Assert.False(result.Success);
        Assert.Contains("toPass exhausted 2 attempt(s)", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
