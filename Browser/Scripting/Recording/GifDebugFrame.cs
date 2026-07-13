namespace CMG.Browser.Scripting.Recording;

public sealed record GifDebugFrame(
    int FrameIndex,
    int StartTimeMilliseconds,
    int DelayMilliseconds,
    string Kind,
    string? Action,
    int? LineNumber,
    string Context,
    string? Target,
    ElementPoint Pointer);
