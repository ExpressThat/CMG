using System.CommandLine;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private static IReadOnlyList<(string Key, string Value)> ScreenshotCliOptions(
        ParseResult parseResult,
        Option<FileInfo?> outputOption,
        Option<string?> typeOption,
        Option<int?> qualityOption,
        Option<bool> omitBackgroundOption,
        Option<string?>? styleOption = null,
        Option<FileInfo?>? stylePathOption = null,
        Option<double?>? clipXOption = null,
        Option<double?>? clipYOption = null,
        Option<double?>? clipWidthOption = null,
        Option<double?>? clipHeightOption = null)
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
        if (styleOption is not null && !string.IsNullOrWhiteSpace(parseResult.GetValue(styleOption)))
        {
            options.Add(("style", parseResult.GetValue(styleOption)!));
        }
        if (stylePathOption is not null && parseResult.GetValue(stylePathOption) is { } stylePath)
        {
            options.Add(("stylePath", stylePath.FullName));
        }
        AddClipOption(parseResult, options, clipXOption, "clipX");
        AddClipOption(parseResult, options, clipYOption, "clipY");
        AddClipOption(parseResult, options, clipWidthOption, "clipWidth");
        AddClipOption(parseResult, options, clipHeightOption, "clipHeight");

        return options;
    }

    private static void AddClipOption(
        ParseResult parseResult,
        ICollection<(string Key, string Value)> options,
        Option<double?>? option,
        string key)
    {
        if (option is not null && parseResult.GetValue(option) is { } value)
        {
            options.Add((key, value.ToString()));
        }
    }
}
