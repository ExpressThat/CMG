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
}
