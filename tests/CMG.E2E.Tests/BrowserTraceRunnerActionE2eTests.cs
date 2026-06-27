using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserTraceRunnerActionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserTraceRunnerActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_TraceActionsWritePartialTrace()
    {
        var trace = fixture.OutputPath("runner-partial.trace.json");
        var script = fixture.CreateScript("runner-partial-trace.cmgscript", $$"""
            test "runner partial trace" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              startTracing path="{{ScriptPath(trace)}}"
              title
              textContent "#title"
              stopTracing
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner partial trace");
        CmgE2eAssert.FileExists(trace);
        AssertTraceContains(trace, "\"name\": \"title\"");
        AssertTraceContains(trace, "\"name\": \"textContent\"");
    }

    [Fact]
    public void RunCommand_TraceActionFailureWritesPartialTrace()
    {
        var trace = fixture.OutputPath("runner-failed-partial.trace.json");
        var script = fixture.CreateScript("runner-failed-partial-trace.cmgscript", $$"""
            test "runner failed partial trace" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              startTracing path="{{ScriptPath(trace)}}"
              fail "runner trace failure reason"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("runner trace failure reason");
        CmgE2eAssert.FileExists(trace);
        AssertTraceContains(trace, "\"success\": false");
        AssertTraceContains(trace, "runner trace failure reason");
    }

    private static void AssertTraceContains(string path, string expected) =>
        Assert.Contains(expected, File.ReadAllText(path), StringComparison.Ordinal);

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
