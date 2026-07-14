namespace CMG.Browser.Scripting.Recording;

public enum PointerContrastMode { Auto, Fixed }

public enum PointerTargetCalloutMode { Auto, Always, None }

public enum PointerIdleMode { Pulse, None }

public sealed record GifPointerEvidenceOptions(
    PointerContrastMode Contrast = PointerContrastMode.Auto,
    PointerTargetCalloutMode TargetCallout = PointerTargetCalloutMode.Auto,
    int TargetCalloutThreshold = 24,
    PointerTargetCalloutMode TargetZoom = PointerTargetCalloutMode.Auto,
    int TargetZoomThreshold = 24,
    PointerTargetCalloutMode PagePosition = PointerTargetCalloutMode.Auto,
    bool FocusPulse = true,
    PointerIdleMode Idle = PointerIdleMode.Pulse,
    int IdleThresholdMilliseconds = 1200,
    bool TeleportMarker = true,
    int MouseDownHoldMilliseconds = 500)
{
    public GifPointerEvidenceOptions WithOptions(
        IReadOnlyDictionary<string, string> options,
        string source)
    {
        return this with
        {
            Contrast = options.TryGetValue("pointerContrast", out var contrast) ? ParseContrast(contrast, source) : Contrast,
            TargetCallout = options.TryGetValue("targetCallout", out var callout) ? ParseCallout(callout, source) : TargetCallout,
            TargetCalloutThreshold = ParseInt(options, "targetCalloutThreshold", TargetCalloutThreshold, 8, 100, source),
            TargetZoom = options.TryGetValue("targetZoom", out var zoom) ? ParseMode(zoom, "targetZoom", source) : TargetZoom,
            TargetZoomThreshold = ParseInt(options, "targetZoomThreshold", TargetZoomThreshold, 8, 100, source),
            PagePosition = options.TryGetValue("pagePosition", out var position) ? ParseMode(position, "pagePosition", source) : PagePosition,
            FocusPulse = ParseBool(options, "focusPulse", FocusPulse, source),
            Idle = options.TryGetValue("pointerIdle", out var idle) ? ParseIdle(idle, source) : Idle,
            IdleThresholdMilliseconds = ParseInt(options, "pointerIdleThreshold", IdleThresholdMilliseconds, 100, 60_000, source),
            TeleportMarker = ParseBool(options, "teleportMarker", TeleportMarker, source),
            MouseDownHoldMilliseconds = ParseInt(options, "mouseDownHold", MouseDownHoldMilliseconds, 0, 60_000, source)
        };
    }

    public static GifPointerEvidenceOptions FromOptions(IReadOnlyDictionary<string, string> options, string source) =>
        new GifPointerEvidenceOptions().WithOptions(options, source);

    public static bool TryParse(
        string? contrast,
        string? callout,
        int? calloutThreshold,
        string? targetZoom,
        int? targetZoomThreshold,
        string? pagePosition,
        bool disableFocusPulse,
        string? idle,
        int? idleThreshold,
        bool disableTeleportMarker,
        int? mouseDownHold,
        out GifPointerEvidenceOptions evidence,
        out string? error)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Add(values, "pointerContrast", contrast);
        Add(values, "targetCallout", callout);
        Add(values, "targetCalloutThreshold", calloutThreshold);
        Add(values, "targetZoom", targetZoom);
        Add(values, "targetZoomThreshold", targetZoomThreshold);
        Add(values, "pagePosition", pagePosition);
        Add(values, "focusPulse", disableFocusPulse ? false : null);
        Add(values, "pointerIdle", idle);
        Add(values, "pointerIdleThreshold", idleThreshold);
        Add(values, "teleportMarker", disableTeleportMarker ? false : null);
        Add(values, "mouseDownHold", mouseDownHold);
        try { evidence = FromOptions(values, "GIF"); error = null; return true; }
        catch (ScriptExecutionException exception) { evidence = new(); error = exception.Message; return false; }
    }

    public static string ContrastValues => "auto, fixed";
    public static string CalloutValues => "auto, always, none";
    public static string IdleValues => "pulse, none";

    private static PointerContrastMode ParseContrast(string value, string source) => value.Trim().ToLowerInvariant() switch
    {
        "auto" => PointerContrastMode.Auto,
        "fixed" or "off" or "none" => PointerContrastMode.Fixed,
        _ => throw new ScriptExecutionException($"{source} option pointerContrast= must be one of: {ContrastValues}.")
    };

    private static PointerTargetCalloutMode ParseCallout(string value, string source) => value.Trim().ToLowerInvariant() switch
    {
        "auto" => PointerTargetCalloutMode.Auto,
        "always" or "true" or "on" => PointerTargetCalloutMode.Always,
        "none" or "false" or "off" => PointerTargetCalloutMode.None,
        _ => throw new ScriptExecutionException($"{source} option targetCallout= must be one of: {CalloutValues}.")
    };

    private static PointerTargetCalloutMode ParseMode(string value, string name, string source) => value.Trim().ToLowerInvariant() switch
    {
        "auto" => PointerTargetCalloutMode.Auto,
        "always" or "true" or "on" => PointerTargetCalloutMode.Always,
        "none" or "false" or "off" => PointerTargetCalloutMode.None,
        _ => throw new ScriptExecutionException($"{source} option {name}= must be one of: {CalloutValues}.")
    };

    private static PointerIdleMode ParseIdle(string value, string source) => value.Trim().ToLowerInvariant() switch
    {
        "pulse" or "auto" => PointerIdleMode.Pulse,
        "none" or "off" or "false" => PointerIdleMode.None,
        _ => throw new ScriptExecutionException($"{source} option pointerIdle= must be one of: {IdleValues}.")
    };

    private static int ParseInt(IReadOnlyDictionary<string, string> options, string name, int fallback, int min, int max, string source) =>
        !options.TryGetValue(name, out var value) ? fallback : int.TryParse(value, out var parsed) && parsed >= min && parsed <= max
            ? parsed : throw new ScriptExecutionException($"{source} option {name}= must be an integer from {min} to {max}.");

    private static bool ParseBool(IReadOnlyDictionary<string, string> options, string name, bool fallback, string source) =>
        !options.TryGetValue(name, out var value) ? fallback : value.Trim().ToLowerInvariant() switch
        {
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{source} option {name}= must be true or false.")
        };

    private static void Add(IDictionary<string, string> values, string name, object? value)
    {
        if (value is not null) values[name] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!;
    }
}
