using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

internal static class GifMotionOptionParser
{
    public static bool TryParse(
        int? pointerDuration,
        string? pointerSpeed,
        string? pointerEasing,
        string? clickPulse,
        out ScriptPointerMotionOptions motion,
        out ClickPulseStyle pulse,
        out string? error)
    {
        motion = ScriptPointerMotionOptions.Default;
        pulse = ClickPulseStyle.Ring;
        error = null;
        try
        {
            if (pointerDuration is < 0)
            {
                error = "--pointer-duration must be zero or greater.";
                return false;
            }

            var easing = ScriptPointerEasing.EaseInOut;
            if (!string.IsNullOrWhiteSpace(pointerEasing) &&
                !ScriptPointerEasingParser.TryParse(pointerEasing, out easing))
            {
                error = $"--pointer-easing must be one of: {ScriptPointerEasingParser.Values}.";
                return false;
            }

            motion = new ScriptPointerMotionOptions(pointerDuration, pointerSpeed, easing).Validate("--pointer-speed");
            if (!string.IsNullOrWhiteSpace(clickPulse) &&
                !ClickPulseStyleParser.TryParse(clickPulse, out pulse))
            {
                error = $"--click-pulse must be one of: {ClickPulseStyleParser.Values}.";
                return false;
            }

            return true;
        }
        catch (ScriptExecutionException exception)
        {
            error = exception.Message.Replace("--pointer-speed option pointerSpeed=", "--pointer-speed", StringComparison.Ordinal);
            return false;
        }
    }
}
