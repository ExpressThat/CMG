using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerWorkerActionTests
{
    [Fact]
    public void RunText_ListAndWaitForWorkers()
    {
        var client = new FakeAutomationClient();
        client.Workers.Add(new BrowserWorkerInfo("w1", "worker", "worker.js", "https://example.com/worker.js"));

        var result = Runner().RunText("listWorkers\nwaitForWorker \"worker.js\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("WORKER 0 id=w1", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.StartsWith("WORKER_READY", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForWorkerSupportsRegexMatch()
    {
        var client = new FakeAutomationClient();
        client.Workers.Add(new BrowserWorkerInfo("w1", "worker", "worker.js", "https://example.com/Worker-42.js"));

        var result = Runner().RunText("waitForWorker \"worker-\\d+\\.js\" match=regex", "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("WORKER_READY", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForWorkerSupportsExactCaseSensitiveMatch()
    {
        var client = new FakeAutomationClient();
        client.Workers.Add(new BrowserWorkerInfo("w1", "worker", "worker.js", "https://example.com/Worker.js"));

        var result = Runner().RunText("waitForWorker \"https://example.com/worker.js\" match=exact ignoreCase=false timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("was not available", result.Error);
    }

    [Fact]
    public void RunText_WorkerEvaluateReportsResult()
    {
        var result = Runner().RunText("workerEvaluate \"1 + 1\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("WORKER_EVALUATE 001 worker:1 + 1", result.StdoutLines);
    }

    [Fact]
    public void RunText_WorkerInterceptPassesRouteOptions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("workerIntercept \"/api\" status=201 body=\"ok\" contentType=\"text/plain\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(new WorkerRouteOptions("/api", 201, "ok", "text/plain"), client.LastWorkerRoute);
        Assert.Contains("WORKER_INTERCEPT 001 routes=1 /api", result.StdoutLines);
    }

    [Fact]
    public void RunText_WorkerInterceptSupportsMatchHeadersAndBodyFile()
    {
        var file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        File.WriteAllText(file, "created");
        var client = new FakeAutomationClient();

        var result = Runner().RunText(
            $"workerIntercept \"/api/\\\\d+\" match=regex ignoreCase=true bodyFile=\"{Slash(file)}\" header=\"X-Test: yes\"",
            "debug",
            client);

        Assert.True(result.Success);
        Assert.NotNull(client.LastWorkerRoute);
        Assert.Equal("regex", client.LastWorkerRoute.Match);
        Assert.True(client.LastWorkerRoute.IgnoreCase);
        Assert.Equal("created", client.LastWorkerRoute.Body);
        Assert.Equal("yes", client.LastWorkerRoute.Headers?["X-Test"]);
    }

    [Fact]
    public void RunText_WorkerInterceptRejectsMissingBodyFile()
    {
        var result = Runner().RunText("workerIntercept \"/api\" bodyFile=\"missing.json\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("body file 'missing.json' was not found", result.Error);
    }

    [Fact]
    public void RunText_WaitForWorkerReportsTimeout()
    {
        var result = Runner().RunText("waitForWorker \"missing\" timeout=1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("was not available", result.Error);
    }

    private static string Slash(string path) => path.Replace('\\', '/');

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
