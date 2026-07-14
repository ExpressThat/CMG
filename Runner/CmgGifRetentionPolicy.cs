namespace CMG.Runner;

public enum CmgGifRetentionMode { Always, OnFailure, OnRetry, Off }

internal sealed record CmgGifRetentionPolicy(
    CmgGifRetentionMode Mode,
    int SampleRate,
    bool CleanPassed,
    int? CleanupDays = null)
{
    public bool ShouldRecord(int ordinal) =>
        Mode is not CmgGifRetentionMode.Off && (ordinal - 1) % SampleRate == 0;

    public static bool TryParse(CmgTestCase test, out CmgGifRetentionPolicy policy, out string? error)
        => TryParse(test, new CmgGifRetentionPolicy(CmgGifRetentionMode.Always, 1, false), out policy, out error);

    public static bool TryParse(
        CmgTestCase test,
        CmgRunOptions options,
        out CmgGifRetentionPolicy policy,
        out string? error) => TryParse(test, new CmgGifRetentionPolicy(
            options.GifRetentionMode,
            options.GifRetentionSampleRate,
            options.GifCleanPassed), out policy, out error);

    private static bool TryParse(
        CmgTestCase test,
        CmgGifRetentionPolicy defaults,
        out CmgGifRetentionPolicy policy,
        out string? error)
    {
        policy = defaults;
        error = null;
        var mode = defaults.Mode;
        if (test.Options.TryGetValue("gif", out var rawMode) && !TryParseMode(rawMode, out mode))
        {
            error = "test option gif= must be one of: always, onFailure, onRetry, off.";
            return false;
        }

        var sampleRate = defaults.SampleRate;
        if (test.Options.TryGetValue("gifSampleRate", out var rawRate) &&
            (!int.TryParse(rawRate, out sampleRate) || sampleRate < 1))
        {
            error = "test option gifSampleRate= must be an integer of at least 1.";
            return false;
        }

        var clean = defaults.CleanPassed;
        if (test.Options.TryGetValue("gifCleanPassed", out var rawClean) && !TryBoolean(rawClean, out clean))
        {
            error = "test option gifCleanPassed= must be true or false.";
            return false;
        }

        policy = new(mode, sampleRate, clean);
        return true;
    }

    internal static bool TryParseMode(string value, out CmgGifRetentionMode mode)
    {
        mode = value.Trim().ToLowerInvariant() switch
        {
            "always" or "all" or "true" => CmgGifRetentionMode.Always,
            "onfailure" or "failed" => CmgGifRetentionMode.OnFailure,
            "onretry" or "retry" => CmgGifRetentionMode.OnRetry,
            "off" or "none" or "false" => CmgGifRetentionMode.Off,
            _ => (CmgGifRetentionMode)(-1)
        };
        return Enum.IsDefined(mode);
    }

    private static bool TryBoolean(string value, out bool result)
    {
        result = value.Trim().ToLowerInvariant() is "true" or "yes" or "on" or "1";
        return result || value.Trim().ToLowerInvariant() is "false" or "no" or "off" or "0";
    }
}
