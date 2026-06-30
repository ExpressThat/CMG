using CMG.Browser;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public interface ICmgRunCommandHandler
{
    int Run(
        BrowserKind browserKind,
        string path,
        DirectoryInfo? artifacts,
        FileInfo? jsonReport,
        FileInfo? htmlReport,
        FileInfo? junitReport,
        DirectoryInfo? traceDirectory,
        string? grep,
        string? tag,
        int retries,
        int maxFailures,
        int repeatEach,
        bool listOnly,
        string? shard,
        int? timeout,
        int? navigationTimeout,
        int? assertionTimeout,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables,
        string projectName = "",
        int? browserPort = null,
        bool autoLaunch = false,
        bool autoLaunchHeadless = false,
        GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null,
        ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
        int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
        string? gifTimelinePath = null,
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
        long? gifWarnSizeBytes = null,
        long? gifMaxSizeBytes = null,
        int? gifMaxDurationMilliseconds = null);
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
        FileInfo? junitReport,
        DirectoryInfo? traceDirectory,
        string? grep,
        string? tag,
        int retries,
        int maxFailures,
        int repeatEach,
        bool listOnly,
        string? shard,
        int? timeout,
        int? navigationTimeout,
        int? assertionTimeout,
        string? baseUrl,
        IReadOnlyDictionary<string, string> variables,
        string projectName = "",
        int? browserPort = null,
        bool autoLaunch = false,
        bool autoLaunchHeadless = false,
        GifQuality gifQuality = GifQuality.Highest,
        ScriptPointerMotionOptions? pointerMotion = null,
        ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
        int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
        int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
        string? gifTimelinePath = null,
        int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
        long? gifWarnSizeBytes = null,
        long? gifMaxSizeBytes = null,
        int? gifMaxDurationMilliseconds = null)
    {
        if (browserKind is BrowserKind.InvalidSelection)
        {
            Console.Error.WriteLine("Use only one browser option: --chrome, --edge, or --firefox.");
            return 1;
        }

        if (browserPort is not null and (< 1 or > 65535))
        {
            Console.Error.WriteLine("--browser-port must be between 1 and 65535.");
            return 1;
        }

        if (!CmgShard.TryParse(shard, out var shardIndex, out var shardCount, out var shardError))
        {
            Console.Error.WriteLine(shardError);
            return 1;
        }

        var result = runService.Run(path, new CmgRunOptions(
            browserKind,
            artifacts,
            jsonReport,
            htmlReport,
            junitReport,
            traceDirectory,
            grep,
            tag,
            Math.Max(0, retries),
            Math.Max(0, maxFailures),
            Math.Max(1, repeatEach),
            listOnly,
            shardIndex,
            shardCount,
            NonNegative(timeout),
            NonNegative(navigationTimeout),
            NonNegative(assertionTimeout),
            baseUrl,
            variables,
            projectName,
            browserPort,
            autoLaunch,
            autoLaunchHeadless,
            gifQuality,
            pointerMotion,
            clickPulse,
            holdAfterActionMilliseconds,
            holdOnFailureMilliseconds,
            gifTimelinePath,
            frameDelayMilliseconds,
            gifWarnSizeBytes,
            gifMaxSizeBytes,
            gifMaxDurationMilliseconds));
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

        foreach (var error in FailedTestErrors(result.Tests))
        {
            Console.Error.WriteLine(error);
        }

        return result.Success ? 0 : 1;
    }

    internal static IReadOnlyList<string> FailedTestErrors(IEnumerable<CmgTestResult> tests)
    {
        return tests
            .Where(test => !test.Success && !string.IsNullOrWhiteSpace(test.Error))
            .Where(test => !test.Steps.Any(step => !step.Success && step.Error == test.Error))
            .Select(test => $"TEST ERROR {test.Name} reason={test.Error}")
            .ToArray();
    }

    private static int? NonNegative(int? value) => value is null ? null : Math.Max(0, value.Value);
}
