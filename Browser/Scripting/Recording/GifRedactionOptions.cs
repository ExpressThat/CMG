namespace CMG.Browser.Scripting.Recording;

public enum GifAutoRedactionMode
{
    None,
    Passwords,
    Sensitive,
    Emails,
    Payment,
    Privacy
}

public enum GifRedactionStyle
{
    Solid,
    Blur,
    Replacement
}

public sealed record GifRedactionRule(
    string Id,
    string Locator,
    GifRedactionStyle Style,
    string Color,
    string Replacement,
    int Padding);

public sealed record GifRedactionOptions(
    IReadOnlyList<GifRedactionRule>? Rules = null,
    GifAutoRedactionMode Auto = GifAutoRedactionMode.Passwords,
    bool Strict = false)
{
    public IReadOnlyList<GifRedactionRule> EffectiveRules => Rules ?? [];

    public static GifRedactionOptions FromOptions(
        IReadOnlyDictionary<string, string> options,
        string source)
    {
        var style = ParseStyle(options.GetValueOrDefault("redactStyle"), source);
        var color = options.GetValueOrDefault("redactColor") ?? "#111827";
        var replacement = options.GetValueOrDefault("redactReplacement") ?? "[redacted]";
        var padding = ParsePadding(options.GetValueOrDefault("redactPadding"), source);
        var locators = (options.GetValueOrDefault("redact") ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var rules = locators
            .Select((locator, index) => new GifRedactionRule($"initial-{index + 1}", locator, style, color, replacement, padding))
            .ToArray();
        return new GifRedactionOptions(
            rules,
            ParseAuto(options.GetValueOrDefault("autoRedact"), source),
            ParseStrict(options.GetValueOrDefault("redactionSafety"), source));
    }

    public static GifRedactionStyle ParseStyle(string? value, string source) =>
        value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "solid" => GifRedactionStyle.Solid,
            "blur" => GifRedactionStyle.Blur,
            "replacement" or "replace" or "text" => GifRedactionStyle.Replacement,
            _ => throw new ScriptExecutionException($"{source} redactStyle= must be solid, blur, or replacement.")
        };

    private static GifAutoRedactionMode ParseAuto(string? value, string source) =>
        value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "passwords" or "password" or "true" or "on" => GifAutoRedactionMode.Passwords,
            "sensitive" or "tokens" => GifAutoRedactionMode.Sensitive,
            "emails" or "email" => GifAutoRedactionMode.Emails,
            "payment" or "payments" or "cards" => GifAutoRedactionMode.Payment,
            "privacy" or "all" => GifAutoRedactionMode.Privacy,
            "none" or "false" or "off" => GifAutoRedactionMode.None,
            _ => throw new ScriptExecutionException($"{source} autoRedact= must be passwords, tokens, emails, payment, privacy, or none.")
        };

    private static bool ParseStrict(string? value, string source) =>
        value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "standard" or "false" or "off" => false,
            "strict" or "true" or "on" => true,
            _ => throw new ScriptExecutionException($"{source} redactionSafety= must be standard or strict.")
        };

    private static int ParsePadding(string? value, string source)
    {
        if (value is null) return 0;
        return int.TryParse(value, out var padding) && padding is >= 0 and <= 100
            ? padding
            : throw new ScriptExecutionException($"{source} redactPadding= must be between 0 and 100.");
    }
}

public sealed record GifRedactionAuditEntry(
    string Operation,
    string Locator,
    string Style,
    int FrameIndex,
    int TimeMilliseconds);
