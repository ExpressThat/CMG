namespace CMG.Browser.Scripting.Recording;

public enum ScriptPointerEasing
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut,
    Spring
}

public static class ScriptPointerEasingParser
{
    public const string Values = "linear, ease-in, ease-out, ease-in-out, or spring";

    public static bool TryParse(string? value, out ScriptPointerEasing easing)
    {
        easing = ScriptPointerEasing.EaseInOut;
        return value?.Trim().ToLowerInvariant() switch
        {
            "linear" => Set(out easing, ScriptPointerEasing.Linear),
            "ease-in" or "easein" => Set(out easing, ScriptPointerEasing.EaseIn),
            "ease-out" or "easeout" => Set(out easing, ScriptPointerEasing.EaseOut),
            "ease-in-out" or "easeinout" => Set(out easing, ScriptPointerEasing.EaseInOut),
            "spring" => Set(out easing, ScriptPointerEasing.Spring),
            _ => false
        };
    }

    private static bool Set(out ScriptPointerEasing target, ScriptPointerEasing value)
    {
        target = value;
        return true;
    }
}
