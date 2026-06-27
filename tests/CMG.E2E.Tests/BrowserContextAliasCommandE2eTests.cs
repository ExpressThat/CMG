using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserContextAliasCommandE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserContextAliasCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ContextStateAliasCommands_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "storage", "local", "set", "context-alias-key", "value");
        Run("browser", "control", "storage", "session", "set", "context-session-key", "value");
        Run("browser", "control", "context", "clear").StdoutContains("CONTEXT_CLEARED 001");
        AssertStorageMissing("local", "context-alias-key");
        AssertStorageMissing("session", "context-session-key");

        Run("browser", "control", "storage", "local", "set", "context-alias-key", "value");
        Run("browser", "control", "context", "clearContext").StdoutContains("CONTEXT_CLEARED 001");
        AssertStorageMissing("local", "context-alias-key");

        Navigate();
        Run("browser", "control", "context", "reset").StdoutContains("CONTEXT_RESET 001");
        Run("browser", "control", "navigation", "url").StdoutContains("about:blank");

        Navigate();
        Run("browser", "control", "context", "resetContext").StdoutContains("CONTEXT_RESET 001");
        Run("browser", "control", "navigation", "url").StdoutContains("about:blank");
    }

    [Fact]
    public void JavaScriptAndServiceWorkerAliasCommands_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "context", "setJavaScriptEnabled", "false")
            .StdoutContains("JAVASCRIPT_ENABLED 001 false");
        Run("browser", "control", "context", "javaScriptEnabled", "true")
            .StdoutContains("JAVASCRIPT_ENABLED 001 true");
        Run("browser", "control", "context", "serviceWorkers", "block")
            .StdoutContains("SERVICE_WORKERS 001 block");
        Run("browser", "control", "context", "setServiceWorkers", "allow")
            .StdoutContains("SERVICE_WORKERS 001 allow");

        var invalidMode = fixture.Cli.Run("browser", "control", "context", "setServiceWorkers", "maybe");
        invalidMode.ShouldFail();
        invalidMode.StderrContains("allow or block");

        var invalidBoolean = fixture.Cli.Run("browser", "control", "context", "javaScriptEnabled", "maybe");
        invalidBoolean.ShouldFail();
        Assert.Contains("maybe", invalidBoolean.Stderr + invalidBoolean.Stdout, StringComparison.Ordinal);
    }

    private void AssertStorageMissing(string scope, string key)
    {
        var result = Run("browser", "control", "storage", scope, "get", key);
        Assert.DoesNotContain("value", result.Stdout, StringComparison.Ordinal);
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
}
