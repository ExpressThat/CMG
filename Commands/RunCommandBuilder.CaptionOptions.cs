using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class RunCommandBuilder
{
    private static CaptionCliOptions BuildCaptionOptions() =>
        new(
            new Option<string?>("--caption-style") { Description = $"Default caption style for --gif recordings: {BrowserCaptionOptions.StyleValues}." },
            new Option<string?>("--caption-position") { Description = $"Default caption position for --gif recordings: {BrowserCaptionOptions.PositionValues}." },
            new Option<string?>("--caption-severity") { Description = $"Default caption severity color for --gif recordings: {BrowserCaptionOptions.SeverityValues}." },
            new Option<string?>("--caption-size") { Description = $"Default caption text size for --gif recordings: {BrowserCaptionOptions.SizeValues}." },
            new Option<bool>("--auto-captions") { Description = "Automatically caption supported visual actions in --gif recordings." },
            new Option<string?>("--caption-template") { Description = "Automatic-caption template for --gif recordings." });

    private static bool TryParseCaption(
        ParseResult parseResult,
        CaptionCliOptions options,
        out BrowserCaptionOptions? caption,
        out string error) =>
        GifCaptionOptionParser.TryParse(
            parseResult.GetValue(options.Style),
            parseResult.GetValue(options.Position),
            parseResult.GetValue(options.Severity),
            out caption,
            out error,
            parseResult.GetValue(options.Size),
            WasProvided(parseResult, options.AutoCaptions) ? parseResult.GetValue(options.AutoCaptions) : null,
            parseResult.GetValue(options.Template));

    private sealed record CaptionCliOptions(
        Option<string?> Style,
        Option<string?> Position,
        Option<string?> Severity,
        Option<string?> Size,
        Option<bool> AutoCaptions,
        Option<string?> Template);
}
