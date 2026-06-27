using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserTabCommandE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserTabCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void TabAliasCommands_RunAgainstRealBrowserTargets()
    {
        Navigate();
        var target = fixture.FixtureHttpUri("index.html");

        Run("browser", "control", "tabs", "listTabs").StdoutContains("TAB 0");
        Run("browser", "control", "tabs", "openTab", target).StdoutContains("TAB_OPENED");
        Run("browser", "control", "tabs", "waitForTab", "--count", "2", "--timeout", "5000")
            .StdoutContains("TAB_COUNT");
        Run("browser", "control", "tabs", "waitForPopup", "--count", "2", "--timeout", "5000")
            .StdoutContains("TAB_COUNT");
        Run("browser", "control", "tabs", "activateTab", "--index", "1");
        Run("browser", "control", "navigation", "title").StdoutContains("CMG E2E Fixture");
        Run("browser", "control", "tabs", "closeTab", "--index", "1");
        Run("browser", "control", "tabs", "wait", "--count", "1", "--timeout", "5000")
            .StdoutContains("TAB_COUNT");
    }

    [Fact]
    public void TabWaitFailure_ReturnsExpectedCountReason()
    {
        Navigate();

        var result = fixture.Cli.Run("browser", "control", "tabs", "waitForTab", "--count", "3", "--timeout", "20");

        result.ShouldFail();
        result.StderrContains("Expected at least 3 tab(s)");
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
