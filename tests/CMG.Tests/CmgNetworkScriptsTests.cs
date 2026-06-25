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
        Assert.Contains("XMLHttpRequest", script);
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

    [Fact]
    public void WaitForRequest_ReportsTimeoutPattern()
    {
        var action = Node("waitForRequest", ["/api"], new Dictionary<string, string> { ["timeout"] = "1000" });
        var script = CmgNetworkScripts.WaitForRequest(action);

        Assert.Contains("__cmgRequests", script);
        Assert.Contains("1000", script);
        Assert.Contains("Timed out waiting for request", script);
    }

    [Fact]
    public void ExportHar_BuildsHarFromCapturedResponses()
    {
        var script = CmgNetworkScripts.ExportHar();

        Assert.Contains("log", script);
        Assert.Contains("__cmgResponses", script);
        Assert.Contains("content", script);
    }

    [Fact]
    public void ReplayHar_InstallsRoutesFromEntries()
    {
        var script = CmgNetworkScripts.ReplayHar("""{"log":{"entries":[{"request":{"url":"/api"},"response":{"status":200,"content":{"text":"ok"}}}]}}""");

        Assert.Contains("JSON.parse", script);
        Assert.Contains("__cmgRoutes.push", script);
        Assert.Contains("/api", script);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyDictionary<string, string> options) =>
        new(2, kind, kind, args, options, []);
}
