using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserLifecycleE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserLifecycleE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void Launch_WhenAlreadyRunningReportsExistingHeadlessBrowser()
    {
        var result = fixture.Cli.Run("browser", "launch", "--headless");

        result.ShouldPass();
        result.StdoutContains("Chrome is already running for CMG.");
        result.StdoutContains("Remote debugging: http://127.0.0.1:9222");
    }

    [Fact]
    public void AppAttachFailure_RejectsInvalidPort()
    {
        var result = fixture.Cli.Run("browser", "app", "attach", "--port", "0");

        result.ShouldFail();
        result.StderrContains("--port must be between 1 and 65535.");
    }
}
