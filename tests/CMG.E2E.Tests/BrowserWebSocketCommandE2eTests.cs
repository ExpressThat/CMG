using System.Text.Json;
using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserWebSocketCommandE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserWebSocketCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void WebSocketCommands_RouteWaitForSocketAndMessageAgainstRealConnection()
    {
        Navigate();
        var socketUrl = fixture.FixtureWebSocketUri("socket");

        Run("browser", "control", "network", "webSocket", "route", "socket", "--message", "route ready");
        Evaluate($"window.__cmgSocket = new WebSocket({JsonSerializer.Serialize(socketUrl)}); true");
        Run("browser", "control", "network", "webSocket", "wait", "socket", "--timeout", "5000")
            .StdoutContains("WEBSOCKET");
        Run("browser", "control", "events", "wait", "websocket", "socket", "--timeout", "5000")
            .StdoutContains("WEBSOCKET");
        Run("browser", "control", "network", "webSocket", "waitMessage", "route ready", "--timeout", "5000")
            .StdoutContains("route ready");
        Run("browser", "control", "events", "wait", "websocketMessage", "route ready", "--timeout", "5000")
            .StdoutContains("route ready");
        Run("browser", "control", "network", "webSocket", "clearRoutes");
    }

    private CmgResult Evaluate(string expression) =>
        Run("browser", "control", "page", "evaluate", expression);

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
}
