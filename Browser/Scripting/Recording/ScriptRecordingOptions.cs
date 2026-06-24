namespace CMG.Browser.Scripting.Recording;

public sealed record ScriptRecordingOptions(string OutputPath)
{
    public const int FrameDelayCentiseconds = 10;

    public const int HoldFrameDelayCentiseconds = 35;

    public const int MovementFrameCount = 8;
}
