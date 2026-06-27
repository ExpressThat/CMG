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

    [Fact]
    public void RunCommand_ClockAndAccessibilityActionsRunInsideTests()
    {
        var snapshot = fixture.OutputPath("runner-accessibility.json");
        var traceDir = fixture.OutputPath("runner-clock-accessibility-traces");
        var script = fixture.CreateScript("runner-clock-accessibility.cmgscript", $$"""
            test "runner clock accessibility actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              clock now=1000
              tick 250
              expectEval "Date.now()" equals="1250"
              restoreClock
              expectEval "Date.now() > 1250" equals="True"
              accessibilitySnapshot "#visible-target" output="{{ScriptPath(snapshot)}}"
              expectAccessible role=button name="Visible target"
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner clock accessibility actions");
        CmgE2eAssert.FileExists(snapshot);
        Assert.Contains("Visible target", File.ReadAllText(snapshot), StringComparison.Ordinal);
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "CLOCK");
        AssertTraceContains(trace, "TICK");
        AssertTraceContains(trace, "CLOCK_RESTORED");
        AssertTraceContains(trace, "ACCESSIBILITY");
        AssertTraceContains(trace, "ACCESSIBLE");
    }

    [Fact]
    public void RunCommand_AccessibilityFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-bad-accessibility.cmgscript", $$"""
            test "runner accessibility failure" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              expectAccessible name="Visible target"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=expectAccessible");
        result.StderrContains("expectAccessible requires role=<role>.");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
