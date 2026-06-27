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

    private CmgResult RunScript(string script)
    {
        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);
        result.ShouldPass();
        return result;
    }
}
