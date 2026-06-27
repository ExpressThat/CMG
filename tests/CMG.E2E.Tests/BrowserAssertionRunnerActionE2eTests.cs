using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserAssertionRunnerActionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserAssertionRunnerActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_AssertionAliasesRunInsideTests()
    {
        var traceDir = fixture.OutputPath("runner-assertion-traces");
        var script = fixture.CreateScript("runner-assertion-actions.cmgscript", $$"""
            test "runner assertion aliases" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              fill "#name" "CMG"
              focus "#name"
              expectVisible "#visible-target"
              expectHidden "#hidden-target"
              expectEnabled "#primary"
              expectDisabled "#disabled-button"
              expectAttached "#primary"
              expectDetached "#not-present" timeout=50
              expectEditable "#name"
              expectNotEditable "#primary"
              expectEmpty "#empty-target"
              expectNotEmpty "#title"
              expectFocused "#name"
              expectNotFocused "#primary"
              scrollIntoView "#visible-target"
              expectInViewport "#visible-target"
              expectNotInViewport "#deep-button"
              evaluate "const s=document.querySelector('#multi'); for (const o of s.options) o.selected=['alpha','beta'].includes(o.value); s.dispatchEvent(new Event('change',{bubbles:true})); true"
              expectValue "#name" "CMG"
              expectValues "#multi" "alpha" "beta"
              expectAttribute "#primary" "data-state" "idle"
              expectClass "#class-target" "beta"
              expectId "#primary" "primary"
              expectCss "#css-target" "color" "rgb(10, 20, 30)"
              expectProperty "#primary" "dataset.state" "idle"
              expectAccessibleName "#visible-target" "Visible target"
              expectRole "#visible-target" "button"
              expectUnchecked "#agree"
              check "#agree"
              expectChecked "#agree"
              expectCount ".item" "3"
              expectText "#title" "CMG E2E Fixture"
              toContainText "#title" "CMG"
              expectNoText "#status" "missing" timeout=50
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner assertion aliases");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "EXPECT");
        AssertTraceContains(trace, "EVALUATE");
    }

    [Fact]
    public void RunCommand_AssertionFailureReportsActualStep()
    {
        var script = fixture.CreateScript("runner-assertion-failure.cmgscript", $$"""
            test "runner assertion failure" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              expectClass "#class-target" "missing-class"
              caption "should not run"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=expectClass");
        result.StderrContains("missing-class");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);
}
