using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class RunCommandBuilder
{
    private static PointerVisualCliOptions BuildPointerVisualOptions() =>
        new(
            new Option<string?>("--pointer-theme") { Description = $"Default virtual pointer theme for --gif recordings: {PointerVisualOptions.ThemeValues}." },
            new Option<string?>("--pointer-color") { Description = "Default virtual pointer CSS color for --gif recordings." },
            new Option<int?>("--pointer-size") { Description = "Default virtual pointer size in CSS pixels for --gif recordings. Valid range is 8 to 96." },
            new Option<string?>("--pointer-shadow") { Description = $"Default virtual pointer shadow for --gif recordings: {PointerVisualOptions.ShadowValues}." });

    private static bool TryParsePointerVisual(
        ParseResult parseResult,
        PointerVisualCliOptions options,
        out PointerVisualOptions visual,
        out string error) =>
        GifVisualOptionParser.TryParse(
            parseResult.GetValue(options.Theme),
            parseResult.GetValue(options.Color),
            parseResult.GetValue(options.Size),
            parseResult.GetValue(options.Shadow),
            out visual,
            out error);

    private sealed record PointerVisualCliOptions(
        Option<string?> Theme,
        Option<string?> Color,
        Option<int?> Size,
        Option<string?> Shadow);
}
