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
    Option<string?> Viewport,
    Option<double?> PixelRatio,
    Option<bool> Debug,
    Option<bool> Accessibility,
    Option<bool> EventCaptions,
    Option<string?> Intro,
    Option<string?> Outro,
    Option<int?> IntroDuration,
    Option<int?> OutroDuration,
    Option<bool> ResultOutro,
    Option<bool> DisableCoalescing,
    Option<int?> SampleEvery,
    Option<string?> PointerContrast,
    Option<string?> PointerCallout,
    Option<int?> PointerCalloutThreshold,
    Option<bool> DisableFocusPulse,
    Option<string?> PointerIdle,
    Option<int?> PointerIdleThreshold,
    Option<bool> DisableTeleportMarker,
    Option<int?> MouseDownHold,
    Option<string?> Background,
    Option<string?> GradientMode,
    Option<bool> HighContrastPalette)
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
            new Option<string?>("--gif-viewport") { Description = "Temporary recording viewport as <width>x<height>." },
            new Option<double?>("--gif-pixel-ratio") { Description = "Recording device pixel ratio from 1 to 4." },
            new Option<bool>("--gif-debug") { Description = "Show action, scope, target, pointer, and scroll diagnostics in GIF frames." },
            new Option<bool>("--gif-accessibility") { Description = "Show keyboard, focus, accessible-name, and contrast evidence in GIF frames." },
            new Option<bool>("--gif-event-captions") { Description = "Show safe outcome captions for network, dialog, console, download, and upload events." },
            new Option<string?>("--gif-intro") { Description = "Opening title-card text for the whole GIF." },
            new Option<string?>("--gif-outro") { Description = "Final title-card text for the whole GIF." },
            new Option<int?>("--gif-intro-duration") { Description = "Opening title-card duration in milliseconds." },
            new Option<int?>("--gif-outro-duration") { Description = "Final title-card duration in milliseconds." },
            new Option<bool>("--gif-result-outro") { Description = "Generate a passed, failed, or skipped final title card." },
            new Option<bool>("--gif-no-coalesce") { Description = "Keep consecutive duplicate frames instead of merging their delays." },
            new Option<int?>("--gif-sample-every") { Description = "Keep every Nth intermediate pointer/drag frame, from 1 to 100." },
            new Option<string?>("--pointer-contrast") { Description = $"Virtual pointer contrast: {GifPointerEvidenceOptions.ContrastValues}." },
            new Option<string?>("--pointer-callout") { Description = $"Target callouts: {GifPointerEvidenceOptions.CalloutValues}." },
            new Option<int?>("--pointer-callout-threshold") { Description = "Auto-callout target size threshold from 8 to 100 CSS pixels." },
            new Option<bool>("--no-pointer-focus-pulse") { Description = "Disable focused-control pulses after keyboard/focus actions." },
            new Option<string?>("--pointer-idle") { Description = $"Long-hold pointer evidence: {GifPointerEvidenceOptions.IdleValues}." },
            new Option<int?>("--pointer-idle-threshold") { Description = "Long-hold pointer pulse threshold from 100 to 60000 milliseconds." },
            new Option<bool>("--no-pointer-teleport-marker") { Description = "Disable origin/path evidence for instant pointer moves." },
            new Option<int?>("--mouse-down-hold") { Description = "Pressed-pointer hold after mouseDown, from 0 to 60000 milliseconds." },
            new Option<string?>("--gif-background") { Description = "Flatten transparent pixels onto this CSS color." },
            new Option<string?>("--gif-gradient-mode") { Description = $"GIF color tuning: {GifColorOptions.GradientModeValues}." },
            new Option<bool>("--gif-high-contrast-palette") { Description = "Increase frame contrast and saturation for accessibility review." });
    }

    public bool TryParse(ParseResult result, RunGifSettings? settings, out GifEncodingOptions encoding, out string? error)
    {
        if (!GifEncodingOptions.TryParse(
            result.GetValue(Dither),
            result.GetValue(Palette),
            result.GetValue(Colors),
            result.GetValue(KeepFrames)?.FullName,
            out encoding,
            out error)) return false;
        if (!GifFramingOptions.TryParse(
            Value(result, Crop, settings?.Crop), Value(result, CropPadding, settings?.CropPadding), Value(result, Scale, settings?.Scale),
            Value(result, MaxWidth, settings?.MaxWidth), Value(result, MaxHeight, settings?.MaxHeight), Value(result, Viewport, settings?.Viewport), Value(result, PixelRatio, settings?.PixelRatio),
            out var framing, out error)) return false;
        if (!GifTitleCardOptions.TryParse(
            result.GetValue(Intro), result.GetValue(Outro), result.GetValue(IntroDuration), result.GetValue(OutroDuration),
            result.GetValue(ResultOutro), out var titleCards, out error)) return false;
        if (!GifCaptureOptimizationOptions.TryParse(
            result.GetValue(DisableCoalescing), result.GetValue(SampleEvery), out var captureOptimization, out error)) return false;
        if (!GifPointerEvidenceOptions.TryParse(
            result.GetValue(PointerContrast), result.GetValue(PointerCallout), result.GetValue(PointerCalloutThreshold),
            result.GetValue(DisableFocusPulse), result.GetValue(PointerIdle), result.GetValue(PointerIdleThreshold),
            result.GetValue(DisableTeleportMarker), result.GetValue(MouseDownHold), out var pointerEvidence, out error)) return false;
        if (!GifColorOptions.TryParse(
            result.GetValue(Background), result.GetValue(GradientMode), result.GetValue(HighContrastPalette),
            out var color, out error)) return false;
        encoding = encoding with
        {
            Framing = framing,
            Diagnostics = result.GetValue(Debug) ? new GifDebugOptions(true, true, true, true, true, true) : null,
            Accessibility = result.GetValue(Accessibility) ? new GifAccessibilityOptions(true, true, true, true, true) : null,
            EventCaptions = result.GetValue(EventCaptions) ? new GifEventCaptionOptions(true, true, true, true, true) : null,
            TitleCards = titleCards,
            CaptureOptimization = captureOptimization,
            PointerEvidence = pointerEvidence,
            Color = color
        };
        return true;
    }

    public bool TryParse(ParseResult result, out GifEncodingOptions encoding, out string? error) =>
        TryParse(result, settings: null, out encoding, out error);

    private static T? Value<T>(ParseResult result, Option<T?> option, T? fallback) =>
        Provided(result, option) ? result.GetValue(option) : fallback;

    private static bool Provided(ParseResult result, Option option) =>
        result.Tokens.Any(token => option.Aliases.Prepend(option.Name).Contains(token.Value, StringComparer.Ordinal));
}
