using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserApiRequestActionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserApiRequestActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_ApiRequestSupportsQueryHeadersStatusAndOutput()
    {
        var output = fixture.OutputPath("script-api-response.json");
        var script = fixture.CreateScript("api-request-action.cmgscript", $$"""
            apiRequest "GET" "{{fixture.FixtureHttpPath("api/echo")}}" query.mode=script header.x-agent=cmg-script status=200 ok=true expectHeader.x-cmg-api=fixture contains="cmg-script" output="{{ScriptPath(output)}}"
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("API ");
        result.StdoutContains("API_BODY_FILE");
        CmgE2eAssert.FileExists(output);
        Assert.Contains("\"query\":\"mode=script\"", File.ReadAllText(output), StringComparison.Ordinal);
        Assert.Contains("\"xAgent\":\"cmg-script\"", File.ReadAllText(output), StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_ApiRequestRunsInsideTests()
    {
        var script = fixture.CreateScript("runner-api-request.cmgscript", $$"""
            test "api request action" {
              apiRequest "POST" "{{fixture.FixtureHttpPath("api/echo")}}" json="{\"name\":\"Runner\"}" status="200-299" ok=true contains="Runner"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldPass();
        result.StdoutContains("TEST PASS api request action");
    }

    [Fact]
    public void RunCommand_ApiRequestFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-api-request-failure.cmgscript", $$"""
            test "api request failure" {
              apiRequest "GET" "{{fixture.FixtureHttpPath("api/status/418")}}" status=200
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=apiRequest");
        result.StderrContains("Expected status 200, got 418.");
    }

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
