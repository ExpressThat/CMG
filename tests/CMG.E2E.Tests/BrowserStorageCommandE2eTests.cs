using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserStorageCommandE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserStorageCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void StorageCommands_RemoveClearAndRestoreBrowserState()
    {
        Navigate();
        var state = fixture.OutputPath("storage-lifecycle.json");

        Run("browser", "control", "storage", "local", "set", "local-one", "alpha");
        Run("browser", "control", "storage", "session", "set", "session-one", "bravo");
        Run("browser", "control", "storage", "cookie", "set", "cookie-one", "charlie", "--path", "/", "--same-site", "Lax", "--max-age", "60");
        Run("browser", "control", "storage", "state", "save", "--path", state).StdoutContains("saved");
        CmgE2eAssert.FileExists(state);

        Run("browser", "control", "storage", "local", "remove", "local-one");
        Run("browser", "control", "storage", "local", "get", "local-one").StdoutContains("LOCAL_STORAGE 001 get local-one");
        Run("browser", "control", "storage", "session", "clear");
        Run("browser", "control", "storage", "session", "get", "session-one").StdoutContains("SESSION_STORAGE 001 get session-one");
        Run("browser", "control", "storage", "cookie", "remove", "cookie-one", "--path", "/");
        Run("browser", "control", "storage", "cookie", "get", "cookie-one").StdoutContains("COOKIE 001 get cookie-one");

        Run("browser", "control", "storage", "storageState", "load", "--path", state).StdoutContains("loaded");
        Run("browser", "control", "storage", "local", "get", "local-one").StdoutContains("alpha");
        Run("browser", "control", "storage", "session", "get", "session-one").StdoutContains("bravo");
        Run("browser", "control", "storage", "cookie", "get", "cookie-one").StdoutContains("charlie");
        Run("browser", "control", "storage", "cookie", "clear", "--path", "/");
        Run("browser", "control", "storage", "cookie", "get").StdoutContains("COOKIE 001 get");
    }

    [Fact]
    public void StorageCommandFailure_ReturnsValidationReason()
    {
        Navigate();

        var result = fixture.Cli.Run("browser", "control", "storage", "cookie", "set", "mode", "demo", "--same-site", "Maybe");

        result.ShouldFail();
        result.StderrContains("cookie sameSite expects Strict, Lax, or None");
    }

    private void Navigate()
    {
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }
}
