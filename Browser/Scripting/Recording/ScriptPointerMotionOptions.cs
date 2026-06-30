using System.Globalization;
using CMG.Browser.Scripting;

namespace CMG.Browser.Scripting.Recording;

public sealed record ScriptPointerMotionOptions(
    int? PointerDurationMilliseconds = null,
    string? PointerSpeed = null,
    ScriptPointerEasing PointerEasing = ScriptPointerEasing.EaseInOut)
{
    public static readonly ScriptPointerMotionOptions Default = new();

    public const int DefaultDurationMilliseconds = 800;

    public ScriptPointerMotionOptions WithAction(BrowserScriptAction action, string? durationAlias = null, string? easingAlias = null)
    {
        var duration = PointerDurationMilliseconds;
        if (action.Options.TryGetValue("pointerDuration", out var rawDuration) ||
            durationAlias is not null && action.Options.TryGetValue(durationAlias, out rawDuration))
        {
            duration = ParseDuration(rawDuration, $"{action.Name} option pointerDuration=");
        }

        var speed = action.Options.TryGetValue("pointerSpeed", out var rawSpeed) ? rawSpeed : PointerSpeed;
        var easing = PointerEasing;
        if (action.Options.TryGetValue("pointerEasing", out var rawEasing) ||
            easingAlias is not null && action.Options.TryGetValue(easingAlias, out rawEasing))
        {
            easing = ParseEasing(rawEasing, $"{action.Name} option pointerEasing=");
        }

        return new ScriptPointerMotionOptions(duration, speed, easing).Validate(action.Name);
    }

    public ScriptPointerMotionOptions WithDurationOption(BrowserScriptAction action, string optionName)
    {
        return action.Options.TryGetValue(optionName, out var value)
            ? this with { PointerDurationMilliseconds = ParseDuration(value, $"{action.Name} option {optionName}=") }
            : this;
    }

    public ScriptPointerMotionOptions WithEasingOption(BrowserScriptAction action, string optionName)
    {
        return action.Options.TryGetValue(optionName, out var value)
            ? this with { PointerEasing = ParseEasing(value, $"{action.Name} option {optionName}=") }
            : this;
    }

    public ScriptPointerMotionOptions Validate(string source)
    {
        if (PointerDurationMilliseconds is < 0)
        {
            throw new ScriptExecutionException($"{source} option pointerDuration= must be zero or greater.");
        }

        if (PointerSpeed is not null)
        {
            _ = DurationMilliseconds(source);
        }

        return this;
    }

    public int FrameCount(string source, int frameDelayMilliseconds = ScriptRecordingOptions.DefaultFrameDelayMilliseconds)
    {
        var duration = DurationMilliseconds(source);
        return Math.Max(1, (int)Math.Ceiling(duration / (double)Math.Max(1, frameDelayMilliseconds)));
    }

    public int DurationMilliseconds(string source)
    {
        var baseDuration = PointerDurationMilliseconds ?? DefaultDurationMilliseconds;
        if (string.IsNullOrWhiteSpace(PointerSpeed))
        {
            return baseDuration;
        }

        return PointerSpeed.Trim().ToLowerInvariant() switch
        {
            "slow" => 1200,
            "normal" => DefaultDurationMilliseconds,
            "fast" => 350,
            "instant" => 0,
            var value when TryParseMultiplier(value, out var multiplier) => Math.Max(0, (int)Math.Round(baseDuration / multiplier)),
            _ => throw new ScriptExecutionException($"{source} option pointerSpeed= must be slow, normal, fast, instant, or a positive multiplier like 1.5x.")
        };
    }

    public static int ParseDuration(string value, string label) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0
            ? parsed
            : throw new ScriptExecutionException($"{label} must be zero or greater.");

    public static ScriptPointerEasing ParseEasing(string value, string label) =>
        ScriptPointerEasingParser.TryParse(value, out var easing)
            ? easing
            : throw new ScriptExecutionException($"{label} must be one of: {ScriptPointerEasingParser.Values}.");

    private static bool TryParseMultiplier(string value, out double multiplier)
    {
        multiplier = 0;
        if (!value.EndsWith("x", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return double.TryParse(value[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out multiplier) &&
            multiplier > 0;
    }
}
