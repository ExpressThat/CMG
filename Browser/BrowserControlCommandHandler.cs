namespace CMG.Browser;

public interface IBrowserControlCommandHandler
{
    int GetElement(string selector, bool html, bool screenshot, FileInfo? output);
}

public sealed class BrowserControlCommandHandler : IBrowserControlCommandHandler
{
    private readonly IBrowserControlService browserControlService;

    public BrowserControlCommandHandler(IBrowserControlService browserControlService)
    {
        this.browserControlService = browserControlService;
    }

    public int GetElement(string selector, bool html, bool screenshot, FileInfo? output)
    {
        if (html == screenshot)
        {
            Console.Error.WriteLine("Specify exactly one output mode: --html or --screenshot.");
            return 1;
        }

        var result = browserControlService.GetElement(selector, html ? ElementOutputMode.Html : ElementOutputMode.Screenshot);

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
}
