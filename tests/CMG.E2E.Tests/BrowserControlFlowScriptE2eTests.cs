using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserControlFlowScriptE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserControlFlowScriptE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ScriptCommand_ExecutesAdvancedControlFlowAgainstBrowser()
    {
        var script = fixture.CreateScript("control-flow.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}"
        evaluate "window.__cmgLoop = { while: 0, until: 0, doWhile: 0, doUntil: 0, for: [], each: [], json: [], selector: [], retry: 0, toPass: 0 }"

        set whileValue { evaluate "window.__cmgLoop.while" }
        while (${whileValue} < 2) max=5 {
          evaluate "window.__cmgLoop.while += 1"
          set whileValue { evaluate "window.__cmgLoop.while" }
        }

        until (${whileValue} >= 4) max=5 {
          evaluate "window.__cmgLoop.while += 1"
          set whileValue { evaluate "window.__cmgLoop.while" }
        }

        set doWhileValue { evaluate "window.__cmgLoop.doWhile" }
        doWhile (${doWhileValue} < 2) max=5 {
          evaluate "window.__cmgLoop.doWhile += 1"
          set doWhileValue { evaluate "window.__cmgLoop.doWhile" }
        }

        set doUntilValue { evaluate "window.__cmgLoop.doUntil" }
        doUntil (${doUntilValue} >= 2) max=5 {
          evaluate "window.__cmgLoop.doUntil += 1"
          set doUntilValue { evaluate "window.__cmgLoop.doUntil" }
        }

        for i 0 5 {
          if (${i} == 1) {
            continue
          }
          if (${i} == 4) {
            break
          }
          evaluate "window.__cmgLoop.for.push('${i}')"
        }

        foreach item "red" "green" "blue" {
          evaluate "window.__cmgLoop.each.push('${item}')"
        }

        foreachJson row "[{\"name\":\"Ada\"},{\"name\":\"Grace\"}]" {
          set rowName { evaluate "JSON.parse('${row}').name" }
          evaluate "window.__cmgLoop.json.push('${index}:${rowName}')"
        }

        foreachSelector item ".item" {
          set itemText { textContent "${item}" }
          evaluate "window.__cmgLoop.selector.push('${index}:${itemText}')"
        }

        retry max=3 delay=1 {
          evaluate "window.__cmgLoop.retry += 1"
          expectEval "window.__cmgLoop.retry" equals="2"
        }

        toPass 3 delay=1 {
          evaluate "window.__cmgLoop.toPass += 1"
          expectEval "window.__cmgLoop.toPass" equals="2"
        }

        withTimeout default=1000 assertion=1000 {
          waitForFunction "window.__cmgLoop.while === 4"
          expectEval "window.__cmgLoop.for.join(',')" equals="0,2,3"
        }

        withDefaultTimeout 1000 {
          withAssertionTimeout 1000 {
            expectEval "window.__cmgLoop.selector.join('|')" contains="0:Alpha"
          }
        }

        expectEval "window.__cmgLoop.while" equals="4"
        expectEval "window.__cmgLoop.doWhile" equals="2"
        expectEval "window.__cmgLoop.doUntil" equals="2"
        expectEval "window.__cmgLoop.each.join('|')" equals="red|green|blue"
        expectEval "window.__cmgLoop.json.join('|')" equals="0:Ada|1:Grace"
        expectEval "window.__cmgLoop.selector.length" equals="3"
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("RETRY ");
        result.StdoutContains("TO_PASS ");
        result.StdoutContains("EXPECT_EVAL ");
    }

    [Fact]
    public void ScriptCommand_StepBlockShowsCaptionAndRunsChildren()
    {
        var trace = fixture.OutputPath("step-block.trace.json");
        var script = fixture.CreateScript("step-block.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
        step "Open primary flow" {
          scrollIntoView "#primary"
          click "#primary"
          assertText "#status" "clicked"
        }
        set captionText { evaluate "document.getElementById('__cmg_message_bar_text')?.textContent" }
        expect ("${captionText}" == "Open primary flow")
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script, "--trace", trace);

        result.ShouldPass();
        result.StdoutContains("PASS 002 step");
        result.StdoutContains("PASS 002 click #primary");
        CmgE2eAssert.FileExists(trace);
        AssertTraceContains(File.ReadAllText(trace), "Open primary flow");
    }

    [Fact]
    public void ScriptCommand_StepChildFailureReportsChildReason()
    {
        var script = fixture.CreateScript("step-child-failure.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
        step "Guard child failure" {
          assertText "#status" "not the status" timeout=20
        }
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StderrContains("Line 3");
        result.StderrContains("assertText");
        result.StderrContains("not the status");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);
}
