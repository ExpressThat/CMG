using CMG.Browser;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public sealed record CmgDocument(string SourcePath, IReadOnlyList<CmgNode> Nodes);

public sealed record CmgNode(
    int LineNumber,
    string Kind,
    string Name,
    IReadOnlyList<string> Arguments,
    IReadOnlyDictionary<string, string> Options,
    IReadOnlyList<CmgNode> Children);

public sealed record CmgParseResult(bool Success, CmgDocument? Document, string? Error)
{
    public static CmgParseResult Ok(CmgDocument document) => new(true, document, null);

    public static CmgParseResult Fail(string error) => new(false, null, error);
}

public sealed record CmgTestCase(
    string SourcePath,
    string Name,
    IReadOnlyList<CmgNode> Actions,
    IReadOnlyDictionary<string, string> Options)
{
    public string? SuiteName { get; init; }
    public IReadOnlyList<CmgNode> RootBeforeAll { get; init; } = [];
    public IReadOnlyList<CmgNode> RootAfterAll { get; init; } = [];
    public IReadOnlyList<CmgNode> SuiteBeforeAll { get; init; } = [];
    public IReadOnlyList<CmgNode> SuiteAfterAll { get; init; } = [];
    public IReadOnlyList<CmgAnnotation> Annotations { get; init; } = [];
}

public sealed record CmgAnnotation(string Type, string Description);

public sealed record CmgRunOptions(
    BrowserKind BrowserKind,
    DirectoryInfo? GifDirectory,
    FileInfo? JsonReport,
    FileInfo? HtmlReport,
    FileInfo? JUnitReport,
    DirectoryInfo? TraceDirectory,
    string? Grep,
    string? Tag,
    int Retries,
    int MaxFailures,
    int RepeatEach,
    bool ListOnly,
    int ShardIndex,
    int ShardCount,
    int? DefaultTimeout,
    int? NavigationTimeout,
    int? AssertionTimeout,
    string? BaseUrl,
    IReadOnlyDictionary<string, string> Variables,
    string ProjectName = "",
    int? BrowserPort = null,
    bool AutoLaunch = false,
    bool AutoLaunchHeadless = false,
    GifQuality GifQuality = GifQuality.Highest,
    ScriptPointerMotionOptions? PointerMotion = null,
    PointerVisualOptions? PointerVisual = null,
    ClickPulseStyle ClickPulse = ClickPulseStyle.Ring,
    int HoldAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
    int HoldOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
    int PreClickHoldMilliseconds = 0,
    int PostClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
    int HoldAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
    int HoldAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
    string? GifTimelinePath = null,
    int FrameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
    long? GifWarnSizeBytes = null,
    long? GifMaxSizeBytes = null,
    int? GifMaxDurationMilliseconds = null);

public sealed partial record CmgRunResult(
    bool Success,
    IReadOnlyList<string> StdoutLines,
    IReadOnlyList<CmgTestResult> Tests,
    string? Error);

public sealed record CmgTestResult(
    string Name,
    string SourcePath,
    bool Success,
    IReadOnlyList<string> Output,
    string? Error,
    string? GifPath,
    IReadOnlyList<CmgStepResult> Steps)
{
    public string Tags { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Project { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> GifQualities { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<CmgAnnotation> Annotations { get; init; } = [];
}
