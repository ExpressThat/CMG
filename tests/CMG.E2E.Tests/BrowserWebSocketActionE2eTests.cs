using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserWebSocketActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserWebSocketActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_WebSocketActionsRouteWaitAndClearAgainstRealConnection()
    {
        var socketUrl = fixture.FixtureWebSocketUri("socket");
        var script = fixture.CreateScript("websocket-actions.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            routeWebSocket "socket" message="script route ready"
            evaluate "window.__cmgScriptSocket = new WebSocket('{{socketUrl}}'); true"
            waitForWebSocket "socket" timeout=5000
            waitForWebSocketMessage "script route ready" timeout=5000
            waitForEvent websocket "socket" timeout=5000
            waitForEvent websocketMessage "script route ready" timeout=5000
            clearWebSocketRoutes
            """);

        var result = RunScript(script);

        result.StdoutContains("WEBSOCKET_ROUTE");
        result.StdoutContains("WEBSOCKET ");
        result.StdoutContains("WEBSOCKET_MESSAGE");
        result.StdoutContains("script route ready");
        result.StdoutContains("WEBSOCKET_ROUTES_CLEARED");
    }

    [Fact]
    public void DirectScript_WebSocketActionsReportValidationFailure()
    {
        var script = fixture.CreateScript("websocket-invalid.cmgscript", """
            routeWebSocket "socket" close=maybe
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StderrContains("routeWebSocket option close= must be true or false.");
    }

    [Fact]
    public void RunCommand_WebSocketActionsRunInsideTests()
    {
        var socketUrl = fixture.FixtureWebSocketUri("runner-socket");
        var traceDir = fixture.OutputPath("runner-websocket-traces");
        var script = fixture.CreateScript("runner-websocket-actions.cmgscript", $$"""
            test "runner websocket actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              routeWebSocket "runner-socket" message="runner route ready"
              evaluate "window.__cmgRunnerSocket = new WebSocket('{{socketUrl}}'); true"
              waitForWebSocket "runner-socket" timeout=5000
              waitForWebSocketMessage "runner route ready" timeout=5000
              waitForEvent websocket "runner-socket" timeout=5000
              waitForEvent websocketMessage "runner route ready" timeout=5000
              clearWebSocketRoutes
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner websocket actions");
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        Assert.Contains("WEBSOCKET_ROUTE", trace, StringComparison.Ordinal);
        Assert.Contains("runner route ready", trace, StringComparison.Ordinal);
        Assert.Contains("WEBSOCKET_ROUTES_CLEARED", trace, StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_WebSocketActionFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-websocket-invalid.cmgscript", """
            test "runner websocket failure" {
              routeWebSocket "socket" close=maybe
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=routeWebSocket");
        result.StderrContains("routeWebSocket option close= must be true or false.");
    }

    private CmgResult RunScript(string script)
    {
        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);
        result.ShouldPass();
        return result;
    }
}
