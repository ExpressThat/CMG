using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserNetworkEnvironmentActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserNetworkEnvironmentActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_NetworkEnvironmentActionsAffectFetchBehavior()
    {
        var script = fixture.CreateScript("network-environment-actions.cmgscript", Script("direct"));

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("HEADERS_SET");
        result.StdoutContains("HTTP_CREDENTIALS_SET");
        result.StdoutContains("PROXY_SET");
        result.StdoutContains("OFFLINE");
        result.StdoutContains("PROXY_CLEARED");
    }

    [Fact]
    public void RunCommand_NetworkEnvironmentActionsRunInsideTests()
    {
        var traceDir = fixture.OutputPath("runner-network-environment-traces");
        var script = fixture.CreateScript("runner-network-environment.cmgscript", $$"""
            test "runner network environment" {
            {{Indent(Script("runner"))}}
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner network environment");
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        Assert.Contains("HEADERS_SET", trace, StringComparison.Ordinal);
        Assert.Contains("PROXY_CLEARED", trace, StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_NetworkEnvironmentFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-network-environment-failure.cmgscript", """
            test "runner network environment failure" {
              setExtraHTTPHeaders "x-agent"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=setExtraHTTPHeaders");
        result.StderrContains("requires one or more <name> <value> header pairs");
    }

    private string Script(string mode) => $$"""
          navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
          setExtraHTTPHeaders "x-agent" "{{mode}}-agent"
          set headerEcho { evaluate "fetch('/api/echo').then(r => r.text())" }
          expect ("${headerEcho}" contains "\"xAgent\":\"{{mode}}-agent\"")
          clearExtraHTTPHeaders
          setHttpCredentials "{{mode}}-user" "secret"
          set authEcho { evaluate "fetch('/api/echo').then(r => r.text())" }
          expect ("${authEcho}" contains "\"authorization\":\"{{mode}}-user:secret\"")
          clearHttpCredentials
          setProxy "{{fixture.FixtureHttpPath("api/echo?proxied=")}}"
          set proxied { evaluate "fetch('/proxied-value').then(r => r.text())" }
          expect ("${proxied}" contains "proxied=")
          clearProxy
          setOffline true
          set offline { evaluate "fetch('/offline-check').catch(error => error.message)" }
          expect ("${offline}" contains "offline mode")
          setOffline false
        """;

    private static string Indent(string text) =>
        string.Join(Environment.NewLine, text.Split(Environment.NewLine).Select(line => "  " + line));
}
