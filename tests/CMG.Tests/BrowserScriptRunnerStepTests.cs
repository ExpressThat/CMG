using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerStepTests
{
    [Fact]
    public void RunText_StepShowsCaptionAndRunsChildren()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        step "Open" {
          click "#open"
          caption "Done"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#open", client.LastClickedSelector);
        Assert.Contains("PASS 001 step Open", result.StdoutLines);
        Assert.Contains("PASS 001 click #open", result.StdoutLines);
        Assert.Contains("PASS 002 caption Done", result.StdoutLines);
    }

    [Fact]
    public void RunText_StepCanUseVariablesAndNestControlFlow()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        set target "#save"
        step "Use ${target}" {
          if true {
            click "${target}"
          }
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#save", client.LastClickedSelector);
        Assert.Contains("PASS 002 step \"Use #save\"", result.StdoutLines);
    }

    [Fact]
    public void RunText_StepChildFailureReportsChildReason()
    {
        var result = Runner().RunText("""
        step "Guard" {
          fail "Missing setup"
        }
        """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Equal("Line 2: fail failed. Missing setup", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
