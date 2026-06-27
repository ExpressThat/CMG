using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserRuntimeInjectionE2eTests
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

    private static string ScriptPath(string path) => path.Replace("\\", "\\\\", StringComparison.Ordinal);
}
