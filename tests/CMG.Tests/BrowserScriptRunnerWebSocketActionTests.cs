using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerWebSocketActionTests
{
    [Fact]
    public void RunText_RouteWebSocketInstallsPagePatch()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("routeWebSocket \"/socket\" message=ready close=true code=1000", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgWebSocketRoutes", client.LastExpression);
        Assert.Contains("ready", client.LastExpression);
        Assert.Contains("WEBSOCKET_ROUTE 001 /socket", result.StdoutLines);
    }

    [Fact]
    public void RunText_RouteWebSocketValidatesCloseOption()
    {
        var result = Runner().RunText("routeWebSocket \"/socket\" close=maybe", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("close= must be true or false", result.Error);
    }

    [Fact]
    public void RunText_WaitForWebSocketOutputsJsonLine()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/socket","routed":true}}""");

        var result = Runner().RunText("waitForWebSocket \"/socket\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgWebSockets", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("WEBSOCKET 001", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForWebSocketMessageReportsTimeout()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":false,"error":"Timed out waiting for websocket message ready"}""");

        var result = Runner().RunText("waitForWebSocketMessage ready timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Timed out waiting for websocket message ready", result.Error);
    }

    [Fact]
    public void RunText_ClearWebSocketRoutesOutputsParseableLine()
    {
        var result = Runner().RunText("clearWebSocketRoutes", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("WEBSOCKET_ROUTES_CLEARED 001", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
