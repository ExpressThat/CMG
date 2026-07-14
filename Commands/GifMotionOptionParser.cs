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
        string? pointerPath,
        string? dragPath,
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

            var path = ParsePath(pointerPath, "--pointer-path");
            var drag = ParseOptionalPath(dragPath, "--drag-path");
            motion = new ScriptPointerMotionOptions(pointerDuration, pointerSpeed, easing, path, drag).Validate("--pointer-speed");
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

    private static ScriptPointerPath ParsePath(string? value, string option) =>
        string.IsNullOrWhiteSpace(value) ? ScriptPointerPath.Auto :
        ScriptPointerPathParser.TryParse(value, out var path) ? path :
        throw new ScriptExecutionException($"{option} must be one of: {ScriptPointerPathParser.Values}.");

    private static ScriptPointerPath? ParseOptionalPath(string? value, string option) =>
        string.IsNullOrWhiteSpace(value) ? null : ParsePath(value, option);
}
