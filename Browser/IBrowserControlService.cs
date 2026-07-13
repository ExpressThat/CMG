using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser;

public interface IBrowserControlService
{
    ElementResult GetElement(BrowserKind browserKind, string selector, ElementOutputMode outputMode);

    ElementResult GetElement(BrowserKind browserKind, int? port, string selector, ElementOutputMode outputMode) =>
        GetElement(browserKind, selector, outputMode);

    ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif);

    ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace) =>
        RunScript(browserKind, file, gif);

    ScriptRunResult RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts) =>
        RunScript(browserKind, file, gif, trace);

    ScriptRunResult RunScript(
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

    ScriptRunResult RunScript(
        BrowserKind browserKind, int? port, string file, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts,
        string? baseUrl, IReadOnlyDictionary<string, string> variables, GifQuality gifQuality,
        ScriptPointerMotionOptions? pointerMotion, PointerVisualOptions? pointerVisual, PointerVisibility showPointer,
        BrowserCaptionOptions? captionOptions, ClickPulseStyle clickPulse, int holdAfterActionMilliseconds,
        int holdOnFailureMilliseconds, int preClickHoldMilliseconds, int postClickHoldMilliseconds,
        int holdAfterNavigationMilliseconds, int holdAfterAssertionMilliseconds, string? gifTimelinePath,
        int frameDelayMilliseconds, GifEncodingOptions? gifEncoding) =>
        RunScript(browserKind, port, file, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion, pointerVisual,
            showPointer, captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds,
            preClickHoldMilliseconds, postClickHoldMilliseconds, holdAfterNavigationMilliseconds,
            holdAfterAssertionMilliseconds, gifTimelinePath, frameDelayMilliseconds);

    ScriptRunResult RunScript(
        BrowserKind browserKind,
        string file,
        FileInfo? gif,
        FileInfo? trace,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables) =>
        RunScript(browserKind, file, gif, trace, timeouts);

    ScriptRunResult RunScriptAction(BrowserKind browserKind, string scriptLine);

    ScriptRunResult RunScriptAction(BrowserKind browserKind, int? port, string scriptLine) =>
        RunScriptAction(browserKind, scriptLine);

    ScriptRunResult RunScriptText(
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
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds);

    ScriptRunResult RunScriptText(
        BrowserKind browserKind, int? port, string script, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts,
        string? baseUrl, IReadOnlyDictionary<string, string> variables, GifQuality gifQuality,
        ScriptPointerMotionOptions? pointerMotion, PointerVisualOptions? pointerVisual, PointerVisibility showPointer,
        BrowserCaptionOptions? captionOptions, ClickPulseStyle clickPulse, int holdAfterActionMilliseconds,
        int holdOnFailureMilliseconds, int preClickHoldMilliseconds, int postClickHoldMilliseconds,
        int holdAfterNavigationMilliseconds, int holdAfterAssertionMilliseconds, string? gifTimelinePath,
        int frameDelayMilliseconds, GifEncodingOptions? gifEncoding) =>
        RunScriptText(browserKind, port, script, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion,
            pointerVisual, showPointer, captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds,
            preClickHoldMilliseconds, postClickHoldMilliseconds, holdAfterNavigationMilliseconds,
            holdAfterAssertionMilliseconds, gifTimelinePath, frameDelayMilliseconds);
}
