namespace CMG.Commands;

internal static class GifTimingOptionParser
{
    public static bool TryParseHoldAfterAction(int? value, out int milliseconds, out string? error)
    {
        milliseconds = CMG.Browser.Scripting.Recording.ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds;
        error = null;
        if (value is null)
        {
            return true;
        }

        if (value < 0)
        {
            error = "--gif-hold-after-action must be zero or greater.";
            return false;
        }

        milliseconds = value.Value;
        return true;
    }
}
