using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerConditionOperatorTests
{
    [Fact]
    public void RunText_IfSupportsContainsMatchesAndIn()
    {
        var result = Runner().RunText("""
        set text "checkout-ready"
        set mode "billing"
        if (${text} contains "ready" && ${mode} in "checkout" "billing") {
          type "#status" "contains-in"
        }
        if (${text} matches "checkout-.+") {
          type "#regex" "matched"
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #status contains-in", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #regex matched", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ElseIfSupportsWordOperators()
    {
        var result = Runner().RunText("""
        set status "queued"
        if (${status} == "ready") {
          type "#status" "wrong"
        } elseif (${status} in "queued" "retry") {
          type "#status" "queued"
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #status queued", StringComparison.Ordinal));
        Assert.DoesNotContain(result.StdoutLines, line => line.Contains("wrong", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WhileSupportsWordOperators()
    {
        var result = Runner().RunText("""
        set state "again"
        while (${state} in "again" "retry") max=3 {
          type "#state" "${state}"
          set state "done"
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #state again", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_IfElseIfAndWhileCanUseInlineActionPayloads()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("CMG");
        client.EvaluateResponses.Enqueue("queued");
        client.EvaluateResponses.Enqueue("true");
        client.EvaluateResponses.Enqueue("false");
        client.EvaluateResponses.Enqueue("checkout");
        var result = Runner().RunText("""
        if (evaluate "document.title" == "CMG") {
          type "#title" "matched"
        }
        if (evaluate "window.status" == "ready") {
          type "#status" "wrong"
        } elseif (evaluate "window.status" in "queued" "retry") {
          type "#status" "queued"
        }
        while (evaluate "window.keepGoing" == "true") max=3 {
          type "#loop" "again"
        }
        if (evaluate "window.mode" in "checkout" "billing" && 7 > 5) {
          type "#mixed" "yes"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #title matched", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #status queued", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #loop again", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #mixed yes", StringComparison.Ordinal));
        Assert.DoesNotContain(result.StdoutLines, line => line.Contains("wrong", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_StaticTruthyWordsStillWork()
    {
        var result = Runner().RunText("""
        if (ready) {
          type "#ready" "yes"
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #ready yes", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
