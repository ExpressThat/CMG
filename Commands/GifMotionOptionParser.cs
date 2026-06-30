using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

internal static class GifMotionOptionParser
{
    public static bool TryParse(
        int? pointerDuration,
        string? pointerSpeed,
        string? pointerEasing,
        out ScriptPointerMotionOptions motion,
        out string? error)
    {
        motion = ScriptPointerMotionOptions.Default;
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
            return true;
        }
        catch (ScriptExecutionException exception)
        {
            error = exception.Message.Replace("--pointer-speed option pointerSpeed=", "--pointer-speed", StringComparison.Ordinal);
            return false;
        }
    }
}
