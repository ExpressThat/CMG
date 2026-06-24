namespace CMG.Browser;

public interface IBrowserCommandHandler
{
    int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments);

    int Close(BrowserKind browserKind, IReadOnlyList<string> arguments);
}

public sealed class BrowserCommandHandler : IBrowserCommandHandler
{
    private readonly IBrowserController browserController;

    public BrowserCommandHandler(IBrowserController browserController)
    {
        this.browserController = browserController;
    }

    public int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments)
    {
        if (browserKind is BrowserKind.InvalidSelection)
        {
            Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
            return 1;
        }

        var result = browserController.Launch(browserKind, arguments);

        Console.WriteLine(result.Message);

        if (result.RemoteDebuggingUrl is not null)
        {
            Console.WriteLine($"Remote debugging: {result.RemoteDebuggingUrl}");
        }

        return result.ExitCode;
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
