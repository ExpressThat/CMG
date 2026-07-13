using CMG.Browser;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public interface ICmgRunCommandHandler
{
    int Run(
        BrowserKind browserKind, string path, DirectoryInfo? artifacts, FileInfo? jsonReport, FileInfo? htmlReport,
        FileInfo? junitReport, DirectoryInfo? traceDirectory, string? grep, string? tag, int retries, int maxFailures,
        int repeatEach, bool listOnly, string? shard, int? timeout, int? navigationTimeout, int? assertionTimeout,
        string? baseUrl, IReadOnlyDictionary<string, string> variables, string projectName = "", int? browserPort = null,
        bool autoLaunch = false, bool autoLaunchHeadless = false, GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null, PointerVisualOptions? pointerVisual = null,
        PointerVisibility showPointer = PointerVisibility.Auto, BrowserCaptionOptions? captionOptions = null,
        ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
        int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds, int preClickHoldMilliseconds = 0,
        int postClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        string? gifTimelinePath = null, int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
        long? gifWarnSizeBytes = null, long? gifMaxSizeBytes = null, int? gifMaxDurationMilliseconds = null);

    int Run(
        BrowserKind browserKind, string path, DirectoryInfo? artifacts, FileInfo? jsonReport, FileInfo? htmlReport,
        FileInfo? junitReport, DirectoryInfo? traceDirectory, string? grep, string? tag, int retries, int maxFailures,
        int repeatEach, bool listOnly, string? shard, int? timeout, int? navigationTimeout, int? assertionTimeout,
        string? baseUrl, IReadOnlyDictionary<string, string> variables, string projectName, int? browserPort,
        bool autoLaunch, bool autoLaunchHeadless, GifQuality gifQuality, ScriptPointerMotionOptions? pointerMotion,
        PointerVisualOptions? pointerVisual, PointerVisibility showPointer, BrowserCaptionOptions? captionOptions,
        ClickPulseStyle clickPulse, int holdAfterActionMilliseconds, int holdOnFailureMilliseconds,
        int preClickHoldMilliseconds, int postClickHoldMilliseconds, int holdAfterNavigationMilliseconds,
        int holdAfterAssertionMilliseconds, string? gifTimelinePath, int frameDelayMilliseconds,
        long? gifWarnSizeBytes, long? gifMaxSizeBytes, int? gifMaxDurationMilliseconds, GifEncodingOptions? gifEncoding,
        int? browserIdleTimeoutMilliseconds = null, bool noBrowserIdleCleanup = false) =>
        Run(browserKind, path, artifacts, jsonReport, htmlReport, junitReport, traceDirectory, grep, tag, retries,
            maxFailures, repeatEach, listOnly, shard, timeout, navigationTimeout, assertionTimeout, baseUrl, variables,
            projectName, browserPort, autoLaunch, autoLaunchHeadless, gifQuality, pointerMotion, pointerVisual, showPointer,
            captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, preClickHoldMilliseconds,
            postClickHoldMilliseconds, holdAfterNavigationMilliseconds, holdAfterAssertionMilliseconds, gifTimelinePath,
            frameDelayMilliseconds, gifWarnSizeBytes, gifMaxSizeBytes, gifMaxDurationMilliseconds);

    int RunWithGifRetention(
        BrowserKind browserKind, string path, DirectoryInfo? artifacts, FileInfo? jsonReport, FileInfo? htmlReport,
        FileInfo? junitReport, DirectoryInfo? traceDirectory, string? grep, string? tag, int retries, int maxFailures,
        int repeatEach, bool listOnly, string? shard, int? timeout, int? navigationTimeout, int? assertionTimeout,
        string? baseUrl, IReadOnlyDictionary<string, string> variables, string projectName, int? browserPort,
        bool autoLaunch, bool autoLaunchHeadless, GifQuality gifQuality, ScriptPointerMotionOptions? pointerMotion,
        PointerVisualOptions? pointerVisual, PointerVisibility showPointer, BrowserCaptionOptions? captionOptions,
        ClickPulseStyle clickPulse, int holdAfterActionMilliseconds, int holdOnFailureMilliseconds,
        int preClickHoldMilliseconds, int postClickHoldMilliseconds, int holdAfterNavigationMilliseconds,
        int holdAfterAssertionMilliseconds, string? gifTimelinePath, int frameDelayMilliseconds,
        long? gifWarnSizeBytes, long? gifMaxSizeBytes, int? gifMaxDurationMilliseconds, GifEncodingOptions? gifEncoding,
        int? browserIdleTimeoutMilliseconds, bool noBrowserIdleCleanup, CmgGifRetentionMode gifRetentionMode,
        int gifRetentionSampleRate, bool gifCleanPassed) =>
        Run(browserKind, path, artifacts, jsonReport, htmlReport, junitReport, traceDirectory, grep, tag, retries,
            maxFailures, repeatEach, listOnly, shard, timeout, navigationTimeout, assertionTimeout, baseUrl, variables,
            projectName, browserPort, autoLaunch, autoLaunchHeadless, gifQuality, pointerMotion, pointerVisual, showPointer,
            captionOptions, clickPulse, holdAfterActionMilliseconds, holdOnFailureMilliseconds, preClickHoldMilliseconds,
            postClickHoldMilliseconds, holdAfterNavigationMilliseconds, holdAfterAssertionMilliseconds, gifTimelinePath,
            frameDelayMilliseconds, gifWarnSizeBytes, gifMaxSizeBytes, gifMaxDurationMilliseconds, gifEncoding,
            browserIdleTimeoutMilliseconds, noBrowserIdleCleanup);
}
