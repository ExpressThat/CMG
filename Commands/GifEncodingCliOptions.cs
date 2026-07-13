using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

internal sealed record GifEncodingCliOptions(
    Option<string?> Dither,
    Option<string?> Palette,
    Option<int?> Colors,
    Option<DirectoryInfo?> KeepFrames,
    Option<string?> Crop,
    Option<int?> CropPadding,
    Option<double?> Scale,
    Option<int?> MaxWidth,
    Option<int?> MaxHeight,
    Option<bool> Debug,
    Option<bool> Accessibility)
{
    public static GifEncodingCliOptions Build()
    {
        return new(
            new Option<string?>("--gif-dither") { Description = $"GIF dithering: {GifEncodingOptions.DitherValues}." },
            new Option<string?>("--gif-palette") { Description = $"GIF color table: {GifEncodingOptions.PaletteValues}." },
            new Option<int?>("--gif-colors") { Description = "Maximum GIF palette colors, from 2 to 256." },
            new Option<DirectoryInfo?>("--keep-frames") { Description = "Keep exact pre-encoding PNG frames in this directory." },
            new Option<string?>("--gif-crop") { Description = "Crop GIF frames to this selector or rich locator." },
            new Option<int?>("--gif-crop-padding") { Description = "Padding around --gif-crop in CSS pixels, from 0 to 2000." },
            new Option<double?>("--gif-scale") { Description = "Output scale from 0.05 to 1." },
            new Option<int?>("--gif-max-width") { Description = "Maximum GIF width from 1 to 10000 pixels." },
            new Option<int?>("--gif-max-height") { Description = "Maximum GIF height from 1 to 10000 pixels." },
            new Option<bool>("--gif-debug") { Description = "Show action, scope, target, pointer, and scroll diagnostics in GIF frames." },
            new Option<bool>("--gif-accessibility") { Description = "Show keyboard, focus, accessible-name, and contrast evidence in GIF frames." });
    }

    public bool TryParse(ParseResult result, out GifEncodingOptions encoding, out string? error)
    {
        if (!GifEncodingOptions.TryParse(
            result.GetValue(Dither),
            result.GetValue(Palette),
            result.GetValue(Colors),
            result.GetValue(KeepFrames)?.FullName,
            out encoding,
            out error)) return false;
        if (!GifFramingOptions.TryParse(
            result.GetValue(Crop), result.GetValue(CropPadding), result.GetValue(Scale),
            result.GetValue(MaxWidth), result.GetValue(MaxHeight), out var framing, out error)) return false;
        encoding = encoding with
        {
            Framing = framing,
            Diagnostics = result.GetValue(Debug) ? new GifDebugOptions(true, true, true, true, true, true) : null,
            Accessibility = result.GetValue(Accessibility) ? new GifAccessibilityOptions(true, true, true, true, true) : null
        };
        return true;
    }
}
