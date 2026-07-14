using System.Globalization;
using System.Text.RegularExpressions;

namespace CMG.Browser.Scripting.Recording;

public sealed record GifSizeBudgetOptions(
    long? Bytes = null,
    bool QualityFallback = true,
    bool DownscaleFallback = true)
{
    public static GifSizeBudgetOptions FromOptions(
        IReadOnlyDictionary<string, string> options,
        string context,
        GifSizeBudgetOptions? defaults = null)
    {
        defaults ??= new();
        return new(
            options.TryGetValue("sizeBudget", out var size) ? ParseSize(size, context) : defaults.Bytes,
            Boolean(options, "budgetQualityFallback", defaults.QualityFallback, context),
            Boolean(options, "budgetDownscaleFallback", defaults.DownscaleFallback, context));
    }

    public static long ParseSize(string value, string context)
    {
        var match = Regex.Match(value.Trim(), @"^(?<number>\d+(?:\.\d+)?)\s*(?<unit>B|KB|MB|GB)?$", RegexOptions.IgnoreCase);
        if (!match.Success || !decimal.TryParse(match.Groups["number"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            throw new ScriptExecutionException($"{context} option sizeBudget= must be a positive byte size such as 500KB or 2MB.");
        var multiplier = match.Groups["unit"].Value.ToUpperInvariant() switch
        {
            "KB" => 1024m, "MB" => 1024m * 1024m, "GB" => 1024m * 1024m * 1024m, _ => 1m
        };
        var bytes = number * multiplier;
        return bytes is >= 1 and <= long.MaxValue
            ? decimal.ToInt64(decimal.Ceiling(bytes))
            : throw new ScriptExecutionException($"{context} option sizeBudget= must be greater than zero.");
    }

    private static bool Boolean(IReadOnlyDictionary<string, string> options, string name, bool fallback, string context)
    {
        if (!options.TryGetValue(name, out var value)) return fallback;
        return bool.TryParse(value, out var parsed)
            ? parsed
            : throw new ScriptExecutionException($"{context} option {name}= must be true or false.");
    }
}
