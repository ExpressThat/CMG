using System.Diagnostics;

namespace CMG.Browser;

public interface IBrowserController
{
    BrowserLaunchResult Launch(BrowserKind browserKind, IReadOnlyList<string> additionalArguments, int? remoteDebuggingPort = null);

    BrowserCloseResult Close(BrowserKind browserKind);

    BrowserCloseResult Close(BrowserKind browserKind, int? port);
}

public sealed class BrowserController : IBrowserController
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
                userDataDirectory));

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

        return state.ProcessId > 0 && TryGetProcess(state.ProcessId, out _);
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

    private static string? FindExecutable(BrowserKind browserKind) =>
        browserKind switch
        {
            BrowserKind.Edge => EdgeExecutableLocator.Find(),
            BrowserKind.Firefox => FirefoxExecutableLocator.Find(),
            _ => ChromeExecutableLocator.Find()
        };

    private static string GetRemoteDebuggingUrl(BrowserKind browserKind, int remoteDebuggingPort) =>
        browserKind.UsesFirefoxBiDi()
            ? $"ws://127.0.0.1:{remoteDebuggingPort}/session"
            : $"http://127.0.0.1:{remoteDebuggingPort}";

    private static IEnumerable<string> BuildBrowserArguments(
        BrowserKind browserKind,
        int remoteDebuggingPort,
        string userDataDirectory,
        IReadOnlyList<string> additionalArguments)
    {
        return browserKind.UsesFirefoxBiDi()
            ? BuildFirefoxArguments(remoteDebuggingPort, userDataDirectory, additionalArguments)
            : BuildChromeArguments(remoteDebuggingPort, userDataDirectory, additionalArguments);
    }

    private static IEnumerable<string> BuildChromeArguments(
        int remoteDebuggingPort,
        string userDataDirectory,
        IReadOnlyList<string> additionalArguments)
    {
        var arguments = new List<string>
        {
            $"--remote-debugging-port={remoteDebuggingPort}",
            $"--user-data-dir={userDataDirectory}",
            "--no-first-run",
            "--no-default-browser-check"
        };

        arguments.AddRange(additionalArguments);

        if (!additionalArguments.Any(argument => !argument.StartsWith("--", StringComparison.Ordinal)))
        {
            arguments.Add("about:blank");
        }

        return arguments;
    }

    private static IEnumerable<string> BuildFirefoxArguments(
        int remoteDebuggingPort,
        string userDataDirectory,
        IReadOnlyList<string> additionalArguments)
    {
        var arguments = new List<string>
        {
            "-no-remote",
            "--remote-debugging-port",
            remoteDebuggingPort.ToString(),
            "--profile",
            userDataDirectory
        };

        arguments.AddRange(additionalArguments);

        if (!additionalArguments.Any(argument => !argument.StartsWith("-", StringComparison.Ordinal)))
        {
            arguments.Add("about:blank");
        }

        return arguments;
    }

    private static void WriteBrowserPreferences(BrowserKind browserKind, string userDataDirectory)
    {
        if (!browserKind.UsesFirefoxBiDi())
        {
            return;
        }

        File.WriteAllLines(Path.Combine(userDataDirectory, "user.js"), [
            "user_pref(\"remote.active-protocols\", 1);"
        ]);
    }
}

public sealed record BrowserLaunchResult(int ExitCode, string Message, string? RemoteDebuggingUrl);

public sealed record BrowserCloseResult(int ExitCode, string Message);
