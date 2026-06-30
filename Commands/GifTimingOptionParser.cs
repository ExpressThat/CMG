namespace CMG.Commands;

internal static class GifTimingOptionParser
{
    public static bool TryParseHoldAfterAction(int? value, out int milliseconds, out string? error)
    {
        return TryParse(value, CMG.Browser.Scripting.Recording.ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds, "--gif-hold-after-action", out milliseconds, out error);
    }

    public static bool TryParseHoldOnFailure(int? value, out int milliseconds, out string? error)
    {
        return TryParse(value, CMG.Browser.Scripting.Recording.ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds, "--gif-hold-on-failure", out milliseconds, out error);
    }

    private static bool TryParse(int? value, int defaultValue, string optionName, out int milliseconds, out string? error)
    {
        milliseconds = defaultValue;
        error = null;
        if (value is null)
        {
            return true;
        }

        if (value < 0)
        {
            error = $"{optionName} must be zero or greater.";
            return false;
        }

        milliseconds = value.Value;
        return true;
    }
}
