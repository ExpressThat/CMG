using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserCoverageActionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserCoverageActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_CoverageActionsWriteCoverageArtifact()
    {
        var coverage = fixture.OutputPath("runner-coverage.json");
        var script = fixture.CreateScript("runner-coverage.cmgscript", $$"""
            test "runner coverage actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              startCoverage js=true css=true
              evaluate "document.querySelector('#primary').click(); document.body.offsetWidth"
              stopCoverage path="{{ScriptPath(coverage)}}"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner coverage actions");
        CmgE2eAssert.FileExists(coverage);
        var json = File.ReadAllText(coverage);
        Assert.Contains("\"js\"", json, StringComparison.Ordinal);
        Assert.Contains("\"css\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_CoverageActionFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-coverage-failure.cmgscript", """
            test "runner coverage failure" {
              startCoverage js=maybe
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=startCoverage");
        result.StderrContains("startCoverage option js= must be true or false.");
    }

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
