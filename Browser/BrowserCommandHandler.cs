namespace CMG.Browser;

public interface IBrowserCommandHandler
{
    int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments, bool headless, string? url);

    int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments, bool headless, string? url, int? port) =>
        Launch(browserKind, arguments, headless, url);

    int Launch(
        BrowserKind browserKind,
        IReadOnlyList<string> arguments,
        bool headless,
        string? url,
        int? port,
        int? idleTimeoutMilliseconds,
        bool noIdleCleanup) => Launch(browserKind, arguments, headless, url, port);

    int LaunchApp(
        BrowserKind browserKind,
        FileInfo executable,
        string kind,
        BrowserAppDebugOptions options,
        IReadOnlyList<string> arguments);

    int AttachApp(BrowserKind browserKind, BrowserAppDebugOptions options, int processId);

    int Close(BrowserKind browserKind, IReadOnlyList<string> arguments);

    int Close(BrowserKind browserKind, IReadOnlyList<string> arguments, int? port) =>
        Close(browserKind, arguments);

    int LeaseStatus(BrowserKind browserKind, int? port) => 0;
    int LeaseKeepAlive(BrowserKind browserKind, int? port, int? idleTimeoutMilliseconds) => 0;
    int LeaseDisable(BrowserKind browserKind, int? port) => 0;
    int LeaseMonitor(BrowserKind browserKind, int? port, string token) => 0;
}

public sealed partial class BrowserCommandHandler : IBrowserCommandHandler
{
    private readonly IBrowserController browserController;
    private readonly IBrowserAppController browserAppController;
    private readonly BrowserAutomationClientFactory automationClientFactory;
    private readonly IBrowserLeaseManager leaseManager;

    public BrowserCommandHandler(
        IBrowserController browserController,
        IBrowserAppController browserAppController,
        BrowserAutomationClientFactory automationClientFactory,
        IBrowserLeaseManager leaseManager)
    {
        this.browserController = browserController;
        this.browserAppController = browserAppController;
        this.automationClientFactory = automationClientFactory;
        this.leaseManager = leaseManager;
    }

    public int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments, bool headless, string? url)
    {
        return Launch(browserKind, arguments, headless, url, port: null);
    }

    public int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments, bool headless, string? url, int? port)
    {
        return Launch(browserKind, arguments, headless, url, port, idleTimeoutMilliseconds: null, noIdleCleanup: false);
    }

    public int LaunchApp(
        BrowserKind browserKind,
        FileInfo executable,
        string kind,
        BrowserAppDebugOptions options,
        IReadOnlyList<string> arguments)
    {
        if (!ValidateAppTarget(browserKind, options, out var exitCode))
        {
            return exitCode;
        }

        if (!BrowserAppKindParser.TryParse(kind, out var appKind))
        {
            Console.Error.WriteLine("App kind must be 'electron' or 'webview2'.");
            return 1;
        }

        var result = browserAppController.Launch(browserKind, executable, appKind, options, arguments);
        if (result.ExitCode is 0 && result.RemoteDebuggingUrl is not null)
        {
            ArmDiagnostics(browserKind, result.RemoteDebuggingUrl);
        }

        WriteLaunchResult(result);
        return result.ExitCode;
    }

    public int AttachApp(BrowserKind browserKind, BrowserAppDebugOptions options, int processId)
    {
        if (!ValidateAppTarget(browserKind, options, out var exitCode))
        {
            return exitCode;
        }

        if (processId < 0)
        {
            Console.Error.WriteLine("--pid must be 0 or greater.");
            return 1;
        }

        var result = browserAppController.Attach(browserKind, options, processId);
        if (result.ExitCode is 0 && result.RemoteDebuggingUrl is not null)
        {
            ArmDiagnostics(browserKind, result.RemoteDebuggingUrl);
        }

        WriteLaunchResult(result);
        return result.ExitCode;
    }

    private static IReadOnlyList<string> BuildLaunchArguments(
        BrowserKind browserKind,
        IReadOnlyList<string> arguments,
        bool headless,
        string? url)
    {
        var values = new List<string>(arguments);
        if (headless)
        {
            values.Add(browserKind.UsesFirefoxBiDi() ? "--headless" : "--headless=new");
        }

        if (!string.IsNullOrWhiteSpace(url))
        {
            values.Add(url);
        }

        return values;
    }

    public int Close(BrowserKind browserKind, IReadOnlyList<string> arguments)
    {
        return Close(browserKind, arguments, port: null);
    }

    public int Close(BrowserKind browserKind, IReadOnlyList<string> arguments, int? port)
    {
        if (browserKind is BrowserKind.InvalidSelection)
        {
            Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
            return 1;
        }

        if (!ValidatePort(port))
        {
            return 1;
        }

        var result = browserController.Close(browserKind, port);

        Console.WriteLine(result.Message);

        if (arguments.Count > 0)
        {
            Console.WriteLine($"Ignored arguments: {string.Join(' ', arguments)}");
        }

        return result.ExitCode;
    }

    private static bool ValidateAppTarget(BrowserKind browserKind, BrowserAppDebugOptions options, out int exitCode)
    {
        exitCode = 1;
        if (browserKind is BrowserKind.InvalidSelection)
        {
            Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
            return false;
        }

        if (browserKind.UsesFirefoxBiDi())
        {
            Console.Error.WriteLine("App attach/launch uses Chromium CDP endpoints. Use --chrome or --edge state.");
            return false;
        }

        if (options.Port < 1 || options.Port > 65535)
        {
            Console.Error.WriteLine("--port must be between 1 and 65535.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            Console.Error.WriteLine("--host must not be empty.");
            return false;
        }

        if (options.ConnectTimeoutMilliseconds < 0)
        {
            Console.Error.WriteLine("--connect-timeout must be 0 or greater.");
            return false;
        }

        exitCode = 0;
        return true;
    }

    private static bool ValidatePort(int? port)
    {
        if (port is null || port is >= 1 and <= 65535)
        {
            return true;
        }

        Console.Error.WriteLine("--port must be between 1 and 65535.");
        return false;
    }

    private static void WriteLaunchResult(BrowserLaunchResult result)
    {
        Console.WriteLine(result.Message);
        if (result.RemoteDebuggingUrl is not null)
        {
            Console.WriteLine($"Remote debugging: {result.RemoteDebuggingUrl}");
        }
    }

    private void ArmDiagnostics(BrowserKind browserKind, string remoteDebuggingUrl)
    {
        try
        {
            automationClientFactory.Create(browserKind).ArmDiagnostics(remoteDebuggingUrl);
        }
        catch (Exception exception) when (exception is ChromeDevToolsException or HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            Console.Error.WriteLine($"Warning: diagnostics capture was not armed: {exception.Message}");
        }
    }
}
