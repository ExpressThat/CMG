namespace CMG.Browser.Scripting.Recording;

public sealed record GifTitleCardOptions(
    string? Intro = null,
    string? Outro = null,
    int IntroDuration = 1200,
    int OutroDuration = 1200,
    bool ResultOutro = false)
{
    public GifTitleCardOptions WithOptions(IReadOnlyDictionary<string, string> values, string source)
    {
        var intro = values.GetValueOrDefault("intro") ?? Intro;
        var outro = values.GetValueOrDefault("outro") ?? Outro;
        var introDuration = Duration(values.GetValueOrDefault("introDuration"), IntroDuration, "introDuration", source);
        var outroDuration = Duration(values.GetValueOrDefault("outroDuration"), OutroDuration, "outroDuration", source);
        var resultOutro = Boolean(values.GetValueOrDefault("resultOutro"), ResultOutro, "resultOutro", source);
        return new(intro, outro, introDuration, outroDuration, resultOutro);
    }

    public static bool TryParse(
        string? intro,
        string? outro,
        int? introDuration,
        int? outroDuration,
        bool resultOutro,
        out GifTitleCardOptions options,
        out string? error)
    {
        options = new();
        error = null;
        try
        {
            options = new(
                intro,
                outro,
                Duration(introDuration?.ToString(), 1200, "introDuration", "GIF"),
                Duration(outroDuration?.ToString(), 1200, "outroDuration", "GIF"),
                resultOutro);
            return true;
        }
        catch (ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static int Duration(string? value, int fallback, string option, string source)
    {
        if (value is null) return fallback;
        return int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var duration) && duration > 0
            ? duration
            : throw new ScriptExecutionException($"{source} option {option}= must be greater than zero.");
    }

    private static bool Boolean(string? value, bool fallback, string option, string source) =>
        value?.Trim().ToLowerInvariant() switch
        {
            null => fallback,
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{source} option {option}= must be true or false.")
        };
}

public enum GifRecordingOutcome { Passed, Failed, Skipped }
