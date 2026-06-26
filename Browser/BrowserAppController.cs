using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CMG.Browser;

public interface IBrowserAppController
{
    BrowserLaunchResult Launch(
        BrowserKind browserKind,
        FileInfo executable,
        BrowserAppKind appKind,
        BrowserAppDebugOptions options,
        IReadOnlyList<string> arguments);

    BrowserLaunchResult Attach(BrowserKind browserKind, BrowserAppDebugOptions options, int processId);
}

public sealed class BrowserAppController(BrowserStateStore stateStore) : IBrowserAppController
{
    public BrowserLaunchResult Launch(
        BrowserKind browserKind,
        FileInfo executable,
        BrowserAppKind appKind,
        BrowserAppDebugOptions options,
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

        var remoteDebuggingUrl = RemoteDebuggingUrl(options);
        var startInfo = new ProcessStartInfo
        {
            FileName = executable.FullName,
            UseShellExecute = false
        };

        AddLaunchArguments(startInfo, appKind, options.Port, arguments);

        try
        {
            var process = Process.Start(startInfo);
            if (process is null)
            {
                return new BrowserLaunchResult(1, $"App '{executable.FullName}' did not start.", null);
            }

            if (!WaitForEndpoint(remoteDebuggingUrl, options, out var reason))
            {
                return new BrowserLaunchResult(1, $"App launched, but CMG could not connect to {remoteDebuggingUrl}. Reason: {reason}", null);
            }

            SaveState(browserKind, process.Id, options.Port, remoteDebuggingUrl);
            return new BrowserLaunchResult(0, $"App launched for CMG. PID: {process.Id}.", remoteDebuggingUrl);
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return new BrowserLaunchResult(1, $"Failed to launch app '{executable.FullName}': {exception.Message}", null);
        }
    }

    public BrowserLaunchResult Attach(BrowserKind browserKind, BrowserAppDebugOptions options, int processId)
    {
        var remoteDebuggingUrl = RemoteDebuggingUrl(options);
        if (!WaitForEndpoint(remoteDebuggingUrl, options, out var reason))
        {
            return new BrowserLaunchResult(1, $"Could not attach CMG to {remoteDebuggingUrl}. Reason: {reason}", null);
        }

        SaveState(browserKind, processId, options.Port, remoteDebuggingUrl);
        return new BrowserLaunchResult(0, $"Attached CMG to app debugging endpoint on port {options.Port}.", remoteDebuggingUrl);
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

    private static bool WaitForEndpoint(string remoteDebuggingUrl, BrowserAppDebugOptions options, out string reason)
    {
        if (options.ConnectTimeoutMilliseconds is 0)
        {
            reason = string.Empty;
            return true;
        }

        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(options.ConnectTimeoutMilliseconds);
        Exception? lastException = null;
        do
        {
            if (TryProbeEndpoint(remoteDebuggingUrl, options.ConnectTimeoutMilliseconds, out lastException))
            {
                reason = string.Empty;
                return true;
            }

            Thread.Sleep(100);
        }
        while (DateTimeOffset.UtcNow <= deadline);

        reason = lastException?.Message ?? "No page targets were exposed by /json.";
        return false;
    }

    private static bool TryProbeEndpoint(string remoteDebuggingUrl, int timeoutMilliseconds, out Exception? exception)
    {
        exception = null;
        try
        {
            using var httpClient = new HttpClient { Timeout = ProbeTimeout(timeoutMilliseconds) };
            var json = httpClient.GetStringAsync($"{remoteDebuggingUrl.TrimEnd('/')}/json").GetAwaiter().GetResult();
            return json.Contains("webSocketDebuggerUrl", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception caught) when (caught is HttpRequestException or TaskCanceledException or InvalidOperationException or UriFormatException)
        {
            exception = caught;
            return false;
        }
    }

    private static TimeSpan ProbeTimeout(int timeoutMilliseconds) =>
        TimeSpan.FromMilliseconds(Math.Max(1, Math.Min(timeoutMilliseconds, 2_000)));

    private static string RemoteDebuggingUrl(BrowserAppDebugOptions options) =>
        $"http://{options.Host}:{options.Port}";
}
