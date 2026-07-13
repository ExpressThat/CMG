namespace CMG.Browser;

public sealed partial class BrowserStateStore
{
    private readonly Func<BrowserKind, int?, string> stateFileResolver;
    private readonly TimeProvider timeProvider;

    public BrowserStateStore()
        : this(BrowserPaths.GetStateFile, TimeProvider.System)
    {
    }

    public BrowserStateStore(
        Func<BrowserKind, int?, string> stateFileResolver,
        TimeProvider? timeProvider = null)
    {
        this.stateFileResolver = stateFileResolver;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public BrowserState? Load()
    {
        return Load(BrowserKind.Chrome);
    }

    public BrowserState? Load(BrowserKind browserKind)
    {
        return Load(browserKind, port: null);
    }

    public BrowserState? Load(BrowserKind browserKind, int? port)
    {
        var stateFile = stateFileResolver(browserKind, port);
        using var stateLock = AcquireLock(stateFile);
        return LoadUnlocked(stateFile);
    }

    public BrowserLeaseActivity BeginActivity(BrowserKind browserKind, int? port)
    {
        var state = Renew(browserKind, port);
        return new BrowserLeaseActivity(this, browserKind, port, state);
    }

    public BrowserState? Renew(BrowserKind browserKind, int? port)
    {
        var stateFile = stateFileResolver(browserKind, port);
        using var stateLock = AcquireLock(stateFile);
        var state = LoadUnlocked(stateFile);
        if (state is null || !state.HasIdleLease)
        {
            return state;
        }

        state = state with { LastActivityUtcTicks = timeProvider.GetUtcNow().UtcTicks };
        SaveUnlocked(stateFile, state);
        return state;
    }

    public bool TryUpdate(
        BrowserKind browserKind,
        int? port,
        Func<BrowserState, BrowserState?> update,
        out BrowserState? state)
    {
        var stateFile = stateFileResolver(browserKind, port);
        using var stateLock = AcquireLock(stateFile);
        state = LoadUnlocked(stateFile);
        if (state is null) return false;
        state = update(state);
        if (state is null) return false;
        SaveUnlocked(stateFile, state);
        return true;
    }

    private static BrowserState? LoadUnlocked(string stateFile)
    {
        if (!File.Exists(stateFile)) return null;
        var values = File.ReadAllLines(stateFile)
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length is 2)
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);

        if (!values.TryGetValue(nameof(BrowserState.ProcessId), out var processIdValue) ||
            !int.TryParse(processIdValue, out var processId))
        {
            return null;
        }

        _ = int.TryParse(values.GetValueOrDefault(nameof(BrowserState.RemoteDebuggingPort)), out var storedPort);

        _ = bool.TryParse(values.GetValueOrDefault(nameof(BrowserState.IsHeadless)), out var isHeadless);
        _ = long.TryParse(values.GetValueOrDefault(nameof(BrowserState.ProcessStartTimeUtcTicks)), out var processStart);
        _ = long.TryParse(values.GetValueOrDefault(nameof(BrowserState.LastActivityUtcTicks)), out var lastActivity);
        _ = int.TryParse(values.GetValueOrDefault(nameof(BrowserState.IdleTimeoutMilliseconds)), out var idleTimeout);
        return new BrowserState(
            processId,
            storedPort,
            values.GetValueOrDefault(nameof(BrowserState.RemoteDebuggingUrl)) ?? string.Empty,
            values.GetValueOrDefault(nameof(BrowserState.UserDataDirectory)) ?? string.Empty,
            isHeadless,
            values.GetValueOrDefault(nameof(BrowserState.OwnershipToken)) ?? string.Empty,
            processStart,
            lastActivity,
            idleTimeout);
    }

    public void Save(BrowserState state)
    {
        Save(BrowserKind.Chrome, state);
    }

    public void Save(BrowserKind browserKind, BrowserState state)
    {
        Save(browserKind, state, state.RemoteDebuggingPort);
    }

    public void Save(BrowserKind browserKind, BrowserState state, int? port)
    {
        var stateFile = stateFileResolver(browserKind, port);
        using var stateLock = AcquireLock(stateFile);
        SaveUnlocked(stateFile, state);
    }

    private static void SaveUnlocked(string stateFile, BrowserState state)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(stateFile)!);
        var temporary = $"{stateFile}.{Environment.ProcessId}.{Guid.NewGuid():N}.tmp";
        File.WriteAllLines(temporary, [
            $"{nameof(BrowserState.ProcessId)}={state.ProcessId}",
            $"{nameof(BrowserState.RemoteDebuggingPort)}={state.RemoteDebuggingPort}",
            $"{nameof(BrowserState.RemoteDebuggingUrl)}={state.RemoteDebuggingUrl}",
            $"{nameof(BrowserState.UserDataDirectory)}={state.UserDataDirectory}",
            $"{nameof(BrowserState.IsHeadless)}={state.IsHeadless}",
            $"{nameof(BrowserState.OwnershipToken)}={state.OwnershipToken}",
            $"{nameof(BrowserState.ProcessStartTimeUtcTicks)}={state.ProcessStartTimeUtcTicks}",
            $"{nameof(BrowserState.LastActivityUtcTicks)}={state.LastActivityUtcTicks}",
            $"{nameof(BrowserState.IdleTimeoutMilliseconds)}={state.IdleTimeoutMilliseconds}"
        ]);
        File.Move(temporary, stateFile, overwrite: true);
    }

    public void Clear()
    {
        Clear(BrowserKind.Chrome);
    }

    public void Clear(BrowserKind browserKind)
    {
        Clear(browserKind, port: null);
    }

    public void Clear(BrowserKind browserKind, int? port)
    {
        var stateFile = stateFileResolver(browserKind, port);
        using var stateLock = AcquireLock(stateFile);
        if (File.Exists(stateFile)) File.Delete(stateFile);
    }

    private static FileStream AcquireLock(string stateFile)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(stateFile)!);
        var lockFile = $"{stateFile}.lock";
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return new FileStream(lockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (attempt < 100)
            {
                Thread.Sleep(10);
            }
        }
    }

    internal FileStream? TryAcquireMonitorLock(BrowserKind browserKind, int? port)
    {
        var path = $"{stateFileResolver(browserKind, port)}.monitor.lock";
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        try
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            return null;
        }
    }
}
