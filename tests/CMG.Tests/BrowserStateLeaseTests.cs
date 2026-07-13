using CMG.Browser;

namespace CMG.Tests;

public sealed class BrowserStateLeaseTests
{
    [Fact]
    public void Load_RemainsCompatibleWithLegacyStateFile()
    {
        using var fixture = new StateFixture();
        File.WriteAllLines(fixture.Path, [
            "ProcessId=42",
            "RemoteDebuggingPort=9333",
            "RemoteDebuggingUrl=http://127.0.0.1:9333",
            "UserDataDirectory=C:\\profile"
        ]);

        var state = fixture.Store.Load(BrowserKind.Chrome, 9333);

        Assert.NotNull(state);
        Assert.False(state.HasIdleLease);
        Assert.Equal(string.Empty, state.OwnershipToken);
    }

    [Fact]
    public void Renew_AtomicallyMovesOnlyEnabledLeaseDeadline()
    {
        using var fixture = new StateFixture();
        fixture.Store.Save(BrowserKind.Chrome, new BrowserState(
            42, 9333, "http://127.0.0.1:9333", "profile", true, "token", 1,
            fixture.Clock.GetUtcNow().UtcTicks, 60_000), 9333);
        var before = fixture.Store.Load(BrowserKind.Chrome, 9333)!.IdleDeadline;
        fixture.Clock.Advance(TimeSpan.FromMinutes(5));

        Parallel.For(0, 8, _ => fixture.Store.Renew(BrowserKind.Chrome, 9333));

        Assert.True(fixture.Store.Load(BrowserKind.Chrome, 9333)!.IdleDeadline > before);
        Assert.Empty(Directory.GetFiles(fixture.Directory, "*.tmp"));
    }

    private sealed class StateFixture : IDisposable
    {
        public string Directory { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        public string Path => System.IO.Path.Combine(Directory, "state.txt");
        public ManualClock Clock { get; } = new();
        public BrowserStateStore Store { get; }

        public StateFixture()
        {
            System.IO.Directory.CreateDirectory(Directory);
            Store = new BrowserStateStore((_, _) => Path, Clock);
        }
        public void Dispose()
        {
            if (System.IO.Directory.Exists(Directory)) System.IO.Directory.Delete(Directory, true);
        }
    }

    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan duration) => now += duration;
    }
}
