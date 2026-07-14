using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

internal static class GifCliFormatParser
{
    public static bool TryParse(
        ParseResult result,
        GifEncodingCliOptions options,
        RunGifSettings? settings,
        out GifArtifactFormat format,
        out string? ffmpeg,
        out string? error)
    {
        format = GifArtifactFormat.Gif;
        ffmpeg = null;
        error = null;
        try
        {
            format = GifArtifactFormatParser.Parse(
                Provided(result, options.Format) ? result.GetValue(options.Format) : settings?.Format,
                "recording");
            ffmpeg = Provided(result, options.Ffmpeg)
                ? result.GetValue(options.Ffmpeg)?.FullName
                : settings?.FfmpegPath;
            return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static bool Provided(ParseResult result, Option option) =>
        result.Tokens.Any(token => option.Aliases.Prepend(option.Name)
            .Contains(token.Value, StringComparer.Ordinal));
}
