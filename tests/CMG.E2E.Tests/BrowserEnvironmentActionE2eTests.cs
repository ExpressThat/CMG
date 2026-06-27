using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserEnvironmentActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserEnvironmentActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ScriptEnvironmentActions_AffectPageVisibleState()
    {
        var script = fixture.CreateScript("environment-actions.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            emulate locale=en-GB timezone=Europe/London colorScheme=dark reducedMotion=reduce
            assertEval "navigator.language" equals="en-GB"
            assertEval "Intl.DateTimeFormat().resolvedOptions().timeZone" equals="Europe/London"
            assertEval "matchMedia('(prefers-color-scheme: dark)').matches" equals="True"
            assertEval "matchMedia('(prefers-reduced-motion: reduce)').matches" equals="True"
            emulateMedia media=print forcedColors=active contrast=more
            assertEval "matchMedia('print').matches" equals="True"
            assertEval "matchMedia('(forced-colors: active)').matches" equals="True"
            assertEval "matchMedia('(prefers-contrast: more)').matches" equals="True"
            setGeolocation "51.5,-0.12" accuracy=7
            set geo { evaluate "new Promise(resolve => navigator.geolocation.getCurrentPosition(position => resolve(position.coords.latitude + ',' + position.coords.longitude + ',' + position.coords.accuracy)))" }
            expect ("${geo}" == "51.5,-0.12,7")
            grantPermissions "geolocation" "notifications"
            set granted { evaluate "navigator.permissions.query({ name: 'geolocation' }).then(result => result.state)" }
            expect ("${granted}" == "granted")
            clearPermissions
            set cleared { evaluate "navigator.permissions.query({ name: 'geolocation' }).then(result => result.state)" }
            expect ("${cleared}" == "prompt")
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("EMULATE");
        result.StdoutContains("MEDIA");
        result.StdoutContains("GEOLOCATION");
        result.StdoutContains("PERMISSIONS");
        result.StdoutContains("PERMISSIONS_CLEARED");
    }

    [Fact]
    public void ScriptEnvironmentActions_ReportValidationFailures()
    {
        var script = fixture.CreateScript("bad-environment-action.cmgscript", """
            navigate "about:blank"
            emulateMedia media=paper
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StderrContains("media=");
    }

    [Fact]
    public void RunCommand_EnvironmentActionsRunInsideTests()
    {
        var traceDir = fixture.OutputPath("runner-environment-traces");
        var script = fixture.CreateScript("runner-environment-actions.cmgscript", $$"""
            test "runner environment actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              emulate locale=en-GB timezone=Europe/London colorScheme=dark reducedMotion=reduce
              assertEval "navigator.language" equals="en-GB"
              assertEval "Intl.DateTimeFormat().resolvedOptions().timeZone" equals="Europe/London"
              assertEval "matchMedia('(prefers-color-scheme: dark)').matches" equals="True"
              assertEval "matchMedia('(prefers-reduced-motion: reduce)').matches" equals="True"
              emulateMedia media=print forcedColors=active contrast=more
              assertEval "matchMedia('print').matches" equals="True"
              assertEval "matchMedia('(forced-colors: active)').matches" equals="True"
              assertEval "matchMedia('(prefers-contrast: more)').matches" equals="True"
              setGeolocation latitude=51.5 longitude=-0.12 accuracy=7
              set geo { evaluate "new Promise(resolve => navigator.geolocation.getCurrentPosition(position => resolve(position.coords.latitude + ',' + position.coords.longitude + ',' + position.coords.accuracy)))" }
              expect ("${geo}" == "51.5,-0.12,7")
              grantPermissions permissions="geolocation,notifications"
              set granted { evaluate "navigator.permissions.query({ name: 'geolocation' }).then(result => result.state)" }
              expect ("${granted}" == "granted")
              clearPermissions
              set cleared { evaluate "navigator.permissions.query({ name: 'geolocation' }).then(result => result.state)" }
              expect ("${cleared}" == "prompt")
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner environment actions");
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "EMULATE");
        AssertTraceContains(trace, "MEDIA");
        AssertTraceContains(trace, "GEOLOCATION");
        AssertTraceContains(trace, "PERMISSIONS");
        AssertTraceContains(trace, "PERMISSIONS_CLEARED");
    }

    [Fact]
    public void RunCommand_EnvironmentActionFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-bad-environment-action.cmgscript", """
            test "runner environment failure" {
              emulateMedia media=paper
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=emulateMedia");
        result.StderrContains("media=");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);
}
