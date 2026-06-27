using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserNavigationRuntimeAliasE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserNavigationRuntimeAliasE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void NavigationLeafAliases_RunAgainstBrowser()
    {
        var url = fixture.FixtureHttpUri("index.html") + "?alias=nav";
        Run("browser", "control", "navigation", "navigate", url, "--wait-until", "domcontentloaded");

        Run("browser", "control", "navigation", "expectUrl", "ALIAS=NAV", "--ignore-case")
            .StdoutContains("URL 001");
        Run("browser", "control", "navigation", "waitForNavigation", "alias=nav", "--wait-until", "domcontentloaded", "--timeout", "5000")
            .StdoutContains("NAVIGATION 001");
        Run("browser", "control", "navigation", "waitForNetworkIdle", "--timeout", "5000")
            .StdoutContains("NETWORK_IDLE 001");

        var missing = fixture.Cli.Run("browser", "control", "navigation", "expectUrl", "definitely-missing", "--match", "exact");
        missing.ShouldFail();
        missing.StderrContains("definitely-missing");
    }

    [Fact]
    public void RuntimeSelectorEvaluationAliases_RunAgainstBrowser()
    {
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");

        Run("browser", "control", "page", "runtime", "evalOnSelector", "#title", "element.textContent")
            .StdoutContains("CMG E2E Fixture");
        Run("browser", "control", "page", "runtime", "evalAll", ".item", "elements => elements.map(e => e.textContent).join('|')")
            .StdoutContains("Alpha|Beta|Gamma");

        var missing = fixture.Cli.Run("browser", "control", "page", "runtime", "evalAll", ".not-real", "elements => elements.length");
        missing.ShouldFail();
        missing.StderrContains(".not-real");
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }
}
