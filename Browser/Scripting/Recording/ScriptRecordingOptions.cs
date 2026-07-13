namespace CMG.Browser.Scripting.Recording;

public sealed record ScriptRecordingOptions(
    string OutputPath,
    GifQuality Quality = GifQuality.Highest,
    ScriptPointerMotionOptions? PointerMotion = null,
    PointerVisualOptions? PointerVisual = null,
    PointerVisibility ShowPointer = PointerVisibility.Auto,
    ClickPulseStyle ClickPulse = ClickPulseStyle.Ring,
    int HoldAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
    int HoldOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds,
    int PreClickHoldMilliseconds = 0,
    int PostClickHoldMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
    int HoldAfterNavigationMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
    int HoldAfterAssertionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
    string? TimelinePath = null,
    int FrameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds,
    GifEncodingOptions? Encoding = null,
    GifFramingOptions? Framing = null,
    GifRedactionOptions? Redaction = null,
    GifAccessibilityOptions? Accessibility = null)
{
    public const int DefaultFrameDelayMilliseconds = 100;

    public const int HoldFrameDelayCentiseconds = 35;

    public const int DefaultHoldAfterActionMilliseconds = HoldFrameDelayCentiseconds * 10;

    public const int DefaultHoldOnFailureMilliseconds = 1200;

    public const int MovementFrameCount = 8;

    public ScriptPointerMotionOptions EffectivePointerMotion => PointerMotion ?? ScriptPointerMotionOptions.Default;

    public PointerVisualOptions EffectivePointerVisual => PointerVisual ?? PointerVisualOptions.Default;

    public GifEncodingOptions EffectiveEncoding => Encoding ?? new();

    public GifFramingOptions EffectiveFraming => Framing ?? Encoding?.Framing ?? new();

    public GifRedactionOptions EffectiveRedaction => Redaction ?? new();

    public GifAccessibilityOptions EffectiveAccessibility => Accessibility ?? Encoding?.Accessibility ?? new();

    public GifEventCaptionOptions EffectiveEventCaptions => Encoding?.EventCaptions ?? new();

    public GifTitleCardOptions EffectiveTitleCards => Encoding?.TitleCards ?? new();

    public int FrameDelayCentiseconds => Math.Max(1, (FrameDelayMilliseconds + 9) / 10);
}
