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
        Assert.Contains(result.StdoutLines, line => line.Contains("action=step Open", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("context=\"step Open\" action=click #open", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("context=\"step Open\" action=caption Done", StringComparison.Ordinal));
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
        Assert.Contains(result.StdoutLines, line => line.Contains("action=step \"Use #save\"", StringComparison.Ordinal));
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
        Assert.Equal("Line 2: fail failed in step Guard. Missing setup", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
