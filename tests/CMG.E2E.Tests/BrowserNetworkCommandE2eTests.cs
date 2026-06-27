using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserNetworkCommandE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserNetworkCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void NetworkCommands_RouteWaitHarHeadersAndOfflineAgainstRealPage()
    {
        NavigateHttp();
        var har = fixture.OutputPath("command-network.har");
        var replayHar = fixture.CreateScript("replay.har", """
        {"log":{"entries":[{"request":{"url":"http://cmg.local/replay.json"},"response":{"status":202,"content":{"mimeType":"application/json","text":"{\"replayed\":true}"}}}]}}
        """);

        Run("browser", "control", "network", "route", "mocked.json", "--status", "201", "--body", "{\"ok\":true}", "--content-type", "application/json", "--header", "x-cmg: routed");
        Evaluate("fetch('/mocked.json').then(r => r.text())").StdoutContains("{\"ok\":true}");
        Run("browser", "control", "network", "waitForRequest", "mocked.json", "--method", "GET");
        Run("browser", "control", "network", "waitForResponse", "mocked.json", "--status", "201", "--mocked", "true", "--contains", "ok", "--header", "x-cmg: routed");
        Run("browser", "control", "network", "waitForRequestFinished", "mocked.json");
        Run("browser", "control", "network", "exportHar", "--path", har);
        CmgE2eAssert.FileExists(har);
        Run("browser", "control", "network", "clearRoutes");
        Run("browser", "control", "network", "replayHar", "--path", replayHar);
        Evaluate("fetch('http://cmg.local/replay.json').then(async r => r.status + ':' + await r.text())")
            .StdoutContains("202:{\"replayed\":true}");
        Run("browser", "control", "network", "route", "abort-me", "--abort");
        Evaluate("fetch('/abort-me').catch(error => error.message)");
        Run("browser", "control", "network", "waitForRequestFailed", "abort-me", "--mocked", "true");
        Run("browser", "control", "network", "setHeaders", "x-agent", "cmg-e2e");
        Evaluate("fetch('/headers.json', { headers: { 'x-local': 'yes' } }).then(r => r.status).catch(() => '')");
        Run("browser", "control", "network", "waitForRequest", "headers.json", "--header-name", "x-agent", "--header-value", "cmg-e2e");
        Run("browser", "control", "network", "clearHeaders");
        Run("browser", "control", "network", "setOffline", "true");
        Evaluate("fetch('/offline.json').catch(error => error.message)").StdoutContains("offline");
        Run("browser", "control", "network", "setOffline", "false");
    }

    private CmgResult Evaluate(string expression) =>
        Run("browser", "control", "page", "evaluate", expression);

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void NavigateHttp() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
}
