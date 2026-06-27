using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerOutputContextTests
{
    [Fact]
    public void RunText_OutputUsesGlobalSequenceLineActionAndOptionalContext()
    {
        var result = Runner().RunText("""
        navigate https://example.test
        macro login {
          repeat 2 {
            click #save
          }
        }
        call login
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("PASS 001 line=1 action=navigate https://example.test", result.StdoutLines);
        Assert.DoesNotContain(result.StdoutLines, line => line.StartsWith("PASS 001 ", StringComparison.Ordinal) && line.Contains("context=", StringComparison.Ordinal));
        var contextualClicks = result.StdoutLines
            .Where(line => line.Contains("line=4 context=\"macro login > repeat[", StringComparison.Ordinal) && line.Contains(" action=click #save", StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(2, contextualClicks.Length);
        Assert.True(Sequence(contextualClicks[0]) < Sequence(contextualClicks[1]));
    }

    [Fact]
    public void RunText_BooleanConditionsAcceptCapturedEvaluateCasing()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("True");
        var result = Runner().RunText("""
        set ready {
          evaluate "document.querySelector('#app') !== null"
        }
        while (${ready} != true) {
          fail "should not loop"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_CaughtFailurePreservesContextInVariableAndStepRecord()
    {
        var result = Runner().RunText("""
        macro login {
          repeat 1 {
            try {
              fail "boom"
            } catch err {
              return "${err}"
            }
          }
        }
        set caught {
          call login
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.Contains("Line 4: fail failed in macro login > repeat[1/1] > try. boom", StringComparison.Ordinal));
        Assert.Contains(result.StepRecords, step => !step.Success && step.Context == "macro login > repeat[1/1] > try" && step.LineNumber == 4);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private static int Sequence(string line) => int.Parse(line.Split(' ')[1]);
}
