namespace CMG.Runner;

public sealed record CmgStepResult(
    int LineNumber,
    string Name,
    bool Success,
    IReadOnlyList<string> Output,
    string? Error,
    string? GifPath);
