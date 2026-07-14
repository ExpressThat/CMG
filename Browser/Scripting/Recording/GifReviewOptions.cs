namespace CMG.Browser.Scripting.Recording;

public sealed record GifReviewOptions(
    string? NarrationSidecar = null,
    string? AltText = null,
    string? Description = null)
{
    public static GifReviewOptions FromOptions(
        IReadOnlyDictionary<string, string> options,
        string context,
        GifReviewOptions? defaults = null)
    {
        defaults ??= new();
        var result = new GifReviewOptions(
            options.TryGetValue("narrationSidecar", out var narration)
                ? ParseNarration(narration, context)
                : defaults.NarrationSidecar,
            options.GetValueOrDefault("altText") ?? defaults.AltText,
            options.GetValueOrDefault("description") ?? defaults.Description);
        ValidateAltText(result.AltText, context);
        return result;
    }

    public string? ResolveNarrationPath(string gifPath) => NarrationSidecar switch
    {
        null => null,
        "auto" => GifArtifactPaths.Narration(gifPath),
        _ when Path.IsPathRooted(NarrationSidecar) => NarrationSidecar,
        _ => Path.GetFullPath(NarrationSidecar)
    };

    public string? RenderAltText(
        string gifPath,
        GifFrameSink sink,
        IReadOnlyList<GifTimelineStep> steps,
        GifRecordingOutcome? outcome = null)
    {
        if (string.IsNullOrWhiteSpace(AltText)) return null;
        var result = outcome?.ToString().ToLowerInvariant() ?? (steps.Any(step => !step.Success) ? "failed" : "passed");
        return AltText
            .Replace("{name}", Path.GetFileNameWithoutExtension(gifPath), StringComparison.Ordinal)
            .Replace("{steps}", steps.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{duration}", sink.DurationMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{outcome}", result, StringComparison.Ordinal);
    }

    private static string? ParseNarration(string value, string context)
    {
        if (bool.TryParse(value, out var enabled)) return enabled ? "auto" : null;
        if (!string.IsNullOrWhiteSpace(value)) return value;
        throw new ScriptExecutionException($"{context} option narrationSidecar= must be true, false, or a file path.");
    }

    private static void ValidateAltText(string? template, string context)
    {
        if (template is null) return;
        var allowed = new HashSet<string>(["{name}", "{steps}", "{duration}", "{outcome}"], StringComparer.Ordinal);
        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(template, @"\{[^{}]+\}"))
            if (!allowed.Contains(match.Value))
                throw new ScriptExecutionException($"{context} option altText= has unknown placeholder {match.Value}. Supported placeholders: {{name}}, {{steps}}, {{duration}}, {{outcome}}.");
    }
}
