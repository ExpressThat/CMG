using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserLifecycleE2eTests : IClassFixture<CmgBrowserFixture>
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
        result.StdoutContains($"Remote debugging: http://127.0.0.1:{fixture.BrowserPort}");
    }

    [Fact]
    public void AppAttachFailure_RejectsInvalidPort()
    {
        var result = fixture.Cli.Run("browser", "app", "attach", "--port", "0");

        result.ShouldFail();
        result.StderrContains("--port must be between 1 and 65535.");
    }

    [Fact]
    public void Control_WithWrongPortReportsNoSelectedBrowser()
    {
        var result = fixture.Cli.Run(
            "browser",
            "--port",
            "1",
            "control",
            "page",
            "runtime",
            "textContent",
            "#title");

        result.ShouldFail();
        result.StderrContains("No CMG-controlled Chrome instance is running.");
    }
}
