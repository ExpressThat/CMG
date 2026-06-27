using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserControlFlowRunnerActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserControlFlowRunnerActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_ControlFlowActionsRunInsideTests()
    {
        var traceDir = fixture.OutputPath("runner-control-flow-traces");
        var script = fixture.CreateScript("runner-control-flow-actions.cmgscript", $$"""
            test "runner control flow actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              evaluate "window.__runnerFlow = { while: 0, until: 0, doWhile: 0, doUntil: 0, for: [], each: [], json: [], selector: [], switchValue: '', caught: '', finallyRan: false, retry: 0, pass: 0 }"

              set whileValue { evaluate "window.__runnerFlow.while" }
              while (${whileValue} < 2) max=5 {
                evaluate "window.__runnerFlow.while += 1"
                set whileValue { evaluate "window.__runnerFlow.while" }
              }

              until (${whileValue} >= 4) max=5 {
                evaluate "window.__runnerFlow.while += 1"
                set whileValue { evaluate "window.__runnerFlow.while" }
              }

              set doWhileValue { evaluate "window.__runnerFlow.doWhile" }
              doWhile (${doWhileValue} < 2) max=5 {
                evaluate "window.__runnerFlow.doWhile += 1"
                set doWhileValue { evaluate "window.__runnerFlow.doWhile" }
              }

              set doUntilValue { evaluate "window.__runnerFlow.doUntil" }
              doUntil (${doUntilValue} >= 2) max=5 {
                evaluate "window.__runnerFlow.doUntil += 1"
                set doUntilValue { evaluate "window.__runnerFlow.doUntil" }
              }

              for i 0 5 {
                if (${i} == 1) { continue }
                if (${i} == 4) { break }
                evaluate "window.__runnerFlow.for.push('${i}')"
              }

              foreach item "red" "green" "blue" {
                evaluate "window.__runnerFlow.each.push('${item}')"
              }

              foreachJson row "[{\"name\":\"Ada\"},{\"name\":\"Grace\"}]" {
                set rowName { evaluate "JSON.parse('${row}').name" }
                evaluate "window.__runnerFlow.json.push('${index}:${rowName}')"
              }

              foreachSelector item ".item" {
                set itemText { textContent "${item}" }
                evaluate "window.__runnerFlow.selector.push('${index}:${itemText}')"
              }

              switch (${whileValue}) {
                case 4 {
                  evaluate "window.__runnerFlow.switchValue = 'matched'"
                }
                default {
                  fail "switch default should not run"
                }
              }

              try {
                fail "handled failure"
              } catch {
                evaluate "window.__runnerFlow.caught = 'handled'"
              } finally {
                evaluate "window.__runnerFlow.finallyRan = true"
              }

              retry max=3 delay=1 {
                evaluate "window.__runnerFlow.retry += 1"
                expectEval "window.__runnerFlow.retry" equals="2"
              }

              toPass 3 delay=1 {
                evaluate "window.__runnerFlow.pass += 1"
                expectEval "window.__runnerFlow.pass" equals="2"
              }

              withTimeout default=1000 assertion=1000 {
                waitForFunction "window.__runnerFlow.while === 4"
                expectEval "window.__runnerFlow.for.join(',')" equals="0,2,3"
              }

              expectEval "window.__runnerFlow.while" equals="4"
              expectEval "window.__runnerFlow.doWhile" equals="2"
              expectEval "window.__runnerFlow.doUntil" equals="2"
              expectEval "window.__runnerFlow.each.join('|')" equals="red|green|blue"
              expectEval "window.__runnerFlow.json.join('|')" equals="0:Ada|1:Grace"
              expectEval "window.__runnerFlow.selector.length" equals="3"
              expectEval "window.__runnerFlow.switchValue" equals="matched"
              expectEval "window.__runnerFlow.caught" equals="handled"
              expectEval "window.__runnerFlow.finallyRan" equals="True"
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner control flow actions");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "RETRY ");
        AssertTraceContains(trace, "TO_PASS ");
        AssertTraceContains(trace, "EXPECT_EVAL ");
    }

    [Fact]
    public void RunCommand_ControlFlowFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-control-flow-failure.cmgscript", $$"""
            test "runner control flow failure" {
              while (true) max=1 {
                caption "looping"
              }
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=while");
        result.StderrContains("max");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);
}
