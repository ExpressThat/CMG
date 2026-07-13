using System.Diagnostics;

namespace CMG.Browser;

public interface IBrowserController
{
    BrowserLaunchResult Launch(BrowserKind browserKind, IReadOnlyList<string> additionalArguments, int? remoteDebuggingPort = null);

    BrowserCloseResult Close(BrowserKind browserKind);

    BrowserCloseResult Close(BrowserKind browserKind, int? port);
}

public sealed partial class BrowserController : IBrowserController
{
    private readonly BrowserStateStore stateStore;

    public BrowserController(BrowserStateStore stateStore)
    {
        this.stateStore = stateStore;
    }

    public BrowserLaunchResult Launch(BrowserKind browserKind, IReadOnlyList<string> additionalArguments, int? remoteDebuggingPort = null)
    {
        var port = remoteDebuggingPort ?? browserKind.DefaultRemoteDebuggingPort();
        if (TryGetRunningBrowser(browserKind, port, out var runningState))
        {
            return new BrowserLaunchResult(
                0,
                $"{browserKind.DisplayName()} is already running for CMG. PID: {runningState.ProcessId}.",
                runningState.RemoteDebuggingUrl);
        }

        stateStore.Clear(browserKind, port);

        var executablePath = FindExecutable(browserKind);
        if (executablePath is null)
        {
            return new BrowserLaunchResult(
                1,
                $"Could not find {browserKind.DisplayName()}. Install {browserKind.DisplayName()} or add its executable to PATH.",
                null);
        }

        var userDataDirectory = BrowserPaths.GetUserDataDirectory(browserKind, port);
        Directory.CreateDirectory(userDataDirectory);
        WriteBrowserPreferences(browserKind, userDataDirectory);

        var remoteDebuggingUrl = GetRemoteDebuggingUrl(browserKind, port);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = true
        };

            foreach (var argument in BuildBrowserArguments(browserKind, port, userDataDirectory, additionalArguments))
        {
            processStartInfo.ArgumentList.Add(argument);
        }

        try
        {
            var process = Process.Start(processStartInfo);
            if (process is null)
            {
                return new BrowserLaunchResult(1, $"{browserKind.DisplayName()} did not start.", null);
            }

            if (!BrowserEndpointProbe.WaitUntilReady(browserKind, remoteDebuggingUrl, TimeSpan.FromSeconds(10)))
            {
                process.Kill(entireProcessTree: true);
                stateStore.Clear(browserKind, port);
                return new BrowserLaunchResult(1, $"{browserKind.DisplayName()} launched but did not expose debugging at {remoteDebuggingUrl}.", null);
            }

            stateStore.Save(browserKind, new BrowserState(
                process.Id,
                port,
                remoteDebuggingUrl,
                userDataDirectory,
                IsHeadless(additionalArguments),
                Guid.NewGuid().ToString("N"),
                process.StartTime.ToUniversalTime().Ticks,
                DateTimeOffset.UtcNow.UtcTicks));

            return new BrowserLaunchResult(
                0,
                $"{browserKind.DisplayName()} launched for CMG. PID: {process.Id}.",
                remoteDebuggingUrl);
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return new BrowserLaunchResult(1, $"Failed to launch {browserKind.DisplayName()}: {exception.Message}", null);
        }
    }

    public BrowserCloseResult Close(BrowserKind browserKind)
    {
        return Close(browserKind, port: null);
    }

    public BrowserCloseResult Close(BrowserKind browserKind, int? port)
    {
        var state = stateStore.Load(browserKind, port);
        if (state is null)
        {
            return new BrowserCloseResult(0, $"No CMG-controlled {browserKind.DisplayName()} instance is running.");
        }

        if (!TryGetProcess(state.ProcessId, out var process))
        {
            stateStore.Clear(browserKind, port);
            return new BrowserCloseResult(0, $"CMG-controlled {browserKind.DisplayName()} was not running. Cleared stale browser state.");
        }

        if (!MatchesOwnedProcess(state, process))
        {
            stateStore.Clear(browserKind, port);
            return new BrowserCloseResult(0, $"Skipped closing {browserKind.DisplayName()} because its saved process identity is stale. Cleared stale browser state.");
        }

        try
        {
            if (!process.CloseMainWindow())
            {
                process.Kill(entireProcessTree: true);
            }
            else if (!process.WaitForExit(milliseconds: 5_000))
            {
                process.Kill(entireProcessTree: true);
            }

            process.WaitForExit();
            stateStore.Clear(browserKind, port);

            return new BrowserCloseResult(0, $"Closed CMG-controlled {browserKind.DisplayName()}. PID: {state.ProcessId}.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return new BrowserCloseResult(1, $"Failed to close {browserKind.DisplayName()}: {exception.Message}");
        }
    }

    private bool TryGetRunningBrowser(BrowserKind browserKind, int port, out BrowserState state)
    {
        state = stateStore.Load(browserKind, port) ?? BrowserState.Empty;
        if (state.ProcessId > 0 && TryGetProcess(state.ProcessId, out var process) && MatchesOwnedProcess(state, process))
            return true;
        if (state.ProcessId > 0) stateStore.Clear(browserKind, port);
        return false;
    }

    private static bool TryGetProcess(int processId, out Process process)
    {
        try
        {
            process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            process = null!;
            return false;
        }
        catch (InvalidOperationException)
        {
            process = null!;
            return false;
        }
    }

    private static bool MatchesOwnedProcess(BrowserState state, Process process)
    {
        if (state.ProcessStartTimeUtcTicks <= 0) return true;
        try
        {
            return process.StartTime.ToUniversalTime().Ticks == state.ProcessStartTimeUtcTicks;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

}

public sealed record BrowserLaunchResult(int ExitCode, string Message, string? RemoteDebuggingUrl);

public sealed record BrowserCloseResult(int ExitCode, string Message);
