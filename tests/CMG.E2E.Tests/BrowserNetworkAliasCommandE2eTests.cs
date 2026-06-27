using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserNetworkAliasCommandE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserNetworkAliasCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void NetworkHeaderCredentialAndProxyAliases_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "network", "setExtraHTTPHeaders", "x-agent", "alias-e2e", "x-extra", "present")
            .StdoutContains("HEADERS_SET 001 2");
        FetchEcho().StdoutContains("\"xAgent\":\"alias-e2e\"");
        Run("browser", "control", "network", "clearExtraHTTPHeaders").StdoutContains("HEADERS_CLEARED 001");
        FetchEcho().StdoutContains("\"xAgent\":null");

        Run("browser", "control", "network", "setCredentials", "agent", "secret")
            .StdoutContains("HTTP_CREDENTIALS_SET 001 agent");
        FetchEcho().StdoutContains("\"authorization\":\"agent:secret\"");
        Run("browser", "control", "network", "clearCredentials").StdoutContains("HTTP_CREDENTIALS_CLEARED 001");
        FetchEcho().StdoutContains("\"authorization\":null");

        Run("browser", "control", "network", "httpCredentials", "robot", "pass")
            .StdoutContains("HTTP_CREDENTIALS_SET 001 robot");
        FetchEcho().StdoutContains("\"authorization\":\"robot:pass\"");
        Run("browser", "control", "network", "clearHttpCredentials").StdoutContains("HTTP_CREDENTIALS_CLEARED 001");

        Run("browser", "control", "network", "authenticate", "authuser", "authpass")
            .StdoutContains("HTTP_CREDENTIALS_SET 001 authuser");
        FetchEcho().StdoutContains("\"authorization\":\"authuser:authpass\"");
        Run("browser", "control", "network", "clearCredentials");

        var prefix = fixture.FixtureHttpUri("api/echo?proxied=");
        Run("browser", "control", "network", "setProxy", prefix).StdoutContains("PROXY_SET 001");
        FetchText("plain-proxy").StdoutContains("plain-proxy");
        Run("browser", "control", "network", "clearProxy").StdoutContains("PROXY_CLEARED 001");
        Run("browser", "control", "network", "proxy", prefix).StdoutContains("PROXY_SET 001");
        FetchText("alias-proxy").StdoutContains("alias-proxy");
        Run("browser", "control", "network", "clearProxy");
    }

    [Fact]
    public void MockResponseAliasAndFailures_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "network", "mockResponse", "alias-mock.json", "--status", "206", "--body", "mocked", "--content-type", "text/plain")
            .StdoutContains("PASS 001 mockResponse");
        FetchText("/alias-mock.json").StdoutContains("mocked");
        Run("browser", "control", "network", "waitForResponse", "alias-mock.json", "--status", "206", "--mocked", "true");

        var oddHeaders = fixture.Cli.Run("browser", "control", "network", "setExtraHTTPHeaders", "x-agent");
        oddHeaders.ShouldFail();
        oddHeaders.StderrContains("header pairs");

        var badOffline = fixture.Cli.Run("browser", "control", "network", "setOffline", "maybe");
        badOffline.ShouldFail();
        badOffline.StderrContains("Cannot parse argument 'maybe'");
    }

    private CmgResult FetchEcho() =>
        FetchText("/api/echo");

    private CmgResult FetchText(string url) =>
        Run("browser", "control", "page", "evaluate", $"fetch('{url}').then(r => r.text())");

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
}
