using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserSoftSkipActionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserSoftSkipActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_SkipStopsLaterActionsButExitsSuccessfully()
    {
        var script = fixture.CreateScript("direct-skip.cmgscript", """
            skip "feature disabled"
            fail "should not run"
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("SKIP 001 feature disabled");
        Assert.DoesNotContain("should not run", result.Stdout + result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public void DirectScript_SoftExpectContinuesThenFailsWithReason()
    {
        var script = fixture.CreateScript("direct-soft-expect.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            softExpect (1 > 2) message="first diagnostic failed"
            fill "#name" "continued"
            expectValue "#name" "continued"
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StdoutContains("SOFT_EXPECT 002 false first diagnostic failed");
        result.StdoutContains("PASS 003 fill #name continued");
        result.StdoutContains("PASS 004 expectValue #name continued");
        result.StderrContains("first diagnostic failed");
    }
}
