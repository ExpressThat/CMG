using System.Text.Json;
using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserWorkerCommandE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserWorkerCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void WorkerCommands_WaitEvaluateAndInterceptRealWorker()
    {
        Navigate();
        var workerUrl = fixture.FixtureHttpUri("worker-fixture.js");
        Run("browser", "control", "workers", "list");
        Evaluate($$"""
        window.__cmgWorker = new Worker({{JsonSerializer.Serialize(workerUrl)}}, { name: "worker-fixture.js" });
        window.__cmgWorker.addEventListener('message', event => window.__cmgWorkerReply = event.data.value);
        true
        """);

        Run("browser", "control", "workers", "list").StdoutContains("worker-fixture.js");
        Run("browser", "control", "workers", "listWorkers").StdoutContains("WORKER");
        Run("browser", "control", "workers", "wait", "worker-fixture.js", "--timeout", "5000");
        Run("browser", "control", "workers", "waitForWorker", "worker-fixture\\.js$", "--match", "regex", "--timeout", "5000");
        Run("browser", "control", "workers", "evaluate", "self.__cmgWorkerState.startedAt > 0", "--target", "worker-fixture.js")
            .StdoutContains("true");
        Run("browser", "control", "workers", "workerEvaluate", "self.__cmgWorkerState.messages.length", "--target", "worker-fixture.js")
            .StdoutContains("0");
        Run("browser", "control", "workers", "intercept", "worker-mocked.json", "--body", "{\"from\":\"worker\"}", "--content-type", "application/json", "--target", "worker-fixture.js");
        Run("browser", "control", "workers", "workerEvaluate", "fetch('/worker-mocked.json').then(r => r.text())", "--target", "worker-fixture.js")
            .StdoutContains("{\"from\":\"worker\"}");

        var missing = fixture.Cli.Run(
            "browser",
            "control",
            "workers",
            "wait",
            "definitely-missing-worker.js",
            "--timeout",
            "10");
        missing.ShouldFail();
        missing.StderrContains("definitely-missing-worker.js");
    }

    private CmgResult Evaluate(string expression) =>
        Run("browser", "control", "page", "evaluate", expression);

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
}
