using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgRunBrowserLeaseTests
{
    [Fact]
    public void Run_AutoLaunchHeadlessSchedulesLeaseAndReportsIt()
    {
        using var fixture = new RunFixture();
        var options = fixture.Options() with
        {
            AutoLaunch = true,
            AutoLaunchHeadless = true,
            BrowserIdleTimeoutMilliseconds = 30 * 60_000
        };

        var result = fixture.Service.Run(fixture.Script, options);

        Assert.True(result.Success, result.Error);
        Assert.Equal(30 * 60_000, fixture.Leases.EnabledTimeout);
        Assert.Contains(result.StdoutLines, line => line.Contains("BROWSER_IDLE_LEASE status=scheduled", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_RejectsVisibleAutoLaunchLeaseBeforeLaunching()
    {
        using var fixture = new RunFixture();
        var options = fixture.Options() with
        {
            AutoLaunch = true,
            BrowserIdleTimeoutMilliseconds = 60_000
        };

        var result = fixture.Service.Run(fixture.Script, options);

        Assert.False(result.Success);
        Assert.Contains("Add --headless", result.Error, StringComparison.Ordinal);
        Assert.False(fixture.Controller.Launched);
    }

    [Fact]
    public void Run_RejectsConflictingLeaseControls()
    {
        using var fixture = new RunFixture();
        var options = fixture.Options() with
        {
            BrowserIdleTimeoutMilliseconds = 60_000,
            NoBrowserIdleCleanup = true
        };

        var result = fixture.Service.Run(fixture.Script, options);

        Assert.False(result.Success);
        Assert.Contains("either --browser-idle-timeout", result.Error, StringComparison.Ordinal);
    }

    private sealed class RunFixture : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        public int Port { get; } = 32000 + Random.Shared.Next(10000);
        public BrowserStateStore Store { get; }
        public Controller Controller { get; }
        public LeaseManager Leases { get; } = new();
        public CmgRunService Service { get; }
        public string Script { get; }

        public RunFixture()
        {
            Directory.CreateDirectory(directory);
            Script = Path.Combine(directory, "flow.cmgscript");
            File.WriteAllText(Script, "test \"empty\" {\n}");
            Store = new BrowserStateStore((kind, port) => Path.Combine(directory, $"{kind}-{port}.state"));
            Controller = new Controller(Store);
            Service = new CmgRunService(
                Store, Controller, Leases,
                new BrowserAutomationClientFactory(new ChromeDevToolsClient(), new FirefoxBiDiClient()),
                new BrowserScriptRunner(new BrowserScriptParser()), new CmgDslParser(), new CmgTestPlanner(),
                new CmgActionLowerer(), new CmgValidator(), new CmgApiRequestRunner(), new CmgStorageStateRunner(),
                new CmgVisualAssertionRunner(), new CmgUploadRunner());
        }

        public CmgRunOptions Options() => new(
            BrowserKind.Chrome, null, null, null, null, null, null, null, 0, 0, 1, false, 1, 1,
            null, null, null, null, new Dictionary<string, string>(), BrowserPort: Port);

        public void Dispose()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private sealed class Controller(BrowserStateStore store) : IBrowserController
    {
        public bool Launched { get; private set; }
        public BrowserLaunchResult Launch(BrowserKind kind, IReadOnlyList<string> arguments, int? port = null)
        {
            Launched = true;
            var actualPort = port ?? kind.DefaultRemoteDebuggingPort();
            var url = $"http://127.0.0.1:{actualPort}";
            store.Save(kind, new BrowserState(123, actualPort, url, "profile", arguments.Count > 0, "owned", 1, DateTimeOffset.UtcNow.UtcTicks), port);
            return new BrowserLaunchResult(0, "launched", url);
        }
        public BrowserCloseResult Close(BrowserKind kind) => new(0, "closed");
        public BrowserCloseResult Close(BrowserKind kind, int? port) => new(0, "closed");
    }

    private sealed class LeaseManager : IBrowserLeaseManager
    {
        public int? EnabledTimeout { get; private set; }
        public BrowserLeaseResult Enable(BrowserKind kind, int? port, int timeout)
        {
            EnabledTimeout = timeout;
            return new BrowserLeaseResult(0, "BROWSER_IDLE_LEASE status=scheduled");
        }
        public BrowserLeaseResult KeepAlive(BrowserKind kind, int? port, int? timeout = null) => new(0, "renewed");
        public BrowserLeaseResult Disable(BrowserKind kind, int? port) => new(0, "disabled");
        public BrowserLeaseResult Status(BrowserKind kind, int? port) => new(0, "status");
        public BrowserLeaseResult Monitor(BrowserKind kind, int? port, string token) => new(0, "monitor");
    }
}
