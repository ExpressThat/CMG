using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserRuntimeInjectionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserRuntimeInjectionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ScriptActions_InjectRuntimeSetupIntoRealPage()
    {
        var initFile = fixture.CreateScript("init-file.js", "window.__cmgInitFile = 'file';");
        var script = fixture.CreateScript("runtime-injection.cmgscript", $$"""
        addInitScript "window.__cmgInitInline = 'inline';"
        evaluateOnNewDocument path="{{ScriptPath(initFile)}}"
        navigate "{{fixture.FixtureHttpUri("index.html")}}"
        expectEval "window.__cmgInitInline" equals="inline"
        expectEval "window.__cmgInitFile" equals="file"
        exposeFunction cmgAdd "(a, b) => a + b"
        exposeBinding cmgSource "(source, value) => source.name + ':' + value"
        expectEval "window.cmgAdd(2, 5)" equals="7"
        expectEval "window.cmgSource('bound')" equals="cmgSource:bound"
        addScriptTag "window.__cmgRuntimeTagInline = 'script-inline';"
        addScriptTag path="{{ScriptPath(E2ePaths.FixtureFile("runtime-tag.js"))}}"
        addStyleTag "body { --cmg-style-tag-inline: style-inline; }"
        addStyleTag path="{{ScriptPath(E2ePaths.FixtureFile("runtime-style.css"))}}"
        expectEval "window.__cmgRuntimeTagInline" equals="script-inline"
        expectEval "window.__cmgRuntimeTagFromFile" equals="script-file"
        expectEval "getComputedStyle(document.body).getPropertyValue('--cmg-style-tag-inline').trim()" equals="style-inline"
        expectEval "getComputedStyle(document.body).getPropertyValue('--cmg-style-tag-file').trim()" equals="style-file"
        setContent "<main><h1 id='generated'>Generated</h1><button id='save' data-ready='yes'>Save</button></main>"
        html "#generated"
        content
        boundingBox "#save"
        expectText "#generated" "Generated"
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("INIT_SCRIPT");
        result.StdoutContains("EXPOSED_FUNCTION");
        result.StdoutContains("SCRIPT_TAG");
        result.StdoutContains("STYLE_TAG");
        result.StdoutContains("CONTENT_SET");
        result.StdoutContains("HTML");
        result.StdoutContains("BOUNDING_BOX");
    }

    [Fact]
    public void RunCommand_RuntimeSetupActionsRunInsideTests()
    {
        var initFile = fixture.CreateScript("runner-init-file.js", "window.__cmgRunnerInitFile = 'file';");
        var traceDir = fixture.OutputPath("runner-runtime-traces");
        var script = fixture.CreateScript("runner-runtime-injection.cmgscript", $$"""
            test "runner runtime setup actions" {
              addInitScript "window.__cmgRunnerInitInline = 'inline';"
              evaluateOnNewDocument path="{{ScriptPath(initFile)}}"
              navigate "{{fixture.FixtureHttpUri("index.html")}}"
              expectEval "window.__cmgRunnerInitInline" equals="inline"
              expectEval "window.__cmgRunnerInitFile" equals="file"
              exposeFunction cmgRunnerAdd "(a, b) => a + b"
              exposeBinding cmgRunnerSource "(source, value) => source.name + ':' + value"
              expectEval "window.cmgRunnerAdd(3, 4)" equals="7"
              expectEval "window.cmgRunnerSource('bound')" equals="cmgRunnerSource:bound"
              addScriptTag "window.__cmgRunnerRuntimeTagInline = 'script-inline';"
              addStyleTag "body { --cmg-runner-style-tag-inline: style-inline; }"
              expectEval "window.__cmgRunnerRuntimeTagInline" equals="script-inline"
              expectEval "getComputedStyle(document.body).getPropertyValue('--cmg-runner-style-tag-inline').trim()" equals="style-inline"
              setContent "<main><h1 id='runner-generated'>Generated</h1><button id='runner-save' data-ready='yes'>Save</button></main>"
              html "#runner-generated"
              content
              boundingBox "#runner-save"
              expectText "#runner-generated" "Generated"
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner runtime setup actions");
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        Assert.Contains("INIT_SCRIPT", trace, StringComparison.Ordinal);
        Assert.Contains("EXPOSED_FUNCTION", trace, StringComparison.Ordinal);
        Assert.Contains("SCRIPT_TAG", trace, StringComparison.Ordinal);
        Assert.Contains("STYLE_TAG", trace, StringComparison.Ordinal);
        Assert.Contains("CONTENT_SET", trace, StringComparison.Ordinal);
        Assert.Contains("BOUNDING_BOX", trace, StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_RuntimeSetupFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-runtime-failure.cmgscript", """
            test "runner runtime setup failure" {
              addInitScript
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=addInitScript");
        result.StderrContains("requires inline script text or path=<file>");
    }

    private static string ScriptPath(string path) => path.Replace("\\", "\\\\", StringComparison.Ordinal);
}
