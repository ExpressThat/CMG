namespace CMG.Browser.Scripting.Recording;

public sealed record GifTimelineStep(
    int Sequence,
    int LineNumber,
    string Action,
    string Context,
    bool Success,
    int StartFrameIndex,
    int? EndFrameIndex,
    int StartTimeMilliseconds,
    int EndTimeMilliseconds,
    int? FailureFrameIndex,
    string? Error);

internal sealed record GifTimelineStepStart(
    int Sequence,
    int LineNumber,
    string Action,
    string Context,
    int StartFrameIndex,
    int StartTimeMilliseconds);
