using CMG.Browser;

namespace CMG.Tests;

public sealed class BrowserLeaseManagerTests
{
    [Fact]
    public void Enable_SchedulesOnlyOwnedHeadlessBrowser()
    {
        using var fixture = new LeaseFixture();
        fixture.Save(headless: true);

        var result = fixture.Manager.Enable(BrowserKind.Chrome, fixture.Port, 30 * 60_000);

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("status=scheduled", result.Message, StringComparison.Ordinal);
        Assert.Equal(1, fixture.Launcher.Starts);
        Assert.Equal(30 * 60_000, fixture.Store.Load(BrowserKind.Chrome, fixture.Port)!.IdleTimeoutMilliseconds);
    }

    [Fact]
    public void Enable_RefusesVisibleBrowserWithoutStartingMonitor()
    {
        using var fixture = new LeaseFixture();
        fixture.Save(headless: false);

        var result = fixture.Manager.Enable(BrowserKind.Chrome, fixture.Port, 60_000);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("headless", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, fixture.Launcher.Starts);
    }

    [Fact]
    public void KeepAlive_RenewsAndCanExtendLease()
    {
        using var fixture = new LeaseFixture();
        fixture.Save(headless: true);
        fixture.Manager.Enable(BrowserKind.Chrome, fixture.Port, 60_000);
        var previous = fixture.Store.Load(BrowserKind.Chrome, fixture.Port)!.LastActivityUtcTicks;
        fixture.Clock.Advance(TimeSpan.FromMinutes(10));

        var result = fixture.Manager.KeepAlive(BrowserKind.Chrome, fixture.Port, 2 * 60_000);

        var state = fixture.Store.Load(BrowserKind.Chrome, fixture.Port)!;
        Assert.Equal(0, result.ExitCode);
        Assert.True(state.LastActivityUtcTicks > previous);
        Assert.Equal(2 * 60_000, state.IdleTimeoutMilliseconds);
        Assert.Equal(1, fixture.Launcher.Starts);
    }

    [Fact]
    public void Disable_IsolatedByBrowserPort()
    {
        using var fixture = new LeaseFixture();
        fixture.Save(headless: true);
        fixture.Save(headless: true, port: fixture.Port + 1, token: "other");
        fixture.Manager.Enable(BrowserKind.Chrome, fixture.Port, 60_000);
        fixture.Manager.Enable(BrowserKind.Chrome, fixture.Port + 1, 60_000);

        fixture.Manager.Disable(BrowserKind.Chrome, fixture.Port);

        Assert.False(fixture.Store.Load(BrowserKind.Chrome, fixture.Port)!.HasIdleLease);
        Assert.True(fixture.Store.Load(BrowserKind.Chrome, fixture.Port + 1)!.HasIdleLease);
    }

    [Fact]
    public void Monitor_ClosesOnlyStillExpiredMatchingOwnership()
    {
        using var fixture = new LeaseFixture();
        fixture.Save(headless: true, timeout: 1000);
        fixture.Clock.Advance(TimeSpan.FromSeconds(2));
        var events = new List<string>();

        var result = BrowserLeaseMonitor.Run(
            fixture.Store,
            fixture.Controller,
            fixture.Clock,
            BrowserKind.Chrome,
            fixture.Port,
            "owned",
            (_, _, message) => events.Add(message),
            _ => { });

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(1, fixture.Controller.Closes);
        Assert.Contains(events, message => message.Contains("status=warning", StringComparison.Ordinal));
        Assert.Contains(events, message => message.Contains("status=closed", StringComparison.Ordinal));
    }

    [Fact]
    public void Monitor_SkipsWhenOwnershipChanged()
    {
        using var fixture = new LeaseFixture();
        fixture.Save(headless: true, timeout: 1000, token: "replacement");

        var result = BrowserLeaseMonitor.Run(
            fixture.Store,
            fixture.Controller,
            fixture.Clock,
            BrowserKind.Chrome,
            fixture.Port,
            "old-token",
            (_, _, _) => { },
            _ => { });

        Assert.Contains("ownership-changed", result.Message, StringComparison.Ordinal);
        Assert.Equal(0, fixture.Controller.Closes);
    }

    [Fact]
    public void Monitor_SkipsDuplicateProcessForSamePort()
    {
        using var fixture = new LeaseFixture();
        fixture.Save(headless: true, timeout: 60_000);
        using var existing = fixture.Store.TryAcquireMonitorLock(BrowserKind.Chrome, fixture.Port);

        var result = BrowserLeaseMonitor.Run(
            fixture.Store, fixture.Controller, fixture.Clock, BrowserKind.Chrome, fixture.Port, "owned",
            (_, _, _) => { }, _ => { });

        Assert.Contains("monitor-already-running", result.Message, StringComparison.Ordinal);
        Assert.Equal(0, fixture.Controller.Closes);
    }

    private sealed class LeaseFixture : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        public int Port { get; } = 31000 + Random.Shared.Next(10000);
        public ManualTimeProvider Clock { get; } = new();
        public FakeMonitorLauncher Launcher { get; } = new();
        public FakeController Controller { get; } = new();
        public BrowserStateStore Store { get; }
        public BrowserLeaseManager Manager { get; }

        public LeaseFixture()
        {
            Store = new BrowserStateStore((kind, port) => Path.Combine(directory, $"{kind}-{port}.state"), Clock);
            Manager = new BrowserLeaseManager(Store, Controller, Launcher, Clock);
        }

        public void Save(bool headless, int timeout = 0, int? port = null, string token = "owned") =>
            Store.Save(BrowserKind.Chrome, new BrowserState(
                123,
                port ?? Port,
                $"http://127.0.0.1:{port ?? Port}",
                directory,
                headless,
                token,
                1,
                Clock.GetUtcNow().UtcTicks,
                timeout), port ?? Port);

        public void Dispose()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
            foreach (var port in new[] { Port, Port + 1 })
            {
                var log = BrowserPaths.GetLeaseEventFile(BrowserKind.Chrome, port);
                if (File.Exists(log)) File.Delete(log);
            }
        }
    }

    private sealed class FakeMonitorLauncher : IBrowserLeaseMonitorLauncher
    {
        public int Starts { get; private set; }
        public bool TryStart(BrowserKind browserKind, int port, string ownershipToken, out string? error)
        {
            Starts++;
            error = null;
            return true;
        }
    }

    private sealed class FakeController : IBrowserController
    {
        public int Closes { get; private set; }
        public BrowserLaunchResult Launch(BrowserKind kind, IReadOnlyList<string> arguments, int? port = null) => throw new NotSupportedException();
        public BrowserCloseResult Close(BrowserKind kind) => Close(kind, null);
        public BrowserCloseResult Close(BrowserKind kind, int? port)
        {
            Closes++;
            return new BrowserCloseResult(0, "closed");
        }
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan duration) => now += duration;
    }
}
