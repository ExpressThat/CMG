using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgNetworkScriptsTests
{
    [Fact]
    public void Route_BuildsFetchPatchWithMockBody()
    {
        var action = Node("route", ["/api"], new Dictionary<string, string> { ["status"] = "201", ["body"] = "ok" });
        var script = CmgNetworkScripts.Route(action);

        Assert.Contains("__cmgRoutes", script);
        Assert.Contains("201", script);
        Assert.Contains("ok", script);
    }

    [Fact]
    public void WaitForResponse_ReportsTimeoutPattern()
    {
        var action = Node("waitForResponse", ["/api"], new Dictionary<string, string> { ["timeout"] = "1000" });
        var script = CmgNetworkScripts.WaitForResponse(action);

        Assert.Contains("__cmgResponses", script);
        Assert.Contains("1000", script);
        Assert.Contains("/api", script);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyDictionary<string, string> options) =>
        new(2, kind, kind, args, options, []);
}
