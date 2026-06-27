using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserActionSurfaceE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserActionSurfaceE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_CoversBroadBrowserActionSurface()
    {
        var trace = fixture.OutputPath("action-surface.trace.json");
        var result = fixture.Cli.Run(
            "browser", "control", "script",
            "--file", E2ePaths.FixtureFile("action-surface.cmgscript"),
            "--trace", trace,
            "--var", $"fixtureUrl={E2ePaths.FixtureUri("index.html")}",
            "--var", $"uploadPath={E2ePaths.FixtureFile("upload-one.txt")}");

        result.ShouldPass();
        result.StdoutContains("TRACE ");
        result.StdoutContains("switch matched");
        CmgE2eAssert.FileExists(trace);
    }
}
