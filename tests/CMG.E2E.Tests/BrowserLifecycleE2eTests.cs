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
        result.StdoutContains($"Remote debugging: http://127.0.0.1:{fixture.BrowserPort}");
    }

    [Fact]
    public void LaunchFailure_RejectsInvalidBrowserPort()
    {
        var result = fixture.Cli.Run("browser", "--port", "0", "launch", "--headless");

        result.ShouldFail();
        result.StderrContains("--port must be between 1 and 65535.");
    }

    [Fact]
    public void Close_WithUnusedPortReportsNoBrowserAndIgnoredArguments()
    {
        var result = fixture.Cli.Run("browser", "--port", "1", "close", "--leftover", "value");

        result.ShouldPass();
        result.StdoutContains("No CMG-controlled Chrome instance is running.");
        result.StdoutContains("Ignored arguments: --leftover value");
    }

    [Fact]
    public void CloseFailure_RejectsInvalidBrowserPort()
    {
        var result = fixture.Cli.Run("browser", "--port", "0", "close");

        result.ShouldFail();
        result.StderrContains("--port must be between 1 and 65535.");
    }

    [Fact]
    public void AppAttachFailure_RejectsInvalidPort()
    {
        var result = fixture.Cli.Run("browser", "app", "attach", "--port", "0");

        result.ShouldFail();
        result.StderrContains("--port must be between 1 and 65535.");
    }

    [Fact]
    public void AppAttachFailure_RejectsInvalidPidAndTimeout()
    {
        var pid = fixture.Cli.Run("browser", "app", "attach", "--pid", "-1", "--connect-timeout", "0");
        var timeout = fixture.Cli.Run("browser", "app", "attach", "--connect-timeout", "-1");

        pid.ShouldFail();
        pid.StderrContains("--pid must be 0 or greater.");
        timeout.ShouldFail();
        timeout.StderrContains("--connect-timeout must be 0 or greater.");
    }

    [Fact]
    public void AppAttach_UsesExistingDebugEndpointForControlCommands()
    {
        var result = fixture.Cli.Run(
            "browser",
            "app",
            "attach",
            "--port",
            fixture.BrowserPort.ToString(),
            "--pid",
            fixture.BrowserProcessId.ToString());

        result.ShouldPass();
        result.StdoutContains("Attached CMG to app debugging endpoint");

        var title = fixture.Cli.Run("browser", "control", "navigation", "title");
        title.ShouldPass();
        title.StdoutContains("CMG E2E Fixture");
    }

    [Fact]
    public void AppLaunchFailure_RejectsInvalidKind()
    {
        var result = fixture.Cli.Run(
            "browser",
            "app",
            "launch",
            SystemExecutable("cmd.exe"),
            "--kind",
            "native",
            "--connect-timeout",
            "0");

        result.ShouldFail();
        result.StderrContains("App kind must be 'electron' or 'webview2'.");
    }

    [Fact]
    public void AppLaunchFailure_ExplainsMissingExecutable()
    {
        var missing = Path.Combine(fixture.OutputDirectory, "missing-electron.exe");

        var result = fixture.Cli.Run(
            "browser",
            "app",
            "launch",
            missing,
            "--connect-timeout",
            "0");

        result.ShouldFail();
        result.StdoutContains("was not found");
    }

    [Fact]
    public void AppLaunchFailure_WhenEndpointIsMissingExplainsConnectionReason()
    {
        var result = fixture.Cli.Run(
            "browser",
            "app",
            "launch",
            SystemExecutable("cmd.exe"),
            "--connect-timeout",
            "50");

        result.ShouldFail();
        result.StdoutContains("App launched, but CMG could not connect to");
        result.StdoutContains("Reason:");
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

    private static string SystemExecutable(string name) =>
        Path.Combine(Environment.SystemDirectory, name);
}
