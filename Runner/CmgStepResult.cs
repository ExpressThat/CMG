namespace CMG.Runner;

public sealed record CmgStepResult(
    int LineNumber,
    string Name,
    bool Success,
    IReadOnlyList<string> Output,
    string? Error,
    string? GifPath,
    int Sequence = 0,
    string Context = "",
    string Action = "")
{
    public IReadOnlyList<CmgStepGifEvidence> GifEvidence { get; init; } = [];
}

public sealed record CmgStepGifEvidence(
    string GifPath,
    string TimelinePath,
    int StartFrameIndex,
    int? EndFrameIndex,
    int StartTimeMilliseconds,
    int EndTimeMilliseconds,
    int? FailureFrameIndex,
    int CapturedFrameCount = 0,
    int CapturedDurationMilliseconds = 0,
    long EstimatedRgbaBytes = 0);
