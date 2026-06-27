using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class CmgRunE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public CmgRunE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_WritesReportsTracesAndOptionalGifs()
    {
        var testScript = fixture.CreateScript("runner-flow.cmgscript", $$"""
            suite "fixture suite" tag=smoke owner=e2e {
              beforeEach {
                navigate "{{E2ePaths.FixtureUri("index.html")}}"
                setViewport 1280 1400
              }

              test "clicks fixture button" {
                scrollIntoView "#primary"
                click "#primary"
                expectText "#status" "clicked"
              }

              test "uses variables and macros" {
                macro writeName value {
                  fill "#name" "${value}"
                  return { inputValue "#name" }
                }
                set result { call writeName ${userName} }
                if (${result} == "Ada") {
                  expectValue "#name" "Ada"
                } else {
                  fail "unexpected name"
                }
              }
            }
            """);
        var json = fixture.OutputPath("run.json");
        var html = fixture.OutputPath("run.html");
        var junit = fixture.OutputPath("run.xml");
        var traceDir = fixture.OutputPath("traces");
        var gifDir = fixture.OutputPath("gifs");

        var result = fixture.Cli.Run(
            "run", testScript,
            "--tag", "smoke",
            "--var", "userName=Ada",
            "--report-json", json,
            "--report-html", html,
            "--report-junit", junit,
            "--trace", traceDir,
            "--gif", gifDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS fixture suite / clicks fixture button");
        result.StdoutContains("TEST PASS fixture suite / uses variables and macros");
        CmgE2eAssert.FileExists(json);
        CmgE2eAssert.FileExists(html);
        CmgE2eAssert.FileExists(junit);
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.json");
        CmgE2eAssert.DirectoryHasFiles(gifDir, "*.gif");
    }

    [Fact]
    public void RunCommand_ListDoesNotStartBrowserActions()
    {
        var testScript = fixture.CreateScript("runner-list.cmgscript", """
            test "listed only" tag=list {
              fail "should not run"
            }
            """);

        var result = fixture.Cli.Run("run", testScript, "--tag", "list", "--list");

        result.ShouldPass();
        result.StdoutContains("TEST LIST run listed only");
    }
}
