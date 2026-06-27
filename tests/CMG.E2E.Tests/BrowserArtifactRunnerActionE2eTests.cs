using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserArtifactRunnerActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserArtifactRunnerActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_PageArtifactFileAndTabActionsRunInsideTests()
    {
        Directory.CreateDirectory(fixture.OutputDirectory);
        var traceDir = fixture.OutputPath("runner-artifact-traces");
        var baseline = ScriptPath(fixture.OutputPath("runner-visible-baseline.png"));
        var actual = ScriptPath(fixture.OutputPath("runner-visible-actual.png"));
        var pageShot = ScriptPath(fixture.OutputPath("runner-page.jpeg"));
        var pdf = ScriptPath(fixture.OutputPath("runner-page.pdf"));
        var file = ScriptPath(fixture.OutputPath("runner-file.txt"));
        var script = fixture.CreateScript("runner-artifact-actions.cmgscript", $$"""
            test "runner artifact page and tab actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              title
              content
              textContent "#title"
              count ".item"
              allTextContents ".item"
              boundingBox "#visible-target"
              screenshot "#visible-target" output="{{baseline}}"
              expectScreenshot "#visible-target" baseline="{{baseline}}" output="{{actual}}" tolerance=0
              screenshotPage output="{{pageShot}}" type=jpeg quality=80 clipX=0 clipY=0 clipWidth=320 clipHeight=240
              printPdf path="{{pdf}}" format=A4 printBackground=true
              writeFile path="{{file}}" text="alpha"
              appendFile path="{{file}}" text="-beta"
              expectFile path="{{file}}" contains="alpha-beta"
              readFile fileBody path="{{file}}"
              expect ("${fileBody}" == "alpha-beta")
              listTabs
              openTab "{{fixture.FixtureHttpUri("index.html")}}"
              waitForTab count=2 timeout=5000
              activateTab index=1
              title
              closeTab index=1
              waitForTab count=1 timeout=5000
              setContent "<!doctype html><title>Runner Content</title><main id='runner-content'>updated</main>"
              expectText "#runner-content" "updated"
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner artifact page and tab actions");
        CmgE2eAssert.FileExists(fixture.OutputPath("runner-visible-baseline.png"));
        CmgE2eAssert.FileExists(fixture.OutputPath("runner-visible-actual.png"));
        CmgE2eAssert.FileExists(fixture.OutputPath("runner-page.jpeg"));
        CmgE2eAssert.FileExists(fixture.OutputPath("runner-page.pdf"));
        CmgE2eAssert.FileExists(fixture.OutputPath("runner-file.txt"));
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "TITLE");
        AssertTraceContains(trace, "CONTENT");
        AssertTraceContains(trace, "TEXT");
        AssertTraceContains(trace, "COUNT");
        AssertTraceContains(trace, "TEXTS");
        AssertTraceContains(trace, "BOUNDING_BOX");
        AssertTraceContains(trace, "SCREENSHOT");
        AssertTraceContains(trace, "VISUAL");
        AssertTraceContains(trace, "PDF");
        AssertTraceContains(trace, "FILE_WRITTEN");
        AssertTraceContains(trace, "FILE_APPENDED");
        AssertTraceContains(trace, "FILE_OK");
        AssertTraceContains(trace, "FILE_READ");
        AssertTraceContains(trace, "TAB_OPENED");
        AssertTraceContains(trace, "TAB_COUNT");
        AssertTraceContains(trace, "CONTENT_SET");
    }

    [Fact]
    public void RunCommand_FileArtifactFailureReportsStepReason()
    {
        var missing = ScriptPath(fixture.OutputPath("missing-runner-file.txt"));
        var script = fixture.CreateScript("runner-artifact-failure.cmgscript", $$"""
            test "runner artifact failure" {
              expectFile path="{{missing}}" contains="anything"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=expectFile");
        result.StderrContains("Expected file");
        result.StderrContains("missing-runner-file.txt");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
