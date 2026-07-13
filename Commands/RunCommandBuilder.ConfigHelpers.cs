using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class RunCommandBuilder
{
    private static bool WasProvided(ParseResult parseResult, Option option)
    {
        var names = option.Aliases.Prepend(option.Name).ToArray();
        return parseResult.Tokens.Any(token => names.Contains(token.Value, StringComparer.Ordinal));
    }

    private static DirectoryInfo? DirectoryValue(ParseResult parseResult, Option<DirectoryInfo?> option, DirectoryInfo? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback;

    private static FileInfo? FileValue(ParseResult parseResult, Option<FileInfo?> option, FileInfo? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback;

    private static string? StringValue(ParseResult parseResult, Option<string?> option, string? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback;

    private static int IntValue(ParseResult parseResult, Option<int> option, int? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback ?? parseResult.GetValue(option);

    private static int? IntValue(ParseResult parseResult, Option<int?> option, int? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback;

    private static bool BoolValue(ParseResult parseResult, Option<bool> option, bool? fallback) =>
        WasProvided(parseResult, option) ? parseResult.GetValue(option) : fallback ?? parseResult.GetValue(option);

    private static bool TryParseBrowserIdle(
        ParseResult parseResult,
        Option<string?> timeoutOption,
        Option<bool> disabledOption,
        RunConfig config,
        out int? timeout,
        out bool disabled,
        out string? error)
    {
        var value = StringValue(parseResult, timeoutOption, config.BrowserIdleTimeout);
        disabled = BoolValue(parseResult, disabledOption, config.NoBrowserIdleCleanup);
        if (WasProvided(parseResult, timeoutOption) && !WasProvided(parseResult, disabledOption)) disabled = false;
        if (WasProvided(parseResult, disabledOption) && !WasProvided(parseResult, timeoutOption)) value = null;
        return BrowserIdleTimeoutParser.TryParse(value, out timeout, out error);
    }

    private static IReadOnlyDictionary<string, string> MergeVariables(
        IReadOnlyDictionary<string, string>? configVariables,
        IReadOnlyDictionary<string, string>? cliVariables)
    {
        var merged = new Dictionary<string, string>(configVariables ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in cliVariables ?? new Dictionary<string, string>())
        {
            merged[key] = value;
        }

        return merged;
    }

    private static bool TrySelectProject(string? name, RunConfig config, out RunProjectConfig? project, out string? error)
    {
        project = null;
        error = null;
        if (string.IsNullOrWhiteSpace(name))
        {
            return true;
        }
        if (!config.Projects.TryGetValue(name, out project))
        {
            error = $"Run config project '{name}' was not found.";
            return false;
        }

        return true;
    }

    private static BrowserKind? BrowserKindFor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.ToLowerInvariant() switch
        {
            "chrome" => BrowserKind.Chrome,
            "edge" => BrowserKind.Edge,
            "firefox" => BrowserKind.Firefox,
            _ => BrowserKind.InvalidSelection
        };
    }
}
