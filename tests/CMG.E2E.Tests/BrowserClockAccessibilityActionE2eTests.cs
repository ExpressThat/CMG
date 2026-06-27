using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserClockAccessibilityActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserClockAccessibilityActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_ClockAndAccessibilityActionsRunAgainstBrowser()
    {
        var snapshot = fixture.OutputPath("direct-accessibility.json");
        var script = fixture.CreateScript("clock-accessibility-actions.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            clock now=1000
            tick 250
            expectEval "Date.now()" equals="1250"
            restoreClock
            expectEval "Date.now() > 1250" equals="True"
            accessibilitySnapshot "#visible-target" output="{{ScriptPath(snapshot)}}"
            expectAccessible role=button name="Visible target"
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("CLOCK 002");
        result.StdoutContains("TICK 003 250 now=1250");
        result.StdoutContains("CLOCK_RESTORED 005");
        result.StdoutContains("ACCESSIBILITY 007");
        result.StdoutContains("ACCESSIBLE 008 role=button");
        CmgE2eAssert.FileExists(snapshot);
        Assert.Contains("Visible target", File.ReadAllText(snapshot), StringComparison.Ordinal);
    }

    [Fact]
    public void DirectScript_AccessibilityFailureReportsReason()
    {
        var script = fixture.CreateScript("bad-accessibility.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            expectAccessible name="Visible target"
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StderrContains("expectAccessible requires role=<role>.");
    }

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
