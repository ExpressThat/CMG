using CMG.Runner;
using System.Net;

namespace CMG.Tests;

public sealed class CmgApiRequestRunnerTests
{
    [Fact]
    public void Run_FailsWhenArgumentsAreMissing()
    {
        var result = new CmgApiRequestRunner().Run(Node("apiRequest", ["GET"]));

        Assert.False(result.Success);
        Assert.Contains("requires method and URL", result.Error);
    }

    [Fact]
    public void Run_FailsForInvalidUrlWithReason()
    {
        var result = new CmgApiRequestRunner().Run(Node("apiRequest", ["GET", "not a url"]));

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void Run_SendsQueryHeadersAndJsonBody()
    {
        HttpRequestMessage? request = null;
        var runner = new CmgApiRequestRunner(new HttpClient(new Handler(message =>
        {
            request = message;
            return new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("ok"), RequestMessage = message };
        })));

        var result = runner.Run(Node("apiRequest", ["POST", "https://example.test/items?existing=1"], new Dictionary<string, string>
        {
            ["query.search"] = "cmg value",
            ["header.Authorization"] = "Bearer token",
            ["json"] = """{"name":"demo"}""",
            ["status"] = "201",
            ["contains"] = "ok"
        }));

        Assert.True(result.Success);
        Assert.Equal("POST", request?.Method.Method);
        Assert.Contains("existing=1", request?.RequestUri?.Query);
        Assert.Contains("search=cmg%20value", request?.RequestUri?.Query);
        Assert.Equal("Bearer token", request?.Headers.Authorization?.ToString());
        Assert.Equal("application/json", request?.Content?.Headers.ContentType?.MediaType);
        Assert.Contains("API 003 201 https://example.test/items?", result.Output[0]);
    }

    [Fact]
    public void Run_ReturnsResponseOutputOnStatusFailure()
    {
        var runner = new CmgApiRequestRunner(new HttpClient(new Handler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("bad") })));

        var result = runner.Run(Node("apiRequest", ["GET", "https://example.test"], new Dictionary<string, string> { ["status"] = "200" }));

        Assert.False(result.Success);
        Assert.Contains("Expected status 200, got 400", result.Error);
        Assert.Contains("API_BODY 003 bad", result.Output);
    }

    [Fact]
    public void Run_RejectsInvalidTimeout()
    {
        var result = new CmgApiRequestRunner().Run(Node("apiRequest", ["GET", "https://example.test"], new Dictionary<string, string> { ["timeout"] = "never" }));

        Assert.False(result.Success);
        Assert.Contains("timeout= must be a positive number", result.Error);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyDictionary<string, string>? options = null) =>
        new(3, kind, kind, args, options ?? new Dictionary<string, string>(), []);

    private sealed class Handler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> handle;

        public Handler(Func<HttpRequestMessage, HttpResponseMessage> handle)
        {
            this.handle = handle;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(handle(request));

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) =>
            handle(request);
    }
}
