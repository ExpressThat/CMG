using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerLoopControlTests
{
    [Fact]
    public void RunText_ForAndForEachExecuteBlocks()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        for i 0 2 {
          type "#item-${i}" "${i}"
        }
        foreach name Alice Bob {
          type "#name" "${name}"
        }
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#name", client.LastTypedSelector);
        Assert.Equal("Bob", client.LastTypedText);
    }

    [Fact]
    public void RunText_RepeatSupportsBreakAndContinue()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        repeat i 4 {
          if (${i} == 1) {
            continue
          }
          if (${i} == 3) {
            break
          }
          type "#item" "${i}"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal("2", client.LastTypedText);
    }

    [Fact]
    public void RunText_WhileReevaluatesConditionFromCurrentScope()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        set keepGoing true
        while (${keepGoing} == true) max=3 {
          set keepGoing false
          type "#state" "done"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal("done", client.LastTypedText);
    }

    [Fact]
    public void RunText_UntilSkipsWhenConditionAlreadyTrue()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        set ready true
        until (${ready} == true) max=2 {
          type "#state" "waiting"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal(string.Empty, client.LastTypedText);
    }

    [Fact]
    public void RunText_DoWhileRunsBeforeCheckingCondition()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        set keepGoing false
        doWhile (${keepGoing} == true) max=2 {
          type "#state" "ran-once"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal("ran-once", client.LastTypedText);
    }

    [Fact]
    public void RunText_DoUntilRunsUntilConditionBecomesTrue()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        set ready false
        doUntil (${ready} == true) max=3 {
          set ready true
          type "#state" "done"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal("done", client.LastTypedText);
    }

    [Fact]
    public void RunText_BreakOutsideLoopFailsClearly()
    {
        var result = Runner().RunText("break", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Equal("break must be inside a loop.", result.Error);
    }

    [Fact]
    public void RunText_ForEachSelectorProvidesSelectorAndIndex()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("2");
        var result = Runner().RunText("""
        foreachSelector item ".row" {
          click "${item}"
        }
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#__cmg_foreach_1_1", client.LastClickedSelector);
    }

    [Fact]
    public void RunText_MacroReceivesSelectorFromIterator()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("2");
        var result = Runner().RunText("""
        macro choose item {
          click "${item}"
        }
        foreachSelector row ".row" {
          call choose "${row}"
        }
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#__cmg_foreach_4_1", client.LastClickedSelector);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
