using CMG.Browser;

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
    IReadOnlyDictionary<string, string> Options);

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
    int ShardIndex,
    int ShardCount);

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
}
