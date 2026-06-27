using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserEventActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserEventActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_ConsoleAndPageErrorActionsRunAgainstBrowser()
    {
        Directory.CreateDirectory(fixture.OutputDirectory);
        File.WriteAllText(fixture.OutputPath("generic-event-download.txt"), "download-ready");
        var script = fixture.CreateScript("event-actions.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            captureConsole
            evaluate "console.warn('direct console ready'); true"
            waitForConsole "^direct console" match=regex level=warn timeout=5000
            expectNoConsole "not emitted" level=error timeout=10
            evaluate "console.info('generic console ready'); true"
            waitForEvent console message="generic console ready" level=info timeout=5000
            capturePageErrors
            evaluate "setTimeout(() => { throw new Error('direct page boom') }, 0); true"
            waitForPageError "direct page boom" timeout=5000
            expectNoPageError "not emitted" timeout=10
            waitForEvent pageError text="direct page boom" timeout=5000
            waitForEvent download directory="{{ScriptPath(fixture.OutputDirectory)}}" pattern="generic-event-download.txt" timeout=1000
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("CONSOLE_CAPTURE 002");
        result.StdoutContains("CONSOLE 004");
        result.StdoutContains("CONSOLE_OK 005");
        result.StdoutContains("CONSOLE 007");
        result.StdoutContains("PAGE_ERROR_CAPTURE 008");
        result.StdoutContains("PAGE_ERROR 010");
        result.StdoutContains("PAGE_ERROR_OK 011");
        result.StdoutContains("PAGE_ERROR 012");
        result.StdoutContains("DOWNLOAD 013");
    }

    [Fact]
    public void DirectScript_WaitForEventValidationReportsMatcherReason()
    {
        var script = fixture.CreateScript("bad-event-action.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            waitForEvent console
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StderrContains("waitForEvent console requires a matcher");
    }

    [Fact]
    public void RunCommand_EventAndDialogActionsRunInsideTests()
    {
        Directory.CreateDirectory(fixture.OutputDirectory);
        File.WriteAllText(fixture.OutputPath("runner-generic-event-download.txt"), "download-ready");
        var traceDir = fixture.OutputPath("runner-event-traces");
        var script = fixture.CreateScript("runner-event-actions.cmgscript", $$"""
            test "runner event actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              captureConsole
              evaluate "console.warn('runner console ready'); true"
              waitForConsole "^runner console" match=regex level=warn timeout=5000
              expectNoConsole "not emitted" level=error timeout=10
              evaluate "console.info('runner generic console'); true"
              waitForEvent console message="runner generic console" level=info timeout=5000
              capturePageErrors
              evaluate "setTimeout(() => { throw new Error('runner page boom') }, 0); true"
              waitForPageError "runner page boom" timeout=5000
              expectNoPageError "not emitted" timeout=10
              waitForEvent pageError text="runner page boom" timeout=5000
              onDialog accept promptText="runner prompt"
              scrollIntoView "#dialog-prompt"
              click "#dialog-prompt"
              waitForDialog "fixture prompt" timeout=5000
              expectText "#status" "runner prompt"
              handleDialog dismiss
              click "#dialog-confirm"
              waitForEvent dialog message="fixture confirm" timeout=5000
              expectText "#status" "confirm dismissed"
              waitForEvent download directory="{{ScriptPath(fixture.OutputDirectory)}}" pattern="runner-generic-event-download.txt" timeout=1000
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner event actions");
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "CONSOLE_CAPTURE");
        AssertTraceContains(trace, "CONSOLE ");
        AssertTraceContains(trace, "CONSOLE_OK");
        AssertTraceContains(trace, "PAGE_ERROR_CAPTURE");
        AssertTraceContains(trace, "PAGE_ERROR ");
        AssertTraceContains(trace, "PAGE_ERROR_OK");
        AssertTraceContains(trace, "DIALOG_BEHAVIOR");
        AssertTraceContains(trace, "DIALOG ");
        AssertTraceContains(trace, "DOWNLOAD");
    }

    [Fact]
    public void RunCommand_EventActionFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-bad-event-action.cmgscript", $$"""
            test "runner event failure" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              waitForEvent console
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=waitForEvent");
        result.StderrContains("waitForEvent console requires a matcher");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
