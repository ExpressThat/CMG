using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserScriptE2eTests : IClassFixture<CmgBrowserFixture>
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
            setDefaultNavigationTimeout 3000
            setDefaultAssertionTimeout 3000
            setDefaultExpectTimeout 3000
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

    [Fact]
    public void ScriptCommand_BaseUrlAndEnvResolveRelativeNavigationAndVariables()
    {
        var script = fixture.CreateScript("script-base-url-env.cmgscript", """
            navigate "index.html" waitUntil=domcontentloaded
            expectText "#title" "${expectedTitle}"
            """);

        var result = fixture.Cli.Run(
            "browser",
            "control",
            "script",
            "--file",
            script,
            "--base-url",
            fixture.FixtureHttpPath("/"),
            "--env",
            "expectedTitle=CMG E2E Fixture");

        result.ShouldPass();
        result.StdoutContains("PASS 001 navigate index.html");
        result.StdoutContains("NAVIGATED 001");
        result.StdoutContains("index.html waitUntil=domcontentloaded");
        result.StdoutContains("PASS 002 expectText #title \"CMG E2E Fixture\"");
    }

    [Fact]
    public void ScriptCommand_CommandTimeoutDefaultsApplyToDirectActions()
    {
        var script = fixture.CreateScript("script-command-timeouts.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            evaluate "setTimeout(() => document.querySelector('#status').textContent = 'delayed assertion', 80); true"
            expectText "#status" "delayed assertion"
            evaluate "setTimeout(() => window.__cliTimeoutReady = true, 80); true"
            waitForFunction "window.__cliTimeoutReady === true"
            """);

        var result = fixture.Cli.Run(
            "browser",
            "control",
            "script",
            "--file",
            script,
            "--timeout",
            "1000",
            "--navigation-timeout",
            "5000",
            "--assertion-timeout",
            "1000");

        result.ShouldPass();
        result.StdoutContains("PASS 003 expectText #status \"delayed assertion\"");
        result.StdoutContains("FUNCTION 005");
    }

    [Fact]
    public void ScriptCommand_ReadsAndExecutesScriptFromStdin()
    {
        var result = fixture.Cli.RunWithInput(
            $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            click "#primary"
            expectText "#status" "clicked"
            """,
            "browser",
            "control",
            "script",
            "--file",
            "-");

        result.ShouldPass();
        result.StdoutContains("PASS 001 navigate");
        result.StdoutContains("PASS 002 click #primary");
        result.StdoutContains("PASS 003 expectText #status clicked");
    }
}
