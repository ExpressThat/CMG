using CMG.Browser;

namespace CMG.Runner;

public interface ICmgRunCommandHandler
{
    int Run(
        BrowserKind browserKind,
        string path,
        DirectoryInfo? artifacts,
        FileInfo? jsonReport,
        FileInfo? htmlReport,
        FileInfo? junitReport);
}

public sealed class CmgRunCommandHandler : ICmgRunCommandHandler
{
    private readonly ICmgRunService runService;

    public CmgRunCommandHandler(ICmgRunService runService)
    {
        this.runService = runService;
    }

    public int Run(
        BrowserKind browserKind,
        string path,
        DirectoryInfo? artifacts,
        FileInfo? jsonReport,
        FileInfo? htmlReport,
        FileInfo? junitReport)
    {
        if (browserKind is BrowserKind.InvalidSelection)
        {
            Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
            return 1;
        }

        var result = runService.Run(path, new CmgRunOptions(browserKind, artifacts, jsonReport, htmlReport, junitReport));
        foreach (var line in result.StdoutLines)
        {
            Console.WriteLine(line);
        }

        if (!result.Success && !string.IsNullOrWhiteSpace(result.Error))
        {
            Console.Error.WriteLine(result.Error);
        }

        foreach (var failed in result.Tests.SelectMany(test => test.Steps).Where(step => !step.Success))
        {
            Console.Error.WriteLine($"STEP FAIL line={failed.LineNumber} action={failed.Name} reason={failed.Error}");
        }

        return result.Success ? 0 : 1;
    }
}
