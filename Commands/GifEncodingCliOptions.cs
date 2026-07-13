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
    Option<bool> Accessibility,
    Option<bool> EventCaptions,
    Option<string?> Intro,
    Option<string?> Outro,
    Option<int?> IntroDuration,
    Option<int?> OutroDuration,
    Option<bool> ResultOutro,
    Option<bool> DisableCoalescing,
    Option<int?> SampleEvery)
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
            new Option<bool>("--gif-accessibility") { Description = "Show keyboard, focus, accessible-name, and contrast evidence in GIF frames." },
            new Option<bool>("--gif-event-captions") { Description = "Show safe outcome captions for network, dialog, console, download, and upload events." },
            new Option<string?>("--gif-intro") { Description = "Opening title-card text for the whole GIF." },
            new Option<string?>("--gif-outro") { Description = "Final title-card text for the whole GIF." },
            new Option<int?>("--gif-intro-duration") { Description = "Opening title-card duration in milliseconds." },
            new Option<int?>("--gif-outro-duration") { Description = "Final title-card duration in milliseconds." },
            new Option<bool>("--gif-result-outro") { Description = "Generate a passed, failed, or skipped final title card." },
            new Option<bool>("--gif-no-coalesce") { Description = "Keep consecutive duplicate frames instead of merging their delays." },
            new Option<int?>("--gif-sample-every") { Description = "Keep every Nth intermediate pointer/drag frame, from 1 to 100." });
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
        if (!GifTitleCardOptions.TryParse(
            result.GetValue(Intro), result.GetValue(Outro), result.GetValue(IntroDuration), result.GetValue(OutroDuration),
            result.GetValue(ResultOutro), out var titleCards, out error)) return false;
        if (!GifCaptureOptimizationOptions.TryParse(
            result.GetValue(DisableCoalescing), result.GetValue(SampleEvery), out var captureOptimization, out error)) return false;
        encoding = encoding with
        {
            Framing = framing,
            Diagnostics = result.GetValue(Debug) ? new GifDebugOptions(true, true, true, true, true, true) : null,
            Accessibility = result.GetValue(Accessibility) ? new GifAccessibilityOptions(true, true, true, true, true) : null,
            EventCaptions = result.GetValue(EventCaptions) ? new GifEventCaptionOptions(true, true, true, true, true) : null,
            TitleCards = titleCards,
            CaptureOptimization = captureOptimization
        };
        return true;
    }
}
