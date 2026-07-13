using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    public ScriptRunResult Run(
        string file,
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        FileInfo? gif,
        FileInfo? trace = null,
        ScriptTimeoutOptions? timeouts = null,
        string? baseUrl = null,
        IReadOnlyDictionary<string, string>? variables = null,
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
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
        GifEncodingOptions? gifEncoding = null,
        GifRedactionOptions? gifRedaction = null,
        GifAccessibilityOptions? gifAccessibility = null)
    {
        var readResult = ReadScript(file);
        if (!readResult.Success)
        {
            return ScriptRunResult.Fail(readResult.Error ?? "Could not read script.");
        }

        return RunParsedScript(readResult.Script ?? string.Empty, remoteDebuggingUrl, automationClient, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion, pointerVisual, showPointer, captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, preClickHoldMilliseconds, postClickHoldMilliseconds, holdAfterNavigationMilliseconds, holdAfterAssertionMilliseconds, gifTimelinePath, frameDelayMilliseconds, gifEncoding, gifRedaction, gifAccessibility);
    }

    public ScriptRunResult RunText(
        string script,
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        FileInfo? gif = null,
        FileInfo? trace = null,
        ScriptTimeoutOptions? timeouts = null,
        string? baseUrl = null,
        IReadOnlyDictionary<string, string>? variables = null,
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
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
        GifEncodingOptions? gifEncoding = null,
        GifRedactionOptions? gifRedaction = null,
        GifAccessibilityOptions? gifAccessibility = null) =>
        RunParsedScript(script, remoteDebuggingUrl, automationClient, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion, pointerVisual, showPointer, captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, preClickHoldMilliseconds, postClickHoldMilliseconds, holdAfterNavigationMilliseconds, holdAfterAssertionMilliseconds, gifTimelinePath, frameDelayMilliseconds, gifEncoding, gifRedaction, gifAccessibility);
}
