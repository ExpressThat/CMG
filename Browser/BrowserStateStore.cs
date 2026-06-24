namespace CMG.Browser;

public sealed class BrowserStateStore
{
    public BrowserState? Load()
    {
        return Load(BrowserKind.Chrome);
    }

    public BrowserState? Load(BrowserKind browserKind)
    {
        var stateFile = BrowserPaths.GetStateFile(browserKind);
        if (!File.Exists(stateFile))
        {
            return null;
        }

        var values = File
            .ReadAllLines(stateFile)
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length is 2)
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);

        if (!values.TryGetValue(nameof(BrowserState.ProcessId), out var processIdValue) ||
            !int.TryParse(processIdValue, out var processId))
        {
            return null;
        }

        _ = int.TryParse(values.GetValueOrDefault(nameof(BrowserState.RemoteDebuggingPort)), out var port);

        return new BrowserState(
            processId,
            port,
            values.GetValueOrDefault(nameof(BrowserState.RemoteDebuggingUrl)) ?? string.Empty,
            values.GetValueOrDefault(nameof(BrowserState.UserDataDirectory)) ?? string.Empty);
    }

    public void Save(BrowserState state)
    {
        Save(BrowserKind.Chrome, state);
    }

    public void Save(BrowserKind browserKind, BrowserState state)
    {
        BrowserPaths.EnsureAppDataDirectory();

        File.WriteAllLines(BrowserPaths.GetStateFile(browserKind), [
            $"{nameof(BrowserState.ProcessId)}={state.ProcessId}",
            $"{nameof(BrowserState.RemoteDebuggingPort)}={state.RemoteDebuggingPort}",
            $"{nameof(BrowserState.RemoteDebuggingUrl)}={state.RemoteDebuggingUrl}",
            $"{nameof(BrowserState.UserDataDirectory)}={state.UserDataDirectory}"
        ]);
    }

    public void Clear()
    {
        Clear(BrowserKind.Chrome);
    }

    public void Clear(BrowserKind browserKind)
    {
        var stateFile = BrowserPaths.GetStateFile(browserKind);
        if (File.Exists(stateFile))
        {
            File.Delete(stateFile);
        }
    }
}
