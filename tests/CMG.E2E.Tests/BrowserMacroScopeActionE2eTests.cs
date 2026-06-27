using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserMacroScopeActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserMacroScopeActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_MacrosSetCaptureAndScopesRunAgainstBrowser()
    {
        var script = fixture.CreateScript("macro-scope-actions.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
        set outer "global"
        set local "root"
        macro parent suffix {
          set local "inner"
          macro child value {
            return "${outer}-${local}-${value}"
          }
          return { call child "${suffix}" }
        }
        set result { call parent "leaf" }
        expect ("${result}" == "global-inner-leaf")
        expect ("${local}" == "root")
        set titleValue { title }
        expect ("${titleValue}" == "CMG E2E Fixture")
        set countValue { count ".item" }
        expect ("${countValue}" == "3")
        set inputValue { return { fill "#name" "${result}"; inputValue "#name" } }
        expect ("${inputValue}" == "global-inner-leaf")
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("SET ");
        result.StdoutContains("RETURN ");
        result.StdoutContains("EXPECT ");
    }

    [Fact]
    public void RunCommand_MacrosSetCaptureAndScopesRunInsideTests()
    {
        var traceDir = fixture.OutputPath("runner-macro-scope-traces");
        var script = fixture.CreateScript("runner-macro-scope-actions.cmgscript", $$"""
            test "runner macro scope actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              set outer "runner"
              set local "root"
              macro parent suffix {
                set local "inner"
                macro child value {
                  return "${outer}-${local}-${value}"
                }
                return { call child "${suffix}" }
              }
              set result { call parent "leaf" }
              expect ("${result}" == "runner-inner-leaf")
              expect ("${local}" == "root")
              set titleValue { title }
              expect ("${titleValue}" == "CMG E2E Fixture")
              set inputValue { return { fill "#name" "${result}"; inputValue "#name" } }
              expect ("${inputValue}" == "runner-inner-leaf")
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner macro scope actions");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "SET");
        AssertTraceContains(trace, "RETURN");
        AssertTraceContains(trace, "EXPECT");
    }

    [Fact]
    public void RunCommand_MacroFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-macro-failure.cmgscript", """
            test "runner macro failure" {
              call missingMacro
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=call");
        result.StderrContains("missingMacro");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);
}
