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
    public void WorkerCommands_ListRealWorkerAndReportMissingWorkerWait()
    {
        Navigate();
        var workerUrl = fixture.FixtureHttpUri("worker-fixture.js");
        Evaluate($$"""
        window.__cmgWorker = new Worker({{JsonSerializer.Serialize(workerUrl)}});
        window.__cmgWorker.addEventListener('message', event => window.__cmgWorkerReply = event.data.value);
        true
        """);

        Run("browser", "control", "workers", "list").StdoutContains("WORKER");
        Run("browser", "control", "workers", "listWorkers").StdoutContains("WORKER");

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
