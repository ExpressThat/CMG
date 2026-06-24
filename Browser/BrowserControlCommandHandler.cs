using CMG.Browser.Scripting;

namespace CMG.Browser;

public interface IBrowserControlCommandHandler
{
    int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output);

    int RunScript(BrowserKind browserKind, string file, FileInfo? gif);

    int RunScriptAction(BrowserKind browserKind, string scriptLine);
}

public sealed class BrowserControlCommandHandler : IBrowserControlCommandHandler
{
    private readonly IBrowserControlService browserControlService;

    public BrowserControlCommandHandler(IBrowserControlService browserControlService)
    {
        this.browserControlService = browserControlService;
    }

    public int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output)
    {
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
        var result = browserControlService.RunScript(browserKind, file, gif);

        return WriteScriptResult(result);
    }

    public int RunScriptAction(BrowserKind browserKind, string scriptLine)
    {
        var result = browserControlService.RunScriptAction(browserKind, scriptLine);

        return WriteScriptResult(result);
    }

    private static int WriteScriptResult(ScriptRunResult result)
    {
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
