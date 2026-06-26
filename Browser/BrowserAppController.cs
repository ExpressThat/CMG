using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CMG.Browser;

public interface IBrowserAppController
{
    BrowserLaunchResult Launch(
        BrowserKind browserKind,
        FileInfo executable,
        BrowserAppKind appKind,
        int remoteDebuggingPort,
        IReadOnlyList<string> arguments);

    BrowserLaunchResult Attach(BrowserKind browserKind, int remoteDebuggingPort, int processId);
}

public sealed class BrowserAppController(BrowserStateStore stateStore) : IBrowserAppController
{
    public BrowserLaunchResult Launch(
        BrowserKind browserKind,
        FileInfo executable,
        BrowserAppKind appKind,
        int remoteDebuggingPort,
        IReadOnlyList<string> arguments)
    {
        if (!executable.Exists)
        {
            return new BrowserLaunchResult(1, $"App executable '{executable.FullName}' was not found.", null);
        }

        if (appKind is BrowserAppKind.WebView2 && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new BrowserLaunchResult(1, "WebView2 app control is only available on Windows. macOS WKWebView and Linux WebKitGTK do not expose CDP/BiDi.", null);
        }

        var remoteDebuggingUrl = RemoteDebuggingUrl(remoteDebuggingPort);
        var startInfo = new ProcessStartInfo
        {
            FileName = executable.FullName,
            UseShellExecute = false
        };

        AddLaunchArguments(startInfo, appKind, remoteDebuggingPort, arguments);

        try
        {
            var process = Process.Start(startInfo);
            if (process is null)
            {
                return new BrowserLaunchResult(1, $"App '{executable.FullName}' did not start.", null);
            }

            SaveState(browserKind, process.Id, remoteDebuggingPort, remoteDebuggingUrl);
            return new BrowserLaunchResult(0, $"App launched for CMG. PID: {process.Id}.", remoteDebuggingUrl);
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return new BrowserLaunchResult(1, $"Failed to launch app '{executable.FullName}': {exception.Message}", null);
        }
    }

    public BrowserLaunchResult Attach(BrowserKind browserKind, int remoteDebuggingPort, int processId)
    {
        var remoteDebuggingUrl = RemoteDebuggingUrl(remoteDebuggingPort);
        SaveState(browserKind, processId, remoteDebuggingPort, remoteDebuggingUrl);
        return new BrowserLaunchResult(0, $"Attached CMG to app debugging endpoint on port {remoteDebuggingPort}.", remoteDebuggingUrl);
    }

    private void SaveState(BrowserKind browserKind, int processId, int port, string url)
    {
        stateStore.Save(browserKind, new BrowserState(processId, port, url, string.Empty));
    }

    private static void AddLaunchArguments(
        ProcessStartInfo startInfo,
        BrowserAppKind appKind,
        int port,
        IReadOnlyList<string> arguments)
    {
        if (appKind is BrowserAppKind.Electron)
        {
            startInfo.ArgumentList.Add($"--remote-debugging-port={port}");
        }
        else
        {
            AddWebView2DebuggingEnvironment(startInfo, port);
        }

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
    }

    private static void AddWebView2DebuggingEnvironment(ProcessStartInfo startInfo, int port)
    {
        const string variable = "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS";
        var existing = Environment.GetEnvironmentVariable(variable);
        var debuggingArgument = $"--remote-debugging-port={port}";
        startInfo.Environment[variable] = string.IsNullOrWhiteSpace(existing)
            ? debuggingArgument
            : $"{existing} {debuggingArgument}";
    }

    private static string RemoteDebuggingUrl(int remoteDebuggingPort) =>
        $"http://127.0.0.1:{remoteDebuggingPort}";
}
