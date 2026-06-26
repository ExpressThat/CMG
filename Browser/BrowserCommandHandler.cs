namespace CMG.Browser;

public interface IBrowserCommandHandler
{
    int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments, bool headless, string? url);

    int LaunchApp(BrowserKind browserKind, FileInfo executable, string kind, int port, IReadOnlyList<string> arguments);

    int AttachApp(BrowserKind browserKind, int port, int processId);

    int Close(BrowserKind browserKind, IReadOnlyList<string> arguments);
}

public sealed class BrowserCommandHandler : IBrowserCommandHandler
{
    private readonly IBrowserController browserController;
    private readonly IBrowserAppController browserAppController;

    public BrowserCommandHandler(IBrowserController browserController, IBrowserAppController browserAppController)
    {
        this.browserController = browserController;
        this.browserAppController = browserAppController;
    }

    public int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments, bool headless, string? url)
    {
        if (browserKind is BrowserKind.InvalidSelection)
        {
            Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
            return 1;
        }

        var result = browserController.Launch(browserKind, BuildLaunchArguments(browserKind, arguments, headless, url));

        Console.WriteLine(result.Message);

        if (result.RemoteDebuggingUrl is not null)
        {
            Console.WriteLine($"Remote debugging: {result.RemoteDebuggingUrl}");
        }

        return result.ExitCode;
    }

    public int LaunchApp(BrowserKind browserKind, FileInfo executable, string kind, int port, IReadOnlyList<string> arguments)
    {
        if (!ValidateAppTarget(browserKind, port, out var exitCode))
        {
            return exitCode;
        }

        if (!BrowserAppKindParser.TryParse(kind, out var appKind))
        {
            Console.Error.WriteLine("App kind must be 'electron' or 'webview2'.");
            return 1;
        }

        var result = browserAppController.Launch(browserKind, executable, appKind, port, arguments);
        WriteLaunchResult(result);
        return result.ExitCode;
    }

    public int AttachApp(BrowserKind browserKind, int port, int processId)
    {
        if (!ValidateAppTarget(browserKind, port, out var exitCode))
        {
            return exitCode;
        }

        if (processId < 0)
        {
            Console.Error.WriteLine("--pid must be 0 or greater.");
            return 1;
        }

        var result = browserAppController.Attach(browserKind, port, processId);
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
        if (browserKind is BrowserKind.InvalidSelection)
        {
            Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
            return 1;
        }

        var result = browserController.Close(browserKind);

        Console.WriteLine(result.Message);

        if (arguments.Count > 0)
        {
            Console.WriteLine($"Ignored arguments: {string.Join(' ', arguments)}");
        }

        return result.ExitCode;
    }

    private static bool ValidateAppTarget(BrowserKind browserKind, int port, out int exitCode)
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

        if (port < 1 || port > 65535)
        {
            Console.Error.WriteLine("--port must be between 1 and 65535.");
            return false;
        }

        exitCode = 0;
        return true;
    }

    private static void WriteLaunchResult(BrowserLaunchResult result)
    {
        Console.WriteLine(result.Message);
        if (result.RemoteDebuggingUrl is not null)
        {
            Console.WriteLine($"Remote debugging: {result.RemoteDebuggingUrl}");
        }
    }
}
