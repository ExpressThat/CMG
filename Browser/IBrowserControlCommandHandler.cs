using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser;

public interface IBrowserControlCommandHandler
{
    int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output);

    int GetElement(BrowserKind browserKind, int? port, string selector, bool html, bool screenshot, FileInfo? output) =>
        GetElement(browserKind, selector, html, screenshot, output);

    int RunScript(BrowserKind browserKind, string file, FileInfo? gif);

    int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace) =>
        RunScript(browserKind, file, gif);

    int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts) =>
        RunScript(browserKind, file, gif, trace);

    int RunScript(
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
        int preClickHoldMilliseconds = 0,
        int postClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        string? gifTimelinePath = null,
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds) =>
        RunScript(browserKind, file, gif, trace, timeouts, baseUrl, variables);

    int RunInlineScript(
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
        int preClickHoldMilliseconds = 0,
        int postClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        string? gifTimelinePath = null,
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds) => 0;

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

    int RunScriptAction(BrowserKind browserKind, int? port, string scriptLine) =>
        RunScriptAction(browserKind, scriptLine);

    int ValidateScript(string file);

    int ValidateInlineScript(string script) => 0;
}
