using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerSwitchTests
{
    [Fact]
    public void RunText_SwitchRunsFirstMatchingCase()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("ok");
        var result = Runner().RunText("""
        set status "ready"
        switch ${status} {
          case "missing" {
            evaluate "'wrong'"
          }
          case "ready" {
            evaluate "'ok'"
          }
          default {
            evaluate "'default'"
          }
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.StartsWith("EVALUATE ", StringComparison.Ordinal) && line.Contains("ok", StringComparison.Ordinal));
        Assert.DoesNotContain(result.StdoutLines, line => line.Contains("wrong", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_SwitchSupportsUsefulMatchers()
    {
        var result = Runner().RunText("""
        set count 7
        set text "checkout-ready"
        switch ${count} {
          case > 5 {
            type "#count" "big"
          }
        }
        switch ${text} {
          case contains "ready" {
            type "#contains" "yes"
          }
        }
        switch ${text} {
          case matches "checkout-.+" {
            type "#matches" "yes"
          }
        }
        switch "smoke" {
          case in "unit" "smoke" "integration" {
            type "#in" "yes"
          }
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #count big", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #contains yes", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #matches yes", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #in yes", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_SwitchCanUseInlineActionPayloadAsSubject()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Checkout");
        var result = Runner().RunText("""
        switch title {
          case contains "Check" {
            type "#title" "matched"
          }
          default {
            type "#title" "fallback"
          }
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #title matched", StringComparison.Ordinal));
        Assert.DoesNotContain(result.StdoutLines, line => line.Contains("fallback", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_SwitchBranchMacrosDoNotLeak()
    {
        var result = Runner().RunText("""
        switch "ready" {
          case "ready" {
            macro helper {
              evaluate "'ok'"
            }
          }
        }
        call helper
        """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Macro 'helper' is not defined", result.Error);
    }

    [Fact]
    public void RunText_CaseOutsideSwitchFailsClearly()
    {
        var result = Runner().RunText("""
        case "ready" {
          evaluate "'no'"
        }
        """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("case must follow a switch block", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
