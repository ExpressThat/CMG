using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserArtifactActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserArtifactActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_CoversArtifactStorageTabAndCoverageActions()
    {
        var script = fixture.CreateScript("artifact-actions.cmgscript", $$"""
        navigate "{{ScriptPath(E2ePaths.FixtureUri("index.html"))}}"
        scrollIntoView "#primary"
        click "#primary"
        screenshot "#visible-target" output="{{ScriptPath(fixture.OutputPath("visible-baseline.png"))}}"
        expectScreenshot "#visible-target" baseline="{{ScriptPath(fixture.OutputPath("visible-baseline.png"))}}" output="{{ScriptPath(fixture.OutputPath("visible-actual.png"))}}" tolerance=0
        screenshotPage output="{{ScriptPath(fixture.OutputPath("page.jpeg"))}}" type=jpeg quality=80 clipX=0 clipY=0 clipWidth=320 clipHeight=240
        printPdf path="{{ScriptPath(fixture.OutputPath("fixture.pdf"))}}" format=A4 printBackground=true
        storageState save path="{{ScriptPath(fixture.OutputPath("storage.json"))}}"
        localStorage set "cmg-e2e" "restored"
        storageState load path="{{ScriptPath(fixture.OutputPath("storage.json"))}}"
        expectEval "localStorage.getItem('cmg-e2e')" equals="clicked"
        writeFile path="{{ScriptPath(fixture.OutputPath("script-file.txt"))}}" text="alpha"
        appendFile path="{{ScriptPath(fixture.OutputPath("script-file.txt"))}}" text="-beta"
        expectFile path="{{ScriptPath(fixture.OutputPath("script-file.txt"))}}" contains="alpha-beta"
        readFile fileBody path="{{ScriptPath(fixture.OutputPath("script-file.txt"))}}"
        if (${fileBody} == "alpha-beta") {
          caption "file roundtrip"
        } else {
          fail "file roundtrip failed"
        }
        startCoverage js=true css=true
        evaluate "document.querySelector('#primary').click()"
        stopCoverage path="{{ScriptPath(fixture.OutputPath("coverage.json"))}}"
        listTabs
        openTab "{{ScriptPath(E2ePaths.FixtureUri("index.html"))}}"
        waitForTab count=2 timeout=5000
        activateTab index=1
        title
        closeTab index=1
        waitForTab count=1 timeout=5000
        """);

        var trace = fixture.OutputPath("artifact-actions.trace.json");
        var result = fixture.Cli.Run("browser", "control", "script", "--file", script, "--trace", trace);

        result.ShouldPass();
        result.StdoutContains("VISUAL");
        result.StdoutContains("PDF");
        result.StdoutContains("STORAGE_STATE");
        result.StdoutContains("COVERAGE");
        CmgE2eAssert.FileExists(trace);
        CmgE2eAssert.FileExists(fixture.OutputPath("fixture.pdf"));
        CmgE2eAssert.FileExists(fixture.OutputPath("storage.json"));
        CmgE2eAssert.FileExists(fixture.OutputPath("coverage.json"));
        CmgE2eAssert.FileExists(fixture.OutputPath("script-file.txt"));
    }

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
