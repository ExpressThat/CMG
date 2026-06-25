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
    public void Route_CanAbortMatchingRequests()
    {
        var action = Node("route", ["/api"], new Dictionary<string, string> { ["abort"] = "true", ["error"] = "offline" });
        var script = CmgNetworkScripts.Route(action);

        Assert.Contains("abort: true", script);
        Assert.Contains("__cmgRequestFailures.push", script);
        Assert.Contains("offline", script);
    }

    [Fact]
    public void Route_CanMatchMethodAndLimitUses()
    {
        var action = Node("route", ["/api"], new Dictionary<string, string> { ["method"] = "post", ["times"] = "1" });
        var script = CmgNetworkScripts.Route(action);

        Assert.Contains("method: 'POST'", script);
        Assert.Contains("times: 1", script);
        Assert.Contains("__cmgTakeRoute", script);
        Assert.Contains("splice", script);
    }

    [Fact]
    public void Route_CanDelayMatchingRequests()
    {
        var action = Node("route", ["/api"], new Dictionary<string, string> { ["delay"] = "250" });
        var script = CmgNetworkScripts.Route(action);

        Assert.Contains("delay: 250", script);
        Assert.Contains("__cmgDelay", script);
        Assert.Contains("Number(route.delay)", script);
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
    public void WaitForRequestFinished_ReportsTimeoutPattern()
    {
        var action = Node("waitForRequestFinished", ["/api"], new Dictionary<string, string> { ["timeout"] = "1000" });
        var script = CmgNetworkScripts.WaitForRequestFinished(action);

        Assert.Contains("__cmgResponses", script);
        Assert.Contains("1000", script);
        Assert.Contains("Timed out waiting for finished request", script);
    }

    [Fact]
    public void WaitForRequestFailed_ReportsTimeoutPattern()
    {
        var action = Node("waitForRequestFailed", ["/api"], new Dictionary<string, string> { ["timeout"] = "1000" });
        var script = CmgNetworkScripts.WaitForRequestFailed(action);

        Assert.Contains("__cmgRequestFailures", script);
        Assert.Contains("1000", script);
        Assert.Contains("Timed out waiting for failed request", script);
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
