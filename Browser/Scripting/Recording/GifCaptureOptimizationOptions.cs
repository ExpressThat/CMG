namespace CMG.Browser.Scripting.Recording;

public sealed record GifCaptureOptimizationOptions(
    bool CoalesceDuplicates = true,
    int SampleEvery = 1)
{
    public GifCaptureOptimizationOptions WithOptions(IReadOnlyDictionary<string, string> values, string source) =>
        new(
            Boolean(values.GetValueOrDefault("coalesceDuplicates"), CoalesceDuplicates, "coalesceDuplicates", source),
            PositiveInteger(values.GetValueOrDefault("sampleEvery"), SampleEvery, "sampleEvery", source));

    public static bool TryParse(bool disableCoalescing, int? sampleEvery, out GifCaptureOptimizationOptions options, out string? error)
    {
        options = new();
        error = null;
        try
        {
            options = new(!disableCoalescing, PositiveInteger(sampleEvery?.ToString(), 1, "sampleEvery", "GIF"));
            return true;
        }
        catch (ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static bool Boolean(string? value, bool fallback, string option, string source) =>
        value?.Trim().ToLowerInvariant() switch
        {
            null => fallback,
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{source} option {option}= must be true or false.")
        };

    private static int PositiveInteger(string? value, int fallback, string option, string source)
    {
        if (value is null) return fallback;
        return int.TryParse(value, out var parsed) && parsed is >= 1 and <= 100
            ? parsed
            : throw new ScriptExecutionException($"{source} option {option}= must be an integer from 1 to 100.");
    }
}
