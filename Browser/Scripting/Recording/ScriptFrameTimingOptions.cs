using System.Globalization;

namespace CMG.Browser.Scripting.Recording;

public static class ScriptFrameTimingOptions
{
    public static int FromOptions(
        IReadOnlyDictionary<string, string> options,
        string source,
        int fallback = ScriptRecordingOptions.DefaultFrameDelayMilliseconds)
    {
        var frameDelay = fallback;
        if (options.TryGetValue("fps", out var rawFps))
        {
            frameDelay = DelayFromFps(rawFps, source);
        }

        return options.TryGetValue("frameDelay", out var rawDelay)
            ? ParseFrameDelay(rawDelay, $"{source} option frameDelay=")
            : frameDelay;
    }

    public static int DelayFromFps(string value, string source)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fps) ||
            fps <= 0 || fps > 100)
        {
            throw new ScriptExecutionException($"{source} option fps= must be between 1 and 100.");
        }

        return Math.Max(10, (int)Math.Round(1000d / fps));
    }

    private static int ParseFrameDelay(string value, string label) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
        parsed is >= 10 and <= 10_000
            ? parsed
            : throw new ScriptExecutionException($"{label} must be between 10 and 10000 milliseconds.");
}
