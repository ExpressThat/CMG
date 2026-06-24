namespace CMG.Browser;

public interface IBrowserCommandHandler
{
    int Launch(IReadOnlyList<string> arguments);

    int Close(IReadOnlyList<string> arguments);
}

public sealed class BrowserCommandHandler : IBrowserCommandHandler
{
    private readonly IBrowserController browserController;

    public BrowserCommandHandler(IBrowserController browserController)
    {
        this.browserController = browserController;
    }

    public int Launch(IReadOnlyList<string> arguments)
    {
        var result = browserController.Launch(arguments);

        Console.WriteLine(result.Message);

        if (result.RemoteDebuggingUrl is not null)
        {
            Console.WriteLine($"Remote debugging: {result.RemoteDebuggingUrl}");
        }

        return result.ExitCode;
    }

    public int Close(IReadOnlyList<string> arguments)
    {
        var result = browserController.Close();

        Console.WriteLine(result.Message);

        if (arguments.Count > 0)
        {
            Console.WriteLine($"Ignored arguments: {string.Join(' ', arguments)}");
        }

        return result.ExitCode;
    }
}
