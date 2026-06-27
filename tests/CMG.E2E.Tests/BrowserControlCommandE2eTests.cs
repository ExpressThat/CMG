using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserControlCommandE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserControlCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void NavigationRuntimeAndInputCommands_DriveFixturePage()
    {
        Navigate();

        var title = fixture.Cli.Run("browser", "control", "page", "runtime", "textContent", "#title");
        title.ShouldPass();
        title.StdoutContains("TEXT 001 CMG E2E Fixture");

        var fill = fixture.Cli.Run("browser", "control", "input", "fill", "#name", "Ada Lovelace");
        fill.ShouldPass();

        var value = fixture.Cli.Run("browser", "control", "assertions", "expectValue", "#name", "Ada Lovelace");
        value.ShouldPass();

        fixture.Cli.Run("browser", "control", "input", "scrollIntoView", "#primary").ShouldPass();
        var click = fixture.Cli.Run("browser", "control", "input", "click", "#primary");
        click.ShouldPass();

        var text = fixture.Cli.Run("browser", "control", "assertions", "expectText", "#status", "clicked");
        text.ShouldPass();
    }

    [Fact]
    public void CaptureCommands_WriteArtifacts()
    {
        Navigate();
        var screenshot = fixture.OutputPath("page.png");

        var pageShot = fixture.Cli.Run("browser", "control", "capture", "screenshotPage", "--output", screenshot);
        pageShot.ShouldPass();
        CmgE2eAssert.FileExists(screenshot);

        var elementHtml = fixture.Cli.Run("browser", "control", "capture", "getElement", "#title", "--html");
        elementHtml.ShouldPass();
        elementHtml.StdoutContains("CMG E2E Fixture");
    }

    [Fact]
    public void NavigationAliasesAndPageStateCommands_RunAgainstBrowser()
    {
        var first = fixture.FixtureHttpUri("index.html");
        var second = $"{first}?step=two";

        Run("browser", "control", "navigation", "goto", first, "--wait-until", "domcontentloaded");
        Run("browser", "control", "navigation", "waitForUrl", "index.html", "--match", "contains");
        Run("browser", "control", "navigation", "waitForTitle", "CMG E2E Fixture", "--match", "exact");
        Run("browser", "control", "navigation", "toHaveURL", "INDEX.HTML", "--ignore-case");
        Run("browser", "control", "navigation", "toHaveTitle", "CMG E2E Fixture", "--match", "exact");
        Run("browser", "control", "navigation", "url").StdoutContains("index.html");
        Run("browser", "control", "navigation", "title").StdoutContains("CMG E2E Fixture");
        Run("browser", "control", "navigation", "content").StdoutContains("Primary action");
        Run("browser", "control", "navigation", "visit", second, "--wait-until", "domcontentloaded");
        Run("browser", "control", "navigation", "goBack", "--wait-until", "domcontentloaded");
        Run("browser", "control", "navigation", "goForward", "--wait-until", "domcontentloaded");
        Run("browser", "control", "navigation", "reload", "--wait-until", "domcontentloaded");
        Run("browser", "control", "navigation", "waitForLoadState", "complete");
        Run("browser", "control", "navigation", "networkIdle", "--timeout", "3000");
        Run("browser", "control", "navigation", "setContent", "<title>CMG Inline</title><main id='inline'>Ready</main>");
        Run("browser", "control", "navigation", "expectTitle", "CMG Inline", "--match", "exact");
        Run("browser", "control", "page", "runtime", "textContent", "#inline").StdoutContains("Ready");
    }

    [Fact]
    public void ValidationFailure_ReturnsUsefulReason()
    {
        var result = fixture.Cli.Run("browser", "control", "assertions", "expectText", "#missing", "never", "--timeout", "10");

        result.ShouldFail();
        Assert.True(result.Stderr.Contains("#missing") || result.Stdout.Contains("#missing"));
    }

    private void Navigate()
    {
        var result = fixture.Cli.Run("browser", "control", "navigation", "navigate", E2ePaths.FixtureFile("index.html"), "--wait-until", "domcontentloaded");
        result.ShouldPass();
        result.StdoutContains("NAVIGATED 001");
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }
}
