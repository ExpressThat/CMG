using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerInterceptActionTests
{
    [Fact]
    public void RunText_InterceptInstallsNetworkRoute()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("intercept \"/api/profile\" status=200 body=\"ok\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgRoutes", client.LastExpression);
        Assert.Contains("ROUTE 001 /api/profile", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
