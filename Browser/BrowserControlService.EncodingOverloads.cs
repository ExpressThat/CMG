using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser;

public sealed partial class BrowserControlService
{
    public ScriptRunResult RunScriptText(
        BrowserKind browserKind, int? port, string script, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts,
        string? baseUrl, IReadOnlyDictionary<string, string> variables, GifQuality gifQuality,
        ScriptPointerMotionOptions? pointerMotion, PointerVisualOptions? pointerVisual, PointerVisibility showPointer,
        BrowserCaptionOptions? captionOptions, ClickPulseStyle clickPulse, int holdAfterActionMilliseconds,
        int holdOnFailureMilliseconds, int preClickHoldMilliseconds, int postClickHoldMilliseconds,
        int holdAfterNavigationMilliseconds, int holdAfterAssertionMilliseconds, string? gifTimelinePath,
        int frameDelayMilliseconds) =>
        RunScriptText(browserKind, port, script, gif, trace, timeouts, baseUrl, variables, gifQuality, pointerMotion,
            pointerVisual, showPointer, captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds,
            preClickHoldMilliseconds, postClickHoldMilliseconds, holdAfterNavigationMilliseconds,
            holdAfterAssertionMilliseconds, gifTimelinePath, frameDelayMilliseconds, gifEncoding: null);
}
