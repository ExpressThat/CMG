using System.Diagnostics;

namespace CMG.Browser;

public interface IBrowserController
{
    BrowserLaunchResult Launch(BrowserKind browserKind, IReadOnlyList<string> additionalArguments);

    BrowserCloseResult Close(BrowserKind browserKind);
}

public sealed class BrowserController : IBrowserController
{
    private const int ChromeRemoteDebuggingPort = 9222;
    private const int FirefoxRemoteDebuggingPort = 9223;
    private readonly BrowserStateStore stateStore;

    public BrowserController(BrowserStateStore stateStore)
    {
        this.stateStore = stateStore;
    }

    public BrowserLaunchResult Launch(BrowserKind browserKind, IReadOnlyList<string> additionalArguments)
    {
        if (TryGetRunningBrowser(browserKind, out var runningState))
        {
            return new BrowserLaunchResult(
                0,
                $"{browserKind.DisplayName()} is already running for CMG. PID: {runningState.ProcessId}.",
                runningState.RemoteDebuggingUrl);
        }

        stateStore.Clear(browserKind);

        var executablePath = FindExecutable(browserKind);
        if (executablePath is null)
        {
            return new BrowserLaunchResult(
                1,
                $"Could not find {browserKind.DisplayName()}. Install {browserKind.DisplayName()} or add its executable to PATH.",
                null);
        }

        var userDataDirectory = BrowserPaths.GetUserDataDirectory(browserKind);
        Directory.CreateDirectory(userDataDirectory);
        WriteBrowserPreferences(browserKind, userDataDirectory);

        var remoteDebuggingPort = GetRemoteDebuggingPort(browserKind);
        var remoteDebuggingUrl = GetRemoteDebuggingUrl(browserKind, remoteDebuggingPort);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = true
        };

        foreach (var argument in BuildBrowserArguments(browserKind, remoteDebuggingPort, userDataDirectory, additionalArguments))
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

            stateStore.Save(browserKind, new BrowserState(
                process.Id,
                remoteDebuggingPort,
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
        var state = stateStore.Load(browserKind);
        if (state is null)
        {
            return new BrowserCloseResult(0, $"No CMG-controlled {browserKind.DisplayName()} instance is running.");
        }

        if (!TryGetProcess(state.ProcessId, out var process))
        {
            stateStore.Clear(browserKind);
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
            stateStore.Clear(browserKind);

            return new BrowserCloseResult(0, $"Closed CMG-controlled {browserKind.DisplayName()}. PID: {state.ProcessId}.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return new BrowserCloseResult(1, $"Failed to close {browserKind.DisplayName()}: {exception.Message}");
        }
    }

    private bool TryGetRunningBrowser(BrowserKind browserKind, out BrowserState state)
    {
        state = stateStore.Load(browserKind) ?? BrowserState.Empty;

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
        browserKind is BrowserKind.Firefox ? FirefoxExecutableLocator.Find() : ChromeExecutableLocator.Find();

    private static int GetRemoteDebuggingPort(BrowserKind browserKind) =>
        browserKind is BrowserKind.Firefox ? FirefoxRemoteDebuggingPort : ChromeRemoteDebuggingPort;

    private static string GetRemoteDebuggingUrl(BrowserKind browserKind, int remoteDebuggingPort) =>
        browserKind is BrowserKind.Firefox
            ? $"ws://127.0.0.1:{remoteDebuggingPort}/session"
            : $"http://127.0.0.1:{remoteDebuggingPort}";

    private static IEnumerable<string> BuildBrowserArguments(
        BrowserKind browserKind,
        int remoteDebuggingPort,
        string userDataDirectory,
        IReadOnlyList<string> additionalArguments)
    {
        return browserKind is BrowserKind.Firefox
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
        if (browserKind is not BrowserKind.Firefox)
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
