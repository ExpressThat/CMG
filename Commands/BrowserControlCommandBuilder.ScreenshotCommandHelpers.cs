using System.CommandLine;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private static IReadOnlyList<(string Key, string Value)> ScreenshotCliOptions(
        ParseResult parseResult,
        Option<FileInfo?> outputOption,
        Option<string?> typeOption,
        Option<int?> qualityOption,
        Option<bool> omitBackgroundOption)
    {
        var options = ToOutputOptions(parseResult.GetValue(outputOption)).ToList();
        if (!string.IsNullOrWhiteSpace(parseResult.GetValue(typeOption)))
        {
            options.Add(("type", parseResult.GetValue(typeOption)!));
        }
        if (parseResult.GetValue(qualityOption) is { } quality)
        {
            options.Add(("quality", quality.ToString()));
        }
        if (parseResult.GetValue(omitBackgroundOption))
        {
            options.Add(("omitBackground", "true"));
        }

        return options;
    }
}
