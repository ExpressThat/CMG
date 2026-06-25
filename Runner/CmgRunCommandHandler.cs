using CMG.Browser;

namespace CMG.Runner;

public interface ICmgRunCommandHandler
{
    int Run(BrowserKind browserKind, string path, DirectoryInfo? artifacts, FileInfo? jsonReport);
}

public sealed class CmgRunCommandHandler : ICmgRunCommandHandler
{
    private readonly ICmgRunService runService;

    public CmgRunCommandHandler(ICmgRunService runService)
    {
        this.runService = runService;
    }

    public int Run(BrowserKind browserKind, string path, DirectoryInfo? artifacts, FileInfo? jsonReport)
    {
        if (browserKind is BrowserKind.InvalidSelection)
        {
            Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
            return 1;
        }

        var result = runService.Run(path, new CmgRunOptions(browserKind, artifacts, jsonReport));
        foreach (var line in result.StdoutLines)
        {
            Console.WriteLine(line);
        }

        if (!result.Success && !string.IsNullOrWhiteSpace(result.Error))
        {
            Console.Error.WriteLine(result.Error);
        }

        return result.Success ? 0 : 1;
    }
}
