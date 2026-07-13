using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

internal sealed record GifEncodingCliOptions(
    Option<string?> Dither,
    Option<string?> Palette,
    Option<int?> Colors,
    Option<DirectoryInfo?> KeepFrames)
{
    public static GifEncodingCliOptions Build()
    {
        return new(
            new Option<string?>("--gif-dither") { Description = $"GIF dithering: {GifEncodingOptions.DitherValues}." },
            new Option<string?>("--gif-palette") { Description = $"GIF color table: {GifEncodingOptions.PaletteValues}." },
            new Option<int?>("--gif-colors") { Description = "Maximum GIF palette colors, from 2 to 256." },
            new Option<DirectoryInfo?>("--keep-frames") { Description = "Keep exact pre-encoding PNG frames in this directory." });
    }

    public bool TryParse(ParseResult result, out GifEncodingOptions encoding, out string? error) =>
        GifEncodingOptions.TryParse(
            result.GetValue(Dither),
            result.GetValue(Palette),
            result.GetValue(Colors),
            result.GetValue(KeepFrames)?.FullName,
            out encoding,
            out error);
}
