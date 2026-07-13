namespace CMG.Browser;

public interface IBrowserLeaseManager
{
    BrowserLeaseResult Enable(BrowserKind browserKind, int? port, int idleTimeoutMilliseconds);
    BrowserLeaseResult KeepAlive(BrowserKind browserKind, int? port, int? idleTimeoutMilliseconds = null);
    BrowserLeaseResult Disable(BrowserKind browserKind, int? port);
    BrowserLeaseResult Status(BrowserKind browserKind, int? port);
    BrowserLeaseResult Monitor(BrowserKind browserKind, int? port, string ownershipToken);
}

public sealed class BrowserLeaseManager : IBrowserLeaseManager
{
    private readonly BrowserStateStore stateStore;
    private readonly IBrowserController browserController;
    private readonly IBrowserLeaseMonitorLauncher monitorLauncher;
    private readonly TimeProvider timeProvider;

    public BrowserLeaseManager(
        BrowserStateStore stateStore,
        IBrowserController browserController,
        IBrowserLeaseMonitorLauncher monitorLauncher,
        TimeProvider? timeProvider = null)
    {
        this.stateStore = stateStore;
        this.browserController = browserController;
        this.monitorLauncher = monitorLauncher;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public BrowserLeaseResult Enable(BrowserKind browserKind, int? port, int idleTimeoutMilliseconds)
    {
        if (idleTimeoutMilliseconds <= 0)
            return Fail("--idle-timeout must be greater than zero. Use 'browser lease disable' to turn cleanup off.");

        BrowserState? updated = null;
        var found = stateStore.TryUpdate(browserKind, port, state =>
        {
            if (!state.IsHeadless || state.OwnershipToken.Length is 0) return state;
            updated = state with
            {
                IdleTimeoutMilliseconds = idleTimeoutMilliseconds,
                LastActivityUtcTicks = timeProvider.GetUtcNow().UtcTicks
            };
            return updated;
        }, out var current);
        if (!found || current is null) return Missing(browserKind, port);
        if (updated is null) return Fail("Idle cleanup can only be enabled for a CMG-launched headless browser.");
        if (!monitorLauncher.TryStart(browserKind, EffectivePort(browserKind, port), updated.OwnershipToken, out var error))
        {
            stateStore.TryUpdate(browserKind, port, state => state with { IdleTimeoutMilliseconds = 0 }, out _);
            return Fail($"Could not start the browser idle monitor: {error}");
        }

        var message = Format("scheduled", browserKind, updated, "enabled");
        WriteEvent(browserKind, port, message);
        return new BrowserLeaseResult(0, message);
    }

    public BrowserLeaseResult KeepAlive(BrowserKind browserKind, int? port, int? idleTimeoutMilliseconds = null)
    {
        BrowserState? updated = null;
        var found = stateStore.TryUpdate(browserKind, port, state =>
        {
            var timeout = idleTimeoutMilliseconds ?? state.IdleTimeoutMilliseconds;
            if (!state.IsHeadless || state.OwnershipToken.Length is 0 || timeout <= 0) return state;
            updated = state with
            {
                IdleTimeoutMilliseconds = timeout,
                LastActivityUtcTicks = timeProvider.GetUtcNow().UtcTicks
            };
            return updated;
        }, out var current);
        if (!found || current is null) return Missing(browserKind, port);
        if (updated is null) return Fail("No active idle lease exists. Pass --idle-timeout to enable one for a CMG-launched headless browser.");
        var message = Format("renewed", browserKind, updated, "keepalive");
        WriteEvent(browserKind, port, message);
        return new BrowserLeaseResult(0, message);
    }

    public BrowserLeaseResult Disable(BrowserKind browserKind, int? port)
    {
        if (!stateStore.TryUpdate(browserKind, port, state => state with { IdleTimeoutMilliseconds = 0 }, out var state) || state is null)
            return Missing(browserKind, port);
        var message = Format("disabled", browserKind, state, "caller-request");
        WriteEvent(browserKind, port, message);
        return new BrowserLeaseResult(0, message);
    }

    public BrowserLeaseResult Status(BrowserKind browserKind, int? port)
    {
        var state = stateStore.Load(browserKind, port);
        if (state is null) return new BrowserLeaseResult(0, LastEvent(browserKind, port) ?? FormatMissing(browserKind, port));
        return new BrowserLeaseResult(0, Format(state.HasIdleLease ? "active" : "disabled", browserKind, state, "status"));
    }

    private static int EffectivePort(BrowserKind browserKind, int? port) => port ?? browserKind.DefaultRemoteDebuggingPort();

    private static BrowserLeaseResult Missing(BrowserKind browserKind, int? port) =>
        Fail($"No CMG-controlled {browserKind.DisplayName()} instance is running on port {EffectivePort(browserKind, port)}.");

    private static BrowserLeaseResult Fail(string message) => new(1, message);

    private static string Format(string status, BrowserKind kind, BrowserState state, string reason)
    {
        var deadline = state.IdleDeadline?.ToString("O") ?? "none";
        var ownership = state.OwnershipToken.Length > 0 ? "cmg" : "unowned";
        return $"BROWSER_IDLE_LEASE status={status} browser={kind.StateName()} port={state.RemoteDebuggingPort} pid={state.ProcessId} ownership={ownership} idleTimeoutMs={state.IdleTimeoutMilliseconds} deadline={deadline} reason={reason}";
    }

    private static string FormatMissing(BrowserKind kind, int? port) =>
        $"BROWSER_IDLE_LEASE status=missing browser={kind.StateName()} port={EffectivePort(kind, port)} reason=no-state";

    private static string? LastEvent(BrowserKind kind, int? port)
    {
        var path = BrowserPaths.GetLeaseEventFile(kind, port);
        return File.Exists(path) ? File.ReadLines(path).LastOrDefault() : null;
    }

    private static void WriteEvent(BrowserKind kind, int? port, string message)
    {
        BrowserPaths.EnsureAppDataDirectory();
        File.AppendAllLines(BrowserPaths.GetLeaseEventFile(kind, port), [message]);
    }

    public BrowserLeaseResult Monitor(BrowserKind browserKind, int? port, string ownershipToken) =>
        BrowserLeaseMonitor.Run(stateStore, browserController, timeProvider, browserKind, port, ownershipToken, WriteEvent);
}

public sealed record BrowserLeaseResult(int ExitCode, string Message);
