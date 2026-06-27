using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserScriptImportE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserScriptImportE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ValidateScript_ExpandsRelativeImportsBeforeParsing()
    {
        fixture.CreateScript("shared-validate.cmgscript", """
            macro importedCaption value {
              caption "${value}"
            }
            """);
        var script = fixture.CreateScript("validate-import.cmgscript", """
            import "shared-validate.cmgscript"; call importedCaption "from import"
            """);

        var result = fixture.Cli.Run("browser", "control", "validateScript", "--file", script);

        result.ShouldPass();
        result.StdoutContains("SCRIPT VALID");
    }

    [Fact]
    public void ScriptCommand_ExecutesImportedMacroAndReturnedValue()
    {
        fixture.CreateScript("shared-direct.cmgscript", """
            macro writeImportedName value {
              fill "#name" "${value}"
              return { inputValue "#name" }
            }
            """);
        var script = fixture.CreateScript("direct-import.cmgscript", $$"""
            import "shared-direct.cmgscript"
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            set imported { call writeImportedName Ada }
            expectValue "#name" "Ada"
            expect ("${imported}" == "Ada")
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("SET ");
        result.StdoutContains("imported Ada");
        result.StdoutContains("EXPECT ");
    }

    [Fact]
    public void RunCommand_ExecutesImportedRunnerMacro()
    {
        fixture.CreateScript("shared-runner.cmgscript", """
            macro openFixture url {
              navigate "${url}" waitUntil=domcontentloaded
              expectText "#title" "CMG E2E Fixture"
            }
            """);
        var script = fixture.CreateScript("runner-import.cmgscript", $$"""
            import "shared-runner.cmgscript"
            test "uses imported macro" {
              call openFixture "{{fixture.FixtureHttpUri("index.html")}}"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldPass();
        result.StdoutContains("TEST PASS uses imported macro");
    }

    [Fact]
    public void ValidateScript_MissingImportFailsBeforeBrowserActions()
    {
        var script = fixture.CreateScript("missing-import.cmgscript", """
            import "not-here.cmgscript"
            click "#primary"
            """);

        var result = fixture.Cli.Run("browser", "control", "validateScript", "--file", script);

        result.ShouldFail();
        result.StderrContains("not-here.cmgscript");
    }
}
