using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserCoverageCommandE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserCoverageCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void CoverageCommands_WriteCoverageArtifactsAcrossCliInvocations()
    {
        Navigate();
        var coverage = fixture.OutputPath("cli-coverage.json");

        Run("browser", "control", "coverage", "start", "--js", "true", "--css", "true")
            .StdoutContains("COVERAGE_STARTED 001 js=true css=true");
        Run("browser", "control", "page", "evaluate", "document.querySelector('#primary').click(); document.body.offsetWidth");
        Run("browser", "control", "coverage", "stop", "--path", coverage)
            .StdoutContains(coverage);

        CmgE2eAssert.FileExists(coverage);
        var json = File.ReadAllText(coverage);
        Assert.Contains("\"js\"", json);
        Assert.Contains("\"css\"", json);
    }

    [Fact]
    public void CoverageAliasCommands_CanPrintCoverageJson()
    {
        Navigate();

        Run("browser", "control", "coverage", "startCoverage", "--js", "false", "--css", "false")
            .StdoutContains("COVERAGE_STARTED 001 js=false css=false");
        var result = Run("browser", "control", "coverage", "stopCoverage");

        result.StdoutContains("COVERAGE 001");
        result.StdoutContains("\"js\"");
        result.StdoutContains("\"css\"");
    }

    [Fact]
    public void CoverageCommandFailure_ReturnsInvalidBooleanReason()
    {
        var result = fixture.Cli.Run("browser", "control", "coverage", "start", "--js", "maybe");

        result.ShouldFail();
        result.StderrContains("must be true or false");
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
