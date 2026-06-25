using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerNetworkTests
{
    [Fact]
    public void RunText_RouteInstallsNetworkPatch()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("route \"/api\" status=201 body=ok", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgRoutes", client.LastExpression);
        Assert.Contains("ROUTE", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_ClearRoutesOutputsParseableLine()
    {
        var result = Runner().RunText("clearRoutes", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("ROUTES_CLEARED", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForResponseOutputsResponseLine()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("waitForResponse \"/api\" timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("waitForResponse", result.StdoutLines[0]);
        Assert.Contains(result.StdoutLines, line => line.Contains("RESPONSE", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
