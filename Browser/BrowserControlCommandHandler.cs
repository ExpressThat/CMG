using CMG.Browser.Scripting;

namespace CMG.Browser;

public interface IBrowserControlCommandHandler
{
    int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output);

    int RunScript(BrowserKind browserKind, string file, FileInfo? gif);

    int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace) =>
        RunScript(browserKind, file, gif);

    int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts) =>
        RunScript(browserKind, file, gif, trace);

    int RunScript(
        BrowserKind browserKind,
        string file,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables) =>
        RunScript(browserKind, file, gif, trace, timeouts);

    int RunScriptAction(BrowserKind browserKind, string scriptLine);

    int ValidateScript(string file);
}

public sealed class BrowserControlCommandHandler : IBrowserControlCommandHandler
{
    private readonly IBrowserControlService browserControlService;
    private readonly BrowserScriptValidator scriptValidator;

    public BrowserControlCommandHandler(
        IBrowserControlService browserControlService,
        BrowserScriptValidator scriptValidator)
    {
        this.browserControlService = browserControlService;
        this.scriptValidator = scriptValidator;
    }

    public int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output)
    {
        if (!ValidateBrowserSelection(browserKind))
        {
            return 1;
        }

        if (html == screenshot)
        {
            Console.Error.WriteLine("Specify exactly one output mode: --html or --screenshot.");
            return 1;
        }

        var result = browserControlService.GetElement(browserKind, selector, html ? ElementOutputMode.Html : ElementOutputMode.Screenshot);

        if (!result.Success)
        {
            Console.Error.WriteLine(result.Error);
            return 1;
        }

        if (html)
        {
            Console.WriteLine(result.Html);
            return 0;
        }

        if (result.ScreenshotPng is null)
        {
            Console.Error.WriteLine("Screenshot capture did not return image data.");
            return 1;
        }

        if (output is not null)
        {
            var directory = output.Directory;
            if (directory is not null && !directory.Exists)
            {
                directory.Create();
            }

            File.WriteAllBytes(output.FullName, result.ScreenshotPng);
            Console.WriteLine(output.FullName);
            return 0;
        }

        Console.WriteLine($"data:image/png;base64,{Convert.ToBase64String(result.ScreenshotPng)}");
        return 0;
    }

    public int RunScript(BrowserKind browserKind, string file, FileInfo? gif)
    {
        return RunScript(browserKind, file, gif, trace: null);
    }

    public int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace)
    {
        return RunScript(browserKind, file, gif, trace, timeouts: null);
    }

    public int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts)
    {
        return RunScript(browserKind, file, gif, trace, timeouts, baseUrl: null, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }

    public int RunScript(
        BrowserKind browserKind,
        string file,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables)
    {
        if (!ValidateBrowserSelection(browserKind))
        {
            return 1;
        }

        var result = browserControlService.RunScript(browserKind, file, gif, trace, timeouts, baseUrl, variables);

        return WriteScriptResult(result);
    }

    public int ValidateScript(string file)
    {
        var result = scriptValidator.ValidateFile(file);
        if (result.Success)
        {
            Console.WriteLine($"SCRIPT VALID actions={result.ActionCount}");
            return 0;
        }

        Console.Error.WriteLine(result.Error);
        return 1;
    }

    public int RunScriptAction(BrowserKind browserKind, string scriptLine)
    {
        if (!ValidateBrowserSelection(browserKind))
        {
            return 1;
        }

        var result = browserControlService.RunScriptAction(browserKind, scriptLine);

        return WriteScriptResult(result);
    }

    private static bool ValidateBrowserSelection(BrowserKind browserKind)
    {
        if (browserKind is not BrowserKind.InvalidSelection)
        {
            return true;
        }

        Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
        return false;
    }

    private static int WriteScriptResult(ScriptRunResult result)
    {
        foreach (var line in result.StdoutLines)
        {
            Console.WriteLine(line);
        }

        if (!result.Success && !result.Skipped && !string.IsNullOrWhiteSpace(result.Error))
        {
            Console.Error.WriteLine(result.Error);
        }

        return result.Success || result.Skipped ? 0 : 1;
    }
}
