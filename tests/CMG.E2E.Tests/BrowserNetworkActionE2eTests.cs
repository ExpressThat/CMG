using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserNetworkActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserNetworkActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_NetworkActionsRouteWaitHarAndAbortAgainstRealPage()
    {
        var har = fixture.OutputPath("script-network.har");
        var replayHar = ReplayHar("script");
        var script = fixture.CreateScript("network-actions.cmgscript", Script("script", har, replayHar));

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        AssertOutput(result.Stdout);
        CmgE2eAssert.FileExists(har);
    }

    [Fact]
    public void RunCommand_NetworkActionsRunInsideTests()
    {
        var har = fixture.OutputPath("runner-network.har");
        var replayHar = ReplayHar("runner");
        var traceDir = fixture.OutputPath("runner-network-traces");
        var script = fixture.CreateScript("runner-network-actions.cmgscript", $$"""
            test "runner network actions" {
            {{Indent(Script("runner", har, replayHar))}}
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner network actions");
        CmgE2eAssert.FileExists(har);
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertOutput(trace);
    }

    [Fact]
    public void RunCommand_NetworkActionFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-network-failure.cmgscript", """
            test "runner network failure" {
              waitForResponse "missing-network-response" timeout=10
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=waitForResponse");
        result.StderrContains("Timed out waiting for response missing-network-response");
    }

    private string Script(string mode, string har, string replayHar) => $$"""
          navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
          route "{{mode}}-mock.json" status=201 body="{\"mode\":\"{{mode}}\"}" contentType="application/json" header="x-cmg: {{mode}}"
          evaluate "fetch('/{{mode}}-mock.json').then(r => r.text())"
          waitForRequest "{{mode}}-mock.json" method=GET timeout=5000
          waitForResponse "{{mode}}-mock.json" status=201 mocked=true contains="{{mode}}" header="x-cmg: {{mode}}" timeout=5000
          waitForRequestFinished "{{mode}}-mock.json" timeout=5000
          exportHar path="{{ScriptPath(har)}}"
          clearRoutes
          replayHar path="{{ScriptPath(replayHar)}}"
          evaluate "fetch('http://cmg.local/{{mode}}-replay.json').then(async r => r.status + ':' + await r.text())"
          waitForResponse "{{mode}}-replay.json" status=202 contains="replayed-{{mode}}" mocked=true timeout=5000
          route "{{mode}}-abort" abort=true
          evaluate "fetch('/{{mode}}-abort').catch(error => error.message)"
          waitForRequestFailed "{{mode}}-abort" mocked=true timeout=5000
          clearRoutes
        """;

    private string ReplayHar(string mode)
    {
        var body = $$"""{"value":"replayed-{{mode}}"}""";
        var json = "{" +
            "\"log\":{\"entries\":[{\"request\":{\"url\":\"http://cmg.local/" + mode + "-replay.json\"}," +
            "\"response\":{\"status\":202,\"content\":{\"mimeType\":\"application/json\",\"text\":\"" + Escape(body) + "\"}}}]}}";
        return fixture.CreateScript($"{mode}-replay.har", json);
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    private static void AssertOutput(string output)
    {
        Assert.Contains("ROUTE", output, StringComparison.Ordinal);
        Assert.Contains("REQUEST", output, StringComparison.Ordinal);
        Assert.Contains("RESPONSE", output, StringComparison.Ordinal);
        Assert.Contains("REQUEST_FINISHED", output, StringComparison.Ordinal);
        Assert.Contains("HAR_EXPORTED", output, StringComparison.Ordinal);
        Assert.Contains("HAR_REPLAY", output, StringComparison.Ordinal);
        Assert.Contains("REQUEST_FAILED", output, StringComparison.Ordinal);
        Assert.Contains("ROUTES_CLEARED", output, StringComparison.Ordinal);
    }

    private static string Indent(string text) =>
        string.Join(Environment.NewLine, text.Split(Environment.NewLine).Select(line => "  " + line));

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
