using System.Diagnostics;

namespace CMG.Browser;

public interface IBrowserController
{
    BrowserLaunchResult Launch(IReadOnlyList<string> additionalArguments);

    BrowserCloseResult Close();
}

public sealed class BrowserController : IBrowserController
{
    private const int RemoteDebuggingPort = 9222;
    private readonly BrowserStateStore stateStore;

    public BrowserController(BrowserStateStore stateStore)
    {
        this.stateStore = stateStore;
    }

    public BrowserLaunchResult Launch(IReadOnlyList<string> additionalArguments)
    {
        if (TryGetRunningBrowser(out var runningState))
        {
            return new BrowserLaunchResult(
                0,
                $"Chrome is already running for CMG. PID: {runningState.ProcessId}.",
                runningState.RemoteDebuggingUrl);
        }

        stateStore.Clear();

        var chromePath = ChromeExecutableLocator.Find();
        if (chromePath is null)
        {
            return new BrowserLaunchResult(
                1,
                "Could not find Chrome. Install Chrome or add chrome.exe to PATH.",
                null);
        }

        Directory.CreateDirectory(BrowserPaths.UserDataDirectory);

        var remoteDebuggingUrl = $"http://127.0.0.1:{RemoteDebuggingPort}";
        var processStartInfo = new ProcessStartInfo
        {
            FileName = chromePath,
            UseShellExecute = true
        };

        foreach (var argument in BuildChromeArguments(additionalArguments))
        {
            processStartInfo.ArgumentList.Add(argument);
        }

        try
        {
            var process = Process.Start(processStartInfo);
            if (process is null)
            {
                return new BrowserLaunchResult(1, "Chrome did not start.", null);
            }

            stateStore.Save(new BrowserState(
                process.Id,
                RemoteDebuggingPort,
                remoteDebuggingUrl,
                BrowserPaths.UserDataDirectory));

            return new BrowserLaunchResult(
                0,
                $"Chrome launched for CMG. PID: {process.Id}.",
                remoteDebuggingUrl);
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return new BrowserLaunchResult(1, $"Failed to launch Chrome: {exception.Message}", null);
        }
    }

    public BrowserCloseResult Close()
    {
        var state = stateStore.Load();
        if (state is null)
        {
            return new BrowserCloseResult(0, "No CMG-controlled Chrome instance is running.");
        }

        if (!TryGetProcess(state.ProcessId, out var process))
        {
            stateStore.Clear();
            return new BrowserCloseResult(0, "CMG-controlled Chrome was not running. Cleared stale browser state.");
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
            stateStore.Clear();

            return new BrowserCloseResult(0, $"Closed CMG-controlled Chrome. PID: {state.ProcessId}.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return new BrowserCloseResult(1, $"Failed to close Chrome: {exception.Message}");
        }
    }

    private bool TryGetRunningBrowser(out BrowserState state)
    {
        state = stateStore.Load() ?? BrowserState.Empty;

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

    private static IEnumerable<string> BuildChromeArguments(IReadOnlyList<string> additionalArguments)
    {
        var arguments = new List<string>
        {
            $"--remote-debugging-port={RemoteDebuggingPort}",
            $"--user-data-dir={BrowserPaths.UserDataDirectory}",
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
}

public sealed record BrowserLaunchResult(int ExitCode, string Message, string? RemoteDebuggingUrl);

public sealed record BrowserCloseResult(int ExitCode, string Message);
