namespace CMG.Browser.Scripting.Recording;

public sealed record ScriptRecordingOptions(
    string OutputPath,
    GifQuality Quality = GifQuality.Highest,
    ScriptPointerMotionOptions? PointerMotion = null,
    ClickPulseStyle ClickPulse = ClickPulseStyle.Ring,
    int HoldAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
    int HoldOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds)
{
    public const int FrameDelayCentiseconds = 10;

    public const int HoldFrameDelayCentiseconds = 35;

    public const int DefaultHoldAfterActionMilliseconds = HoldFrameDelayCentiseconds * 10;

    public const int DefaultHoldOnFailureMilliseconds = 1200;

    public const int MovementFrameCount = 8;

    public ScriptPointerMotionOptions EffectivePointerMotion => PointerMotion ?? ScriptPointerMotionOptions.Default;
}
