using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserNavigationWaitRunnerActionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserNavigationWaitRunnerActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_NavigationAndWaitActionsRunInsideTests()
    {
        var first = fixture.FixtureHttpUri("index.html") + "?runner=nav-one";
        var second = fixture.FixtureHttpUri("index.html") + "?runner=nav-two";
        var traceDir = fixture.OutputPath("runner-navigation-wait-traces");
        var script = fixture.CreateScript("runner-navigation-wait-actions.cmgscript", $$"""
            test "runner navigation and wait actions" {
              setDefaultTimeout 1000
              setDefaultNavigationTimeout 5000
              setDefaultAssertionTimeout 1000
              setDefaultExpectTimeout 1000
              navigate "{{first}}" waitUntil=domcontentloaded
              waitForSelector "#primary" state=visible
              waitForElement "#title"
              waitForFunction "document.readyState === 'complete'"
              waitForTimeout "1"
              expectUrl "nav-one"
              expectTitle "CMG E2E Fixture"
              navigate "{{second}}" waitUntil=domcontentloaded
              waitForNavigation "nav-two" waitUntil=domcontentloaded timeout=5000
              goBack waitUntil=domcontentloaded timeout=5000
              waitForUrl "nav-one" timeout=5000
              goForward waitUntil=domcontentloaded timeout=5000
              waitForUrl "nav-two" timeout=5000
              reload waitUntil=domcontentloaded
              waitForTitle "CMG E2E Fixture" timeout=5000
              waitForLoadState "complete" timeout=5000
              waitForNetworkIdle timeout=5000
              evaluate "setTimeout(() => window.__runnerDelayedReady = true, 50); true"
              waitForFunction "window.__runnerDelayedReady === true" timeout=1000
              withNavigationTimeout 5000 {
                waitForTitle "CMG E2E Fixture"
              }
              withExpectTimeout 1000 {
                expectText "#title" "CMG E2E Fixture"
              }
              wait "#primary" timeout=1000
              wait "1"
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner navigation and wait actions");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "DEFAULT_TIMEOUT");
        AssertTraceContains(trace, "DEFAULT_NAVIGATION_TIMEOUT");
        AssertTraceContains(trace, "DEFAULT_ASSERTION_TIMEOUT");
        AssertTraceContains(trace, "NAVIGATED");
        AssertTraceContains(trace, "SELECTOR");
        AssertTraceContains(trace, "FUNCTION");
        AssertTraceContains(trace, "WAIT_TIMEOUT");
        AssertTraceContains(trace, "URL");
        AssertTraceContains(trace, "TITLE");
        AssertTraceContains(trace, "NAVIGATION");
        AssertTraceContains(trace, "BACK");
        AssertTraceContains(trace, "FORWARD");
        AssertTraceContains(trace, "RELOADED");
        AssertTraceContains(trace, "LOAD_STATE");
        AssertTraceContains(trace, "NETWORK_IDLE");
    }

    [Fact]
    public void RunCommand_WaitFailureReportsActualStep()
    {
        var script = fixture.CreateScript("runner-wait-failure.cmgscript", $$"""
            test "runner wait failure" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              waitForSelector "#hidden-target" state=visible timeout=20
              caption "should not run"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=waitForSelector");
        result.StderrContains("did not reach state visible");
        result.StderrContains("attached=true, visible=false");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);
}
