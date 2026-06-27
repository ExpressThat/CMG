using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserTraceActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserTraceActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ScriptTraceActions_WritePartialTraceAgainstRealBrowser()
    {
        var trace = fixture.OutputPath("partial-script.trace.json");
        var script = fixture.CreateScript("partial-trace.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}"
        startTracing path="{{ScriptPath(trace)}}"
        title
        textContent "#title"
        stopTracing
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("TRACE_STARTED 002");
        result.StdoutContains("TRACE 005");
        CmgE2eAssert.FileExists(trace);
        AssertTraceContains(trace, "\"name\": \"title\"");
        AssertTraceContains(trace, "\"name\": \"textContent\"");
    }

    [Fact]
    public void CommandTrace_SuppressesNestedTraceBlocks()
    {
        var nestedTrace = fixture.OutputPath("nested-should-not-exist.trace.json");
        var commandTrace = fixture.OutputPath("command-script.trace.json");
        var script = fixture.CreateScript("suppressed-nested-trace.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}"
        startTracing path="{{ScriptPath(nestedTrace)}}"
        title
        stopTracing
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script, "--trace", commandTrace);

        result.ShouldPass();
        result.StdoutContains("TRACE_BLOCK_SUPPRESSED 002");
        result.StdoutContains("TRACE_BLOCK_SUPPRESSED 004");
        result.StdoutContains("TRACE ");
        CmgE2eAssert.FileExists(commandTrace);
        Assert.False(File.Exists(nestedTrace));
        AssertTraceContains(commandTrace, "\"name\": \"title\"");
    }

    [Fact]
    public void TraceActionFailure_WritesPartialFailureTrace()
    {
        var trace = fixture.OutputPath("failed-partial-script.trace.json");
        var script = fixture.CreateScript("failed-partial-trace.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}"
        startTracing path="{{ScriptPath(trace)}}"
        fail "trace failure reason"
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StderrContains("trace failure reason");
        result.StdoutContains("TRACE ");
        CmgE2eAssert.FileExists(trace);
        AssertTraceContains(trace, "\"success\": false");
        AssertTraceContains(trace, "trace failure reason");
    }

    private static void AssertTraceContains(string path, string expected) =>
        Assert.Contains(expected, File.ReadAllText(path), StringComparison.Ordinal);

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
