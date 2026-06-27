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
    public void RunText_RouteSupportsAbortOption()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("intercept \"/api\" abort=true error=offline", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("abort: true", client.LastExpression);
        Assert.Contains("offline", client.LastExpression);
        Assert.Contains("ROUTE", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_RouteSupportsMethodAndTimesOptions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("route \"/api\" method=POST times=1", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("method: 'POST'", client.LastExpression);
        Assert.Contains("times: 1", client.LastExpression);
    }

    [Fact]
    public void RunText_RouteRejectsInvalidTimesOption()
    {
        var result = Runner().RunText("route \"/api\" times=never", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("times= must be a positive integer", result.Error);
    }

    [Fact]
    public void RunText_RouteSupportsDelayOption()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("route \"/api\" delay=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("delay: 250", client.LastExpression);
        Assert.Contains("__cmgDelay", client.LastExpression);
    }

    [Fact]
    public void RunText_RouteRejectsInvalidDelayOption()
    {
        var result = Runner().RunText("route \"/api\" delay=never", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("delay= must be a non-negative integer", result.Error);
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
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/api","status":200}}""");
        var result = Runner().RunText("waitForResponse \"/api\" timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("waitForResponse", result.StdoutLines[0]);
        Assert.Contains(result.StdoutLines, line => line.Contains("RESPONSE", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForResponseSupportsProviderFilters()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/api","method":"POST","status":201,"body":"created","mocked":true}}""");
        var result = Runner().RunText("waitForResponse \"/api\" method=POST status=201 contains=created mocked=true", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("method: 'POST'", client.LastExpression);
        Assert.Contains("status: 201", client.LastExpression);
        Assert.Contains("contains: 'created'", client.LastExpression);
        Assert.Contains("mocked: true", client.LastExpression);
    }

    [Fact]
    public void RunText_WaitForResponseSupportsHeaderFilter()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/api","headers":{"content-type":"application/json"}}}""");
        var result = Runner().RunText("waitForResponse \"/api\" header=\"Content-Type: json\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("headerName: 'content-type'", client.LastExpression);
        Assert.Contains("headerValue: 'json'", client.LastExpression);
    }

    [Fact]
    public void RunText_WaitForRequestRejectsHeaderValueWithoutHeaderName()
    {
        var result = Runner().RunText("waitForRequest \"/api\" headerValue=Bearer", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("headerValue= requires header= or headerName=", result.Error);
    }

    [Fact]
    public void RunText_WaitForResponseRejectsInvalidStatusFilter()
    {
        var result = Runner().RunText("waitForResponse \"/api\" status=ok", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("status= must be a numeric HTTP status", result.Error);
    }

    [Fact]
    public void RunText_WaitForRequestOutputsRequestLine()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/api","method":"GET"}}""");
        var result = Runner().RunText("waitForRequest \"/api\" timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgRequests", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("REQUEST", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForRequestReportsTimeout()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":false,"error":"Timed out waiting for request /api"}""");

        var result = Runner().RunText("waitForRequest \"/api\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Timed out waiting for request /api", result.Error);
    }

    [Fact]
    public void RunText_WaitForRequestFinishedOutputsLine()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/api","status":200}}""");
        var result = Runner().RunText("waitForRequestFinished \"/api\" timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgResponses", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("REQUEST_FINISHED", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForRequestFinishedReportsTimeout()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":false,"error":"Timed out waiting for finished request /api"}""");

        var result = Runner().RunText("waitForRequestFinished \"/api\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Timed out waiting for finished request /api", result.Error);
    }

    [Fact]
    public void RunText_WaitForRequestFailedOutputsFailureLine()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/api","type":"fetch","error":"Failed to fetch"}}""");
        var result = Runner().RunText("waitForRequestFailed \"/api\" timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgRequestFailures", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("REQUEST_FAILED", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForRequestFailedReportsTimeout()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":false,"error":"Timed out waiting for failed request /api"}""");

        var result = Runner().RunText("waitForRequestFailed \"/api\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Timed out waiting for failed request /api", result.Error);
    }

    [Fact]
    public void RunText_ExportHarWritesFile()
    {
        var file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.har");
        var result = Runner().RunText($"exportHar path=\"{file.Replace('\\', '/')}\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.True(File.Exists(file));
        Assert.Contains("HAR_EXPORTED", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_ReplayHarInstallsRoutes()
    {
        var file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.har");
        File.WriteAllText(file, """{"log":{"entries":[{"request":{"url":"/api"},"response":{"status":200,"content":{"text":"ok"}}}]}}""");
        var client = new FakeAutomationClient();

        var result = Runner().RunText($"replayHar path=\"{file.Replace('\\', '/')}\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgRoutes.push", client.LastExpression);
        Assert.Contains("HAR_REPLAY", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_ReplayHarFailsWhenFileIsMissing()
    {
        var result = Runner().RunText("replayHar path=\"missing.har\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("was not found", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
