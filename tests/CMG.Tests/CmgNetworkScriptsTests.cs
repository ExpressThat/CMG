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
    public void WaitForResponse_CanFilterByMethodStatusAndBody()
    {
        var action = Node("waitForResponse", ["/api"], new Dictionary<string, string>
        {
            ["method"] = "post",
            ["status"] = "201",
            ["contains"] = "created",
            ["mocked"] = "true"
        });
        var script = CmgNetworkScripts.WaitForResponse(action);

        Assert.Contains("method: 'POST'", script);
        Assert.Contains("status: 201", script);
        Assert.Contains("contains: 'created'", script);
        Assert.Contains("mocked: true", script);
    }

    [Fact]
    public void WaitForResponse_CanFilterByHeaderPair()
    {
        var action = Node("waitForResponse", ["/api"], new Dictionary<string, string> { ["header"] = "Content-Type: json" });
        var script = CmgNetworkScripts.WaitForResponse(action);

        Assert.Contains("headerName: 'content-type'", script);
        Assert.Contains("headerValue: 'json'", script);
        Assert.Contains("r.headers[expected.headerName]", script);
        Assert.Contains("header=content-type:json", script);
    }

    [Fact]
    public void WaitForResponse_CanMatchRegexUrlIgnoringCase()
    {
        var action = Node("waitForResponse", ["/api/\\d+"], new Dictionary<string, string>
        {
            ["match"] = "regex",
            ["ignoreCase"] = "true"
        });
        var script = CmgNetworkScripts.WaitForResponse(action);

        Assert.Contains("match: 'regex'", script);
        Assert.Contains("ignoreCase: true", script);
        Assert.Contains("new RegExp", script);
        Assert.Contains("match=regex, ignoreCase=true", script);
    }

    [Fact]
    public void WaitForRequest_CanMatchExactUrl()
    {
        var action = Node("waitForRequest", ["/api/profile"], new Dictionary<string, string> { ["match"] = "exact" });
        var script = CmgNetworkScripts.WaitForRequest(action);

        Assert.Contains("match: 'exact'", script);
        Assert.Contains("normalizeUrl(url) === expectedPattern", script);
        Assert.Contains("match=exact", script);
    }

    [Fact]
    public void WaitForRequest_CanFilterByHeaderNameAndValue()
    {
        var action = Node("waitForRequest", ["/api"], new Dictionary<string, string>
        {
            ["headerName"] = "Authorization",
            ["headerValue"] = "Bearer"
        });
        var script = CmgNetworkScripts.WaitForRequest(action);

        Assert.Contains("headerName: 'authorization'", script);
        Assert.Contains("headerValue: 'Bearer'", script);
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
