namespace CMG.Browser;

public interface IBrowserCommandHandler
{
    int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments, bool headless, string? url);

    int Close(BrowserKind browserKind, IReadOnlyList<string> arguments);
}

public sealed class BrowserCommandHandler : IBrowserCommandHandler
{
    private readonly IBrowserController browserController;

    public BrowserCommandHandler(IBrowserController browserController)
    {
        this.browserController = browserController;
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
}
