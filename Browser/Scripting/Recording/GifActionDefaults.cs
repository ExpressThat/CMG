namespace CMG.Browser.Scripting.Recording;

public sealed record GifActionDefaults(
    int? TypingDelayMilliseconds = null,
    int? PostHoverHoldMilliseconds = null)
{
    public static GifActionDefaults FromOptions(
        IReadOnlyDictionary<string, string> options,
        string source,
        GifActionDefaults? defaults = null)
    {
        defaults ??= new();
        return new(
            Duration(options, "typingDelay", source, defaults.TypingDelayMilliseconds),
            Duration(options, "postHoverHold", source, defaults.PostHoverHoldMilliseconds));
    }

    public static GifActionDefaults FromValues(int? typingDelay, int? postHoverHold, string source)
    {
        Validate(typingDelay, "typingDelay", source);
        Validate(postHoverHold, "postHoverHold", source);
        return new(typingDelay, postHoverHold);
    }

    public IReadOnlyDictionary<string, string> ToOptions()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (TypingDelayMilliseconds is int typing) values["typingDelay"] = typing.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (PostHoverHoldMilliseconds is int hover) values["postHoverHold"] = hover.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return values;
    }

    private static int? Duration(
        IReadOnlyDictionary<string, string> options,
        string name,
        string source,
        int? fallback) =>
        options.TryGetValue(name, out var value)
            ? ScriptPointerMotionOptions.ParseDuration(value, $"{source} option {name}=")
            : fallback;

    private static void Validate(int? value, string name, string source)
    {
        if (value < 0) throw new ScriptExecutionException($"{source} option {name}= must be zero or greater.");
    }
}
