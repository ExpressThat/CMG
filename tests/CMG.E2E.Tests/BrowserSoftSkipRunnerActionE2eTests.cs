using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserSoftSkipRunnerActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserSoftSkipRunnerActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_SkipStopsLaterActionsAndReportsSkipped()
    {
        var traceDir = fixture.OutputPath("runner-skip-traces");
        var script = fixture.CreateScript("runner-skip-action.cmgscript", """
            test "runner runtime skip" {
              skip "feature disabled"
              fail "should not run"
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST SKIP runner runtime skip");
        Assert.DoesNotContain("should not run", result.Stdout + result.Stderr, StringComparison.Ordinal);
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "\"success\": true");
        AssertTraceContains(trace, "\"error\": \"feature disabled\"");
        AssertTraceContains(trace, "SKIP");
    }

    [Fact]
    public void RunCommand_SoftExpectContinuesThenFailsWithReason()
    {
        var json = fixture.OutputPath("runner-soft-report.json");
        var traceDir = fixture.OutputPath("runner-soft-traces");
        var script = fixture.CreateScript("runner-soft-expect-action.cmgscript", $$"""
            test "runner soft expect" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              softExpect (1 > 2) message="first runner diagnostic failed"
              fill "#name" "continued"
              expectValue "#name" "continued"
            }
            """);

        var result = fixture.Cli.Run("run", script, "--report-json", json, "--trace", traceDir);

        result.ShouldFail();
        result.StdoutContains("TEST FAIL runner soft expect");
        result.StderrContains("first runner diagnostic failed");
        CmgE2eAssert.FileExists(json);
        var report = File.ReadAllText(json);
        AssertTraceContains(report, "first runner diagnostic failed");
        AssertTraceContains(report, "continued");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "SOFT_EXPECT");
        AssertTraceContains(trace, "PASS");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);
}
