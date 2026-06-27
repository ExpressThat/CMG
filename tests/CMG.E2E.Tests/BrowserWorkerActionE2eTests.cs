using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserWorkerActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserWorkerActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_WorkerActionsWaitEvaluateAndInterceptRealWorker()
    {
        var script = fixture.CreateScript("worker-actions.cmgscript", Script("direct-worker", "script-worker-body"));

        var result = RunScript(script);

        result.StdoutContains("WORKER ");
        result.StdoutContains("direct-worker");
        result.StdoutContains("WORKER_READY");
        result.StdoutContains("WORKER_EVALUATE");
        result.StdoutContains("true");
        result.StdoutContains("script-worker-body");
        result.StdoutContains("WORKER_INTERCEPT");
    }

    [Fact]
    public void RunCommand_WorkerActionsRunInsideTests()
    {
        var script = fixture.CreateScript("runner-worker-actions.cmgscript", $$"""
            test "worker actions" {
            {{Indent(Script("runner-worker", "runner-worker-body"))}}
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldPass();
        result.StdoutContains("TEST PASS worker actions");
    }

    [Fact]
    public void DirectScript_WorkerActionsReportMissingWorkerFailure()
    {
        var script = fixture.CreateScript("worker-missing.cmgscript", """
            workerEvaluate "self.ready" target="definitely-missing-worker"
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StderrContains("No worker matched 'definitely-missing-worker'.");
    }

    private string Script(string workerName, string body) => $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
        listWorkers
        evaluate "window['{{workerName}}'] = new Worker('{{fixture.FixtureHttpUri("worker-fixture.js")}}', { name: '{{workerName}}' }); true"
        waitForWorker "worker-fixture.js" timeout=5000
        listWorkers
        workerEvaluate "self.__cmgWorkerState.startedAt > 0" target="{{workerName}}"
        workerIntercept "worker-action-mock.txt" body="{{body}}" contentType="text/plain" target="{{workerName}}"
        workerEvaluate "fetch('/worker-action-mock.txt').then(r => r.text())" target="{{workerName}}"
        """;

    private CmgResult RunScript(string script)
    {
        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);
        result.ShouldPass();
        return result;
    }

    private static string Indent(string text) =>
        string.Join(Environment.NewLine, text.Split(Environment.NewLine).Select(line => "  " + line));
}
