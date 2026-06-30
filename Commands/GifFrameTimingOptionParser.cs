using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public static class GifFrameTimingOptionParser
{
    public static bool TryParse(
        int? fps,
        int? frameDelayMilliseconds,
        out int frameDelay,
        out string? error)
    {
        frameDelay = ScriptRecordingOptions.DefaultFrameDelayMilliseconds;
        error = null;

        if (fps is not null)
        {
            if (fps is <= 0 or > 100)
            {
                error = "--gif-fps must be between 1 and 100.";
                return false;
            }

            frameDelay = Math.Max(10, (int)Math.Round(1000d / fps.Value));
        }

        if (frameDelayMilliseconds is null)
        {
            return true;
        }

        if (frameDelayMilliseconds is < 10 or > 10_000)
        {
            error = "--gif-frame-delay must be between 10 and 10000 milliseconds.";
            return false;
        }

        frameDelay = frameDelayMilliseconds.Value;
        return true;
    }
}
