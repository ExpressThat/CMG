using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class ApiCommandE2eTests : IClassFixture<CmgCliFixture>
{
    private readonly CmgCliFixture fixture;

    public ApiCommandE2eTests(CmgCliFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void Request_CoversGetHeadersQueryStatusAndOutputFile()
    {
        using var server = FixtureServer();
        var output = fixture.OutputPath("api-response.json");

        var result = fixture.Cli.Run(
            "api",
            "request",
            "GET",
            server.UrlPath("api/echo"),
            "--query",
            "mode=e2e",
            "--header",
            "x-agent=cmg",
            "--ok",
            "--status-match",
            "200-299",
            "--expect-header",
            "x-cmg-api=fixture",
            "--contains",
            "\"xAgent\":\"cmg\"",
            "--output",
            output);

        result.ShouldPass();
        result.StdoutContains("API 001 200");
        result.StdoutContains("API_BODY_FILE 001");
        CmgE2eAssert.FileExists(output);
        Assert.Contains("\"query\":\"mode=e2e\"", File.ReadAllText(output), StringComparison.Ordinal);
    }

    [Fact]
    public void Request_CoversJsonFormRawBodyAndBasicAuth()
    {
        using var server = FixtureServer();
        RunPost(server, "--json", "{\"name\":\"Ada\"}", "--contains", "Ada");
        RunPost(server, "--form", "name=Ada", "--contains", "name=Ada");
        RunPost(server, "--body", "plain body", "--content-type", "text/plain", "--contains", "plain body");

        var auth = fixture.Cli.Run(
            "api",
            "request",
            "POST",
            server.UrlPath("api/echo"),
            "--auth",
            "user:pass",
            "--contains",
            "\"authorization\":\"user:pass\"");
        auth.ShouldPass();
    }

    [Fact]
    public void Request_FailurePathsExplainValidationAndTimeout()
    {
        using var server = FixtureServer();

        var ok = fixture.Cli.Run("api", "request", "GET", server.UrlPath("api/status/418"), "--ok");
        ok.ShouldFail();
        ok.StderrContains("Expected ok=true, got status 418.");

        var status = fixture.Cli.Run("api", "request", "GET", server.UrlPath("api/status/418"), "--status", "200");
        status.ShouldFail();
        status.StderrContains("Expected status 200, got 418.");

        var header = fixture.Cli.Run("api", "request", "GET", server.UrlPath("api/echo"), "--expect-header", "x-cmg-api=missing");
        header.ShouldFail();
        header.StderrContains("Expected response header 'x-cmg-api' to contain 'missing'.");

        var body = fixture.Cli.Run("api", "request", "GET", server.UrlPath("api/echo"), "--not-contains", "method");
        body.ShouldFail();
        body.StderrContains("Expected response body not to contain 'method'.");

        var invalidTimeout = fixture.Cli.Run("api", "request", "GET", server.UrlPath("api/echo"), "--timeout", "0");
        invalidTimeout.ShouldFail();
        invalidTimeout.StderrContains("apiRequest option timeout= must be a positive number of milliseconds.");

        var timeout = fixture.Cli.Run("api", "request", "GET", server.UrlPath("api/slow"), "--timeout", "25");
        timeout.ShouldFail();
        timeout.StderrContains("API request timed out.");
    }

    private void RunPost(StaticFixtureServer server, params string[] arguments)
    {
        var result = fixture.Cli.Run(["api", "request", "POST", server.UrlPath("api/echo"), .. arguments]);
        result.ShouldPass();
    }

    private static StaticFixtureServer FixtureServer() =>
        new(Path.Combine(E2ePaths.RepositoryRoot(), "tests", "CMG.E2E.Tests", "Fixtures"));
}
