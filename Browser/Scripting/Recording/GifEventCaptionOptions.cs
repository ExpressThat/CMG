namespace CMG.Browser.Scripting.Recording;

public sealed record GifEventCaptionOptions(
    bool Network = false,
    bool Dialogs = false,
    bool Console = false,
    bool Downloads = false,
    bool Uploads = false)
{
    public static GifEventCaptionOptions FromOptions(
        IReadOnlyDictionary<string, string> options,
        string source)
    {
        var preset = Parse(options.GetValueOrDefault("eventCaptions"), false, "eventCaptions", source);
        return new(
            Parse(options.GetValueOrDefault("networkCaptions"), preset, "networkCaptions", source),
            Parse(options.GetValueOrDefault("dialogCaptions"), preset, "dialogCaptions", source),
            Parse(options.GetValueOrDefault("consoleCaptions"), preset, "consoleCaptions", source),
            Parse(options.GetValueOrDefault("downloadCaptions"), preset, "downloadCaptions", source),
            Parse(options.GetValueOrDefault("uploadCaptions"), preset, "uploadCaptions", source));
    }

    public GifEventCaptionOptions WithOptions(IReadOnlyDictionary<string, string> options, string source)
    {
        var parsed = FromOptions(options, source);
        if (options.ContainsKey("eventCaptions")) return parsed;
        return parsed with
        {
            Network = options.ContainsKey("networkCaptions") ? parsed.Network : Network,
            Dialogs = options.ContainsKey("dialogCaptions") ? parsed.Dialogs : Dialogs,
            Console = options.ContainsKey("consoleCaptions") ? parsed.Console : Console,
            Downloads = options.ContainsKey("downloadCaptions") ? parsed.Downloads : Downloads,
            Uploads = options.ContainsKey("uploadCaptions") ? parsed.Uploads : Uploads
        };
    }

    private static bool Parse(string? value, bool fallback, string option, string source) =>
        value?.Trim().ToLowerInvariant() switch
        {
            null => fallback,
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{source} {option}= must be true or false.")
        };
}
