namespace CMG.Browser;

public sealed partial class BrowserCommandHandler
{
    public int Launch(
        BrowserKind browserKind,
        IReadOnlyList<string> arguments,
        bool headless,
        string? url,
        int? port,
        int? idleTimeoutMilliseconds,
        bool noIdleCleanup)
    {
        if (browserKind is BrowserKind.InvalidSelection)
        {
            Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
            return 1;
        }
        if (!ValidatePort(port)) return 1;
        if (idleTimeoutMilliseconds is not null && noIdleCleanup)
        {
            Console.Error.WriteLine("Use either --idle-timeout or --no-idle-cleanup, not both.");
            return 1;
        }

        string? environmentError = null;
        if (idleTimeoutMilliseconds is null)
            idleTimeoutMilliseconds = ParseEnvironmentTimeout(out environmentError);
        if (environmentError is not null)
        {
            Console.Error.WriteLine(environmentError);
            return 1;
        }
        if (idleTimeoutMilliseconds is not null && !headless)
        {
            Console.Error.WriteLine("--idle-timeout requires --headless. CMG never automatically closes a visible browser.");
            return 1;
        }

        var result = browserController.Launch(browserKind, BuildLaunchArguments(browserKind, arguments, headless, url), port);
        Console.WriteLine(result.Message);
        if (result.RemoteDebuggingUrl is not null)
        {
            ArmDiagnostics(browserKind, result.RemoteDebuggingUrl);
            Console.WriteLine($"Remote debugging: {result.RemoteDebuggingUrl}");
        }
        if (result.ExitCode is 0 && (idleTimeoutMilliseconds is not null || noIdleCleanup))
        {
            var lease = noIdleCleanup
                ? leaseManager.Disable(browserKind, port)
                : leaseManager.Enable(browserKind, port, idleTimeoutMilliseconds!.Value);
            Console.WriteLine(lease.Message);
            if (lease.ExitCode is not 0) return lease.ExitCode;
        }
        return result.ExitCode;
    }

    public int LeaseStatus(BrowserKind browserKind, int? port) => WriteLease(leaseManager.Status(browserKind, port));

    public int LeaseKeepAlive(BrowserKind browserKind, int? port, int? idleTimeoutMilliseconds) =>
        WriteLease(leaseManager.KeepAlive(browserKind, port, idleTimeoutMilliseconds));

    public int LeaseDisable(BrowserKind browserKind, int? port) => WriteLease(leaseManager.Disable(browserKind, port));

    public int LeaseMonitor(BrowserKind browserKind, int? port, string token) =>
        WriteLease(leaseManager.Monitor(browserKind, port, token));

    private static int WriteLease(BrowserLeaseResult result)
    {
        (result.ExitCode is 0 ? Console.Out : Console.Error).WriteLine(result.Message);
        return result.ExitCode;
    }

    private static int? ParseEnvironmentTimeout(out string? error)
    {
        var value = Environment.GetEnvironmentVariable("CMG_BROWSER_IDLE_TIMEOUT");
        if (string.IsNullOrWhiteSpace(value))
        {
            error = null;
            return null;
        }
        return Commands.BrowserIdleTimeoutParser.TryParse(value, out var timeout, out error) ? timeout : null;
    }
}
