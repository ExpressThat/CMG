namespace CMG.Browser;

internal static class BrowserLeaseMonitor
{
    public static BrowserLeaseResult Run(
        BrowserStateStore stateStore,
        IBrowserController browserController,
        TimeProvider timeProvider,
        BrowserKind browserKind,
        int? port,
        string ownershipToken,
        Action<BrowserKind, int?, string> writeEvent,
        Action<int>? sleep = null)
    {
        sleep ??= Thread.Sleep;
        using var monitorLock = stateStore.TryAcquireMonitorLock(browserKind, port);
        if (monitorLock is null) return Stop("skipped", "monitor-already-running");
        while (true)
        {
            var state = stateStore.Load(browserKind, port);
            if (state is null) return Stop("missing", "no-state");
            if (!state.OwnershipToken.Equals(ownershipToken, StringComparison.Ordinal)) return Stop("skipped", "ownership-changed");
            if (!state.HasIdleLease) return Stop("skipped", "disabled");

            var remaining = state.IdleDeadline!.Value - timeProvider.GetUtcNow();
            if (remaining > TimeSpan.Zero)
            {
                sleep(ClampDelay(remaining));
                continue;
            }

            var warning = Format("warning", browserKind, state, "lease-expired-recheck");
            writeEvent(browserKind, port, warning);
            sleep(Math.Clamp(state.IdleTimeoutMilliseconds / 10, 500, 5_000));
            if (!TryClaimExpired(stateStore, timeProvider, browserKind, port, ownershipToken, out state)) continue;

            var close = browserController.Close(browserKind, port);
            var result = Format(close.ExitCode is 0 ? "closed" : "failed", browserKind, state!, close.ExitCode is 0 ? "idle-expired" : "close-failed");
            writeEvent(browserKind, port, result);
            return new BrowserLeaseResult(close.ExitCode, result);
        }

        BrowserLeaseResult Stop(string status, string reason)
        {
            var result = $"BROWSER_IDLE_CLEANUP status={status} browser={browserKind.StateName()} port={port ?? browserKind.DefaultRemoteDebuggingPort()} reason={reason}";
            writeEvent(browserKind, port, result);
            return new BrowserLeaseResult(0, result);
        }
    }

    private static bool TryClaimExpired(
        BrowserStateStore store,
        TimeProvider timeProvider,
        BrowserKind kind,
        int? port,
        string token,
        out BrowserState? claimed)
    {
        BrowserState? candidate = null;
        store.TryUpdate(kind, port, state =>
        {
            if (!state.OwnershipToken.Equals(token, StringComparison.Ordinal) ||
                !state.HasIdleLease || state.IdleDeadline > timeProvider.GetUtcNow()) return state;
            candidate = state;
            return state with { IdleTimeoutMilliseconds = 0 };
        }, out _);
        claimed = candidate;
        return claimed is not null;
    }

    private static int ClampDelay(TimeSpan remaining) =>
        (int)Math.Clamp(remaining.TotalMilliseconds, 100, 2_000);

    private static string Format(string status, BrowserKind kind, BrowserState state, string reason) =>
        $"BROWSER_IDLE_CLEANUP status={status} browser={kind.StateName()} port={state.RemoteDebuggingPort} pid={state.ProcessId} ownership=cmg idleTimeoutMs={state.IdleTimeoutMilliseconds} reason={reason}";
}
