using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserScriptE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserScriptE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ScriptCommand_ExecutesActionsAndWritesGifAndTrace()
    {
        var script = fixture.CreateScript("script-flow.cmgscript", $$"""
            navigate "{{E2ePaths.FixtureUri("index.html")}}"
            setDefaultTimeout 3000
            setDefaultAssertionTimeout 3000
            setViewport 1280 1400
            captureDialogs
            setDialogBehavior accept promptText="typed prompt"
            click "#primary"
            assertText "#status" "clicked"
            fill "#name" "Grace Hopper"
            expectValue "#name" "Grace Hopper"
            check "#agree"
            expectChecked "#agree"
            select "#plan" "team"
            hover "#hover-card"
            assertText "#hover-state" "hovered"
            click "#dialog-prompt"
            waitForDialog "fixture prompt"
            assertText "#status" "typed prompt"
            set itemCount { evaluateAll ".item" "elements => elements.length" }
            if (${itemCount} == 3) {
              caption "three items"
            } else {
              fail "expected three items"
            }
            macro clickPrimaryAgain {
              scrollIntoView "#primary"
              click "#primary"
              return { textContent "#counter" }
            }
            set clickedAgain { call clickPrimaryAgain }
            assertEval "document.querySelector('#counter').textContent" equals="2"
            try {
              fail "intentional"
            } catch error {
              caption "${error}"
            }
            """);
        var gif = fixture.OutputPath("script.gif");
        var trace = fixture.OutputPath("script.trace.json");

        var result = fixture.Cli.Run(
            "browser", "control", "script",
            "--file", script,
            "--gif", gif,
            "--trace", trace);

        result.ShouldPass();
        result.StdoutContains("GIF ");
        result.StdoutContains("TRACE ");
        CmgE2eAssert.FileExists(gif);
        CmgE2eAssert.FileExists(trace);
    }

    [Fact]
    public void ValidateScript_CatchesSyntaxBeforeBrowserUse()
    {
        var badScript = fixture.CreateScript("bad-script.cmgscript", "if (true) {\n");

        var result = fixture.Cli.Run("browser", "control", "validateScript", "--file", badScript);

        result.ShouldFail();
        Assert.True(result.Stderr.Length > 0 || result.Stdout.Length > 0);
    }
}
