namespace CMG.Browser;

public enum ClickPulseStyle
{
    Ring,
    Ripple,
    Dot,
    Crosshair,
    None
}

public static class ClickPulseStyleParser
{
    public const string Values = "ring, ripple, dot, crosshair, or none";

    public static bool TryParse(string? value, out ClickPulseStyle style)
    {
        style = ClickPulseStyle.Ring;
        return value?.Trim().ToLowerInvariant() switch
        {
            "ring" => Set(out style, ClickPulseStyle.Ring),
            "ripple" => Set(out style, ClickPulseStyle.Ripple),
            "dot" => Set(out style, ClickPulseStyle.Dot),
            "crosshair" or "cross-hair" => Set(out style, ClickPulseStyle.Crosshair),
            "none" or "off" or "false" => Set(out style, ClickPulseStyle.None),
            _ => false
        };
    }

    private static bool Set(out ClickPulseStyle target, ClickPulseStyle value)
    {
        target = value;
        return true;
    }
}
