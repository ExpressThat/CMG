namespace CMG.Browser.Scripting.Recording;

public sealed record GifTimelineCheckpoint(
    string Name,
    int LineNumber,
    int FrameIndex,
    int TimeMilliseconds);
