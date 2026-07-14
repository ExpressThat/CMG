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
    Option<string?> SmartCrop,
    Option<string?> SplitTabs,
    Option<double?> Scale,
    Option<int?> MaxWidth,
    Option<int?> MaxHeight,
    Option<string?> Viewport,
    Option<double?> PixelRatio,
    Option<int?> SafeArea,
    Option<int?> LayoutStability,
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
    Option<string?> TargetZoom,
    Option<int?> TargetZoomThreshold,
    Option<string?> PagePosition,
    Option<string?> TabContext,
    Option<bool> DisableFocusPulse,
    Option<string?> PointerIdle,
    Option<int?> PointerIdleThreshold,
    Option<bool> DisableTeleportMarker,
    Option<int?> MouseDownHold,
    Option<string?> Background,
    Option<string?> GradientMode,
    Option<bool> HighContrastPalette,
    Option<string[]> Redact,
    Option<string[]> Mask,
    Option<string[]> Blur,
    Option<string?> AutoRedact,
    Option<string?> RedactionSafety)
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
            new Option<string?>("--gif-smart-crop") { Description = "Follow the pointer and active target using true or a fixed <width>x<height> crop." },
            new Option<string?>("--gif-split-tabs") { Description = "Multi-tab split view: auto, always, or none." },
            new Option<double?>("--gif-scale") { Description = "Output scale from 0.05 to 1." },
            new Option<int?>("--gif-max-width") { Description = "Maximum GIF width from 1 to 10000 pixels." },
            new Option<int?>("--gif-max-height") { Description = "Maximum GIF height from 1 to 10000 pixels." },
            new Option<string?>("--gif-viewport") { Description = "Temporary recording viewport as <width>x<height>." },
            new Option<double?>("--gif-pixel-ratio") { Description = "Recording device pixel ratio from 1 to 4." },
            new Option<int?>("--gif-safe-area") { Description = "Minimum target/crop safety margin from 0 to 500 CSS pixels." },
            new Option<int?>("--gif-layout-stability") { Description = "Target settling window from 0 to 5000 milliseconds." },
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
            new Option<string?>("--target-zoom") { Description = $"Tiny-target zoom inset: {GifPointerEvidenceOptions.CalloutValues}." },
            new Option<int?>("--target-zoom-threshold") { Description = "Auto-zoom target size threshold from 8 to 100 CSS pixels." },
            new Option<string?>("--page-position") { Description = $"Long-page position indicator: {GifPointerEvidenceOptions.CalloutValues}." },
            new Option<string?>("--tab-context") { Description = $"Active-tab recording badge: {GifPointerEvidenceOptions.CalloutValues}." },
            new Option<bool>("--no-pointer-focus-pulse") { Description = "Disable focused-control pulses after keyboard/focus actions." },
            new Option<string?>("--pointer-idle") { Description = $"Long-hold pointer evidence: {GifPointerEvidenceOptions.IdleValues}." },
            new Option<int?>("--pointer-idle-threshold") { Description = "Long-hold pointer pulse threshold from 100 to 60000 milliseconds." },
            new Option<bool>("--no-pointer-teleport-marker") { Description = "Disable origin/path evidence for instant pointer moves." },
            new Option<int?>("--mouse-down-hold") { Description = "Pressed-pointer hold after mouseDown, from 0 to 60000 milliseconds." },
            new Option<string?>("--gif-background") { Description = "Flatten transparent pixels onto this CSS color." },
            new Option<string?>("--gif-gradient-mode") { Description = $"GIF color tuning: {GifColorOptions.GradientModeValues}." },
            new Option<bool>("--gif-high-contrast-palette") { Description = "Increase frame contrast and saturation for accessibility review." },
            new Option<string[]>("--gif-redact") { Description = "Solid-mask a selector in every GIF frame. Repeatable." },
            new Option<string[]>("--gif-mask") { Description = "Alias-style solid mask selector. Repeatable." },
            new Option<string[]>("--gif-blur") { Description = "Blur a selector in every GIF frame. Repeatable." },
            new Option<string?>("--gif-auto-redact") { Description = "Automatic redaction: passwords, tokens, emails, payment, privacy, or none." },
            new Option<string?>("--gif-redaction-safety") { Description = "Redaction safety: standard or strict." });
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
            Value(result, SafeArea, settings?.SafeArea), Value(result, LayoutStability, settings?.LayoutStability),
            Value(result, SmartCrop, settings?.SmartCrop),
            Value(result, SplitTabs, settings?.SplitTabs),
            out var framing, out error)) return false;
        if (!GifTitleCardOptions.TryParse(
            result.GetValue(Intro), result.GetValue(Outro), result.GetValue(IntroDuration), result.GetValue(OutroDuration),
            result.GetValue(ResultOutro), out var titleCards, out error)) return false;
        if (!GifCaptureOptimizationOptions.TryParse(
            result.GetValue(DisableCoalescing), result.GetValue(SampleEvery), out var captureOptimization, out error)) return false;
        if (!GifPointerEvidenceOptions.TryParse(
            result.GetValue(PointerContrast), result.GetValue(PointerCallout), result.GetValue(PointerCalloutThreshold),
            Value(result, TargetZoom, settings?.TargetZoom), Value(result, TargetZoomThreshold, settings?.TargetZoomThreshold), Value(result, PagePosition, settings?.PagePosition), Value(result, TabContext, settings?.TabContext),
            result.GetValue(DisableFocusPulse), result.GetValue(PointerIdle), result.GetValue(PointerIdleThreshold),
            result.GetValue(DisableTeleportMarker), result.GetValue(MouseDownHold), out var pointerEvidence, out error)) return false;
        if (!GifColorOptions.TryParse(
            result.GetValue(Background), result.GetValue(GradientMode), result.GetValue(HighContrastPalette),
            out var color, out error)) return false;
        if (!TryRedaction(result, settings, out var redaction, out error)) return false;
        encoding = encoding with
        {
            Framing = framing,
            Diagnostics = result.GetValue(Debug) ? new GifDebugOptions(true, true, true, true, true, true) : null,
            Accessibility = result.GetValue(Accessibility) ? new GifAccessibilityOptions(true, true, true, true, true) : null,
            EventCaptions = result.GetValue(EventCaptions) ? new GifEventCaptionOptions(true, true, true, true, true) : null,
            TitleCards = titleCards,
            CaptureOptimization = captureOptimization,
            PointerEvidence = pointerEvidence,
            Color = color,
            Redaction = redaction
        };
        return true;
    }

    public bool TryParse(ParseResult result, out GifEncodingOptions encoding, out string? error) =>
        TryParse(result, settings: null, out encoding, out error);

    private static T? Value<T>(ParseResult result, Option<T?> option, T? fallback) =>
        Provided(result, option) ? result.GetValue(option) : fallback;

    private static bool Provided(ParseResult result, Option option) =>
        result.Tokens.Any(token => option.Aliases.Prepend(option.Name).Contains(token.Value, StringComparer.Ordinal));

    private bool TryRedaction(ParseResult result, RunGifSettings? settings, out GifRedactionOptions redaction, out string? error)
    {
        try
        {
            var automatic = Provided(result, AutoRedact) ? result.GetValue(AutoRedact) : settings?.AutoRedact;
            var safety = Provided(result, RedactionSafety) ? result.GetValue(RedactionSafety) : settings?.RedactionSafety;
            var parsed = GifRedactionOptions.FromOptions(new Dictionary<string, string>
            {
                ["autoRedact"] = automatic ?? "passwords", ["redactionSafety"] = safety ?? "standard"
            }, "GIF");
            var rules = Rules(Values(result, Redact, settings?.Redact), "redact", GifRedactionStyle.Solid)
                .Concat(Rules(Values(result, Mask, settings?.Mask), "mask", GifRedactionStyle.Solid))
                .Concat(Rules(Values(result, Blur, settings?.Blur), "blur", GifRedactionStyle.Blur)).ToArray();
            redaction = parsed with { Rules = rules }; error = null; return true;
        }
        catch (CMG.Browser.Scripting.ScriptExecutionException exception)
        { redaction = new(); error = exception.Message; return false; }
    }

    private static IReadOnlyList<string> Values(ParseResult result, Option<string[]> option, IReadOnlyList<string>? fallback) =>
        Provided(result, option) ? result.GetValue(option) ?? [] : fallback ?? [];

    private static IEnumerable<GifRedactionRule> Rules(IReadOnlyList<string> selectors, string prefix, GifRedactionStyle style) =>
        selectors.Select((selector, index) => string.IsNullOrWhiteSpace(selector)
            ? throw new CMG.Browser.Scripting.ScriptExecutionException($"GIF option --gif-{prefix} requires a non-empty selector.")
            : new GifRedactionRule($"cli-{prefix}-{index + 1}", selector, style, "#111827", "[redacted]", 0));
}
