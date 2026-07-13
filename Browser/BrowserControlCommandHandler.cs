using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser;

public sealed partial class BrowserControlCommandHandler : IBrowserControlCommandHandler
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
        return GetElement(browserKind, port: null, selector, html, screenshot, output);
    }

    public int GetElement(BrowserKind browserKind, int? port, string selector, bool html, bool screenshot, FileInfo? output)
    {
        if (!ValidateBrowserSelection(browserKind) || !ValidatePort(port))
        {
            return 1;
        }

        if (html == screenshot)
        {
            Console.Error.WriteLine("Specify exactly one output mode: --html or --screenshot.");
            return 1;
        }

        var result = browserControlService.GetElement(browserKind, port, selector, html ? ElementOutputMode.Html : ElementOutputMode.Screenshot);

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
        IReadOnlyDictionary<string, string> variables,
        GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null,
        PointerVisualOptions? pointerVisual = null,
        PointerVisibility showPointer = PointerVisibility.Auto,
        BrowserCaptionOptions? captionOptions = null,
        ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
        int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
        int preClickHoldMilliseconds = 0, int postClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds, int holdAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        string? gifTimelinePath = null,
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
        GifEncodingOptions? gifEncoding = null)
    {
        return RunScript(browserKind, port: null, file, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion, pointerVisual, showPointer, captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, preClickHoldMilliseconds, postClickHoldMilliseconds, holdAfterNavigationMilliseconds, holdAfterAssertionMilliseconds, gifTimelinePath, frameDelayMilliseconds, gifEncoding);
    }

    public int RunScript(
        BrowserKind browserKind,
        int? port,
        string file,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables,
        GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null,
        PointerVisualOptions? pointerVisual = null,
        PointerVisibility showPointer = PointerVisibility.Auto,
        BrowserCaptionOptions? captionOptions = null,
        ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
        int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
        int preClickHoldMilliseconds = 0, int postClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds, int holdAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        string? gifTimelinePath = null,
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
        GifEncodingOptions? gifEncoding = null)
    {
        if (!ValidateBrowserSelection(browserKind) || !ValidatePort(port))
        {
            return 1;
        }

        var result = browserControlService.RunScript(browserKind, port, file, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion, pointerVisual, showPointer, captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, preClickHoldMilliseconds, postClickHoldMilliseconds, holdAfterNavigationMilliseconds, holdAfterAssertionMilliseconds, gifTimelinePath, frameDelayMilliseconds, gifEncoding);

        return WriteScriptResult(result);
    }

    public int RunInlineScript(
        BrowserKind browserKind,
        int? port,
        string script,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables,
        GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null,
        PointerVisualOptions? pointerVisual = null,
        PointerVisibility showPointer = PointerVisibility.Auto,
        BrowserCaptionOptions? captionOptions = null,
        ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
        int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
        int preClickHoldMilliseconds = 0, int postClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds, int holdAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        string? gifTimelinePath = null,
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
        GifEncodingOptions? gifEncoding = null)
    {
        if (!ValidateBrowserSelection(browserKind) || !ValidatePort(port))
        {
            return 1;
        }

        var result = browserControlService.RunScriptText(browserKind, port, script, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion, pointerVisual, showPointer, captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, preClickHoldMilliseconds, postClickHoldMilliseconds, holdAfterNavigationMilliseconds, holdAfterAssertionMilliseconds, gifTimelinePath, frameDelayMilliseconds, gifEncoding);

        return WriteScriptResult(result);
    }

    public int ValidateScript(string file)
    {
        var result = scriptValidator.ValidateFile(file);
        return WriteValidationResult(result);
    }

    public int ValidateInlineScript(string script)
    {
        var result = scriptValidator.ValidateText(script);
        return WriteValidationResult(result);
    }

    public int PreviewGifSettings(string file) => WriteGifSettingsPreview(RecordingSettingsPreviewer.PreviewFile(file));

    public int PreviewInlineGifSettings(string script) => WriteGifSettingsPreview(RecordingSettingsPreviewer.PreviewText(script));

    private static int WriteGifSettingsPreview(RecordingSettingsPreview result)
    {
        if (!result.Success) { Console.Error.WriteLine(result.Error); return 1; }
        foreach (var line in result.Lines) Console.WriteLine(line);
        return 0;
    }

    private static int WriteValidationResult(ScriptValidationResult result)
    {
        if (result.Success)
        {
            if (result.IsRunner)
            {
                Console.WriteLine($"SCRIPT VALID runner suites={result.SuiteCount} tests={result.TestCount} macros={result.MacroCount}");
                return 0;
            }

            Console.WriteLine($"SCRIPT VALID actions={result.ActionCount}");
            return 0;
        }

        Console.Error.WriteLine(result.Error);
        return 1;
    }

    public int RunScriptAction(BrowserKind browserKind, string scriptLine)
    {
        return RunScriptAction(browserKind, port: null, scriptLine);
    }

    public int RunScriptAction(BrowserKind browserKind, int? port, string scriptLine)
    {
        if (!ValidateBrowserSelection(browserKind) || !ValidatePort(port))
        {
            return 1;
        }

        var result = browserControlService.RunScriptAction(browserKind, port, scriptLine);

        return WriteScriptResult(result);
    }

}
