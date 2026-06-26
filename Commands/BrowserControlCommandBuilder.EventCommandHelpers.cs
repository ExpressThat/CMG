using System.CommandLine;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private static void AddDownloadOptions(Command command, out Option<DirectoryInfo?> directory, out Option<string?> pattern, out Option<int?> timeout)
    {
        directory = new Option<DirectoryInfo?>("--directory") { Description = "Directory to watch. Default is the current directory." };
        pattern = CliStringOption("--pattern", "File glob to match. Default is *.");
        timeout = CliIntOption("--timeout", "Timeout in milliseconds.");
        command.Options.Add(directory);
        command.Options.Add(pattern);
        command.Options.Add(timeout);
    }

    private static IReadOnlyList<string> EventArguments(ParseResult parseResult, Argument<string> eventName, Argument<string?> matcher)
    {
        var values = new List<string> { parseResult.GetValue(eventName) ?? string.Empty };
        var match = parseResult.GetValue(matcher);
        if (!string.IsNullOrWhiteSpace(match))
        {
            values.Add(match);
        }

        return values;
    }

    private static IReadOnlyList<(string Key, string Value)> DownloadOptions(
        ParseResult parseResult,
        Option<DirectoryInfo?> directory,
        Option<string?> pattern,
        Option<int?> timeout) =>
        CompactOptions([
            StringOption("directory", parseResult.GetValue(directory)?.FullName),
            StringOption("pattern", parseResult.GetValue(pattern)),
            IntOption("timeout", parseResult.GetValue(timeout))
        ]);

    private static IReadOnlyList<(string Key, string Value)> DialogOptions(ParseResult parseResult, Option<string?> promptText) =>
        CompactOptions([StringOption("promptText", parseResult.GetValue(promptText))]);

    private static IReadOnlyList<(string Key, string Value)> EventWaitOptions(
        ParseResult parseResult,
        Option<int?> timeout,
        Option<string?>? level,
        Option<string?> match,
        Option<bool> ignoreCase) =>
        CompactOptions([
            IntOption("timeout", parseResult.GetValue(timeout)),
            StringOption("level", level is null ? null : parseResult.GetValue(level)),
            StringOption("match", parseResult.GetValue(match)),
            parseResult.GetValue(ignoreCase) ? ("ignoreCase", "true") : null
        ]);

    private static IReadOnlyList<(string Key, string Value)> EventOptions(ParseResult parseResult, params Option[] options) =>
        CompactOptions(options.Select<Option, (string Key, string Value)?>(option =>
        {
            var value = EventOptionValue(parseResult, option);
            return string.IsNullOrWhiteSpace(value) ? null : (option.Name.TrimStart('-'), value);
        }).ToArray());

    private static string? EventOptionValue(ParseResult parseResult, Option option) =>
        option switch
        {
            Option<DirectoryInfo?> directory => parseResult.GetValue(directory)?.FullName,
            Option<int?> integer => parseResult.GetValue(integer)?.ToString(),
            Option<bool> boolean => parseResult.GetValue(boolean) ? "true" : null,
            Option<string?> text => parseResult.GetValue(text),
            _ => null
        };

    private static Option<string?> CliStringOption(string name, string description) =>
        new(name) { Description = description };

    private static Option<int?> CliIntOption(string name, string description) =>
        new(name) { Description = description };
}
