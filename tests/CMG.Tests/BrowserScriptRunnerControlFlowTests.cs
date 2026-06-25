using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerControlFlowTests
{
    [Fact]
    public void RunText_IfSupportsVariableAndStaticComparisons()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("big");
        var result = Runner().RunText("""
        set count 7
        if (${count} > 5 && !(${count} == 0)) {
          evaluate "'big'"
        } elseif (${count} == "") {
          evaluate "'empty'"
        } else {
          evaluate "'small'"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("EVALUATE 003 big", result.StdoutLines);
    }

    [Fact]
    public void RunText_MacroCanReturnPayloadIntoSetBlock()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("CMG");
        client.EvaluateResponses.Enqueue("CMG");
        var result = Runner().RunText("""
        macro readTitle {
          evaluate "document.title"
        }
        set title {
          call readTitle
        }
        evaluate "'${title}'"
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Contains("SET 004 title CMG", result.StdoutLines);
        Assert.Contains("EVALUATE 007 CMG", result.StdoutLines);
    }

    [Fact]
    public void RunText_MacroReceivesParameters()
    {
        var result = Runner().RunText("""
        macro write selector value {
          type "${selector}" "${value}"
        }
        call write "#name" "Agent"
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success);
    }

    [Fact]
    public void RunText_ControlBlocksAndMacrosCanNestDeeply()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        macro outer selector {
          macro middle prefix {
            macro inner target text {
              if (${text} == "go") {
                for i 0 2 {
                  if (${i} == 1) {
                    type "${target}" "${prefix}-${i}"
                  }
                }
              }
            }
            call inner "${selector}" "go"
          }
          call middle "ok"
        }
        call outer "#box"
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal("#box", client.LastTypedSelector);
        Assert.Equal("ok-1", client.LastTypedText);
    }

    [Fact]
    public void RunText_MacrosDeclaredInsideMacroDoNotLeak()
    {
        var result = Runner().RunText("""
        macro outer {
          macro inner {
            evaluate "'inside'"
          }
          call inner
        }
        call outer
        call inner
        """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Macro 'inner' is not defined", result.Error);
    }

    [Fact]
    public void RunText_SetInsideMacroDoesNotMutateCallerVariable()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("outer");
        var result = Runner().RunText("""
        set name "outer"
        macro changeName {
          set name "inner"
        }
        call changeName
        evaluate "'${name}'"
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("EVALUATE 006 outer", result.StdoutLines);
    }

    [Fact]
    public void RunText_MacroReadsCallerVariableButLocalSetDoesNotLeak()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("global");
        var result = Runner().RunText("""
        set token "global"
        macro readToken {
          return "${token}"
        }
        macro shadowToken {
          set token "local"
          return "${token}"
        }
        set first {
          call readToken
        }
        set second {
          call shadowToken
        }
        evaluate "'${token}'"
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("SET 008 first global", result.StdoutLines);
        Assert.Contains("SET 011 second local", result.StdoutLines);
        Assert.Contains("EVALUATE 014 global", result.StdoutLines);
    }

    [Fact]
    public void RunText_NestedMacroReadsParentTreeVariableAndShadowsLocally()
    {
        var result = Runner().RunText("""
        macro outer {
          set token "parent"
          macro inner {
            set first {
              return "${token}"
            }
            set token "inner"
            return "${first}-${token}"
          }
          set result {
            call inner
          }
          return "${result}-${token}"
        }
        set final {
          call outer
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("SET 015 final parent-inner-parent", result.StdoutLines);
    }

    [Fact]
    public void RunText_TopLevelMacroIgnoresUnrelatedCallerLocalShadow()
    {
        var result = Runner().RunText("""
        set token "global"
        macro readTop {
          return "${token}"
        }
        macro caller {
          set token "local"
          call readTop
        }
        set final {
          call caller
        }
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("SET 009 final global", result.StdoutLines);
    }

    [Fact]
    public void RunText_MacroCanReturnVariableValue()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Agent");
        var result = Runner().RunText("""
        macro readName value {
          return "${value}"
        }
        set name {
          call readName "Agent"
        }
        evaluate "'${name}'"
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("SET 004 name Agent", result.StdoutLines);
        Assert.Contains("EVALUATE 007 Agent", result.StdoutLines);
    }

    [Fact]
    public void Run_ImportsMacrosRelativeToScriptFile()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        try
        {
            var shared = Path.Combine(directory.FullName, "shared.cmgscript");
            File.WriteAllText(shared, """
            macro readTitle {
              evaluate "document.title"
            }
            """);
            var main = Path.Combine(directory.FullName, "main.cmgscript");
            File.WriteAllText(main, """
            import "shared.cmgscript"
            set title {
              call readTitle
            }
            """);
            var client = new FakeAutomationClient();
            client.EvaluateResponses.Enqueue("CMG");

            var result = Runner().Run(main, "debug", client, gif: null);

            Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
            Assert.Contains("SET 004 title CMG", result.StdoutLines);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
