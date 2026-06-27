using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserWaitCommandE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserWaitCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void WaitCommands_RunAgainstDynamicBrowserState()
    {
        Navigate();

        Run("browser", "control", "wait", "element", "#primary", "--timeout", "1000");
        Run("browser", "control", "wait", "selector", "#primary", "--state", "attached", "--timeout", "1000")
            .StdoutContains("state=attached");
        Run("browser", "control", "wait", "selector", "#primary", "--state", "visible", "--timeout", "1000")
            .StdoutContains("state=visible");
        Run("browser", "control", "wait", "selector", "#hidden-target", "--state", "hidden", "--timeout", "1000")
            .StdoutContains("state=hidden");
        Run("browser", "control", "wait", "selector", "#never-attached", "--state", "detached", "--timeout", "1000")
            .StdoutContains("state=detached");

        Run("browser", "control", "wait", "function", "document.readyState === 'complete'", "--timeout", "1000")
            .StdoutContains("FUNCTION");
        Run("browser", "control", "page", "evaluate", "setTimeout(() => window.cmgDelayedReady = true, 50); true");
        Run("browser", "control", "wait", "waitForFunction", "window.cmgDelayedReady === true", "--timeout", "1000")
            .StdoutContains("true");
        Run("browser", "control", "wait", "timeout", "1").StdoutContains("WAIT_TIMEOUT");
        Run("browser", "control", "wait", "waitForTimeout", "1").StdoutContains("WAIT_TIMEOUT");
        Run("browser", "control", "wait", "auto", "1").StdoutContains("PASS 001 wait 1");
        Run("browser", "control", "wait", "auto", "#primary", "--timeout", "1000").StdoutContains("PASS 001 wait #primary");
        Run("browser", "control", "wait", "waitForElement", "#primary", "--timeout", "1000");
        Run("browser", "control", "wait", "waitForSelector", "#primary", "--state", "visible", "--timeout", "1000")
            .StdoutContains("state=visible");
    }

    [Fact]
    public void WaitCommandFailure_ReturnsSelectorStateReason()
    {
        Navigate();

        var result = fixture.Cli.Run("browser", "control", "wait", "selector", "#hidden-target", "--state", "visible", "--timeout", "20");

        result.ShouldFail();
        result.StderrContains("did not reach state visible");
        result.StderrContains("attached=true, visible=false");
    }

    private void Navigate()
    {
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }
}
