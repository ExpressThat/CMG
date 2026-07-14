using System.CommandLine;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;

namespace CMG.Tests;

public sealed class GifEncodingCliOptionsTests
{
    [Fact]
    public void TryParse_MapsEveryWholeRunEncoderOption()
    {
        var options = GifEncodingCliOptions.Build();
        var root = Root(options);
        var directory = Path.Combine(Path.GetTempPath(), "cmg-frames");
        var result = root.Parse(["--gif-dither", "sierra", "--gif-palette", "local", "--gif-colors", "144", "--keep-frames", directory,
            "--gif-crop", "#panel", "--gif-crop-padding", "16", "--gif-scale", "0.5", "--gif-max-width", "640", "--gif-max-height", "480", "--gif-viewport", "1280x720", "--gif-pixel-ratio", "2", "--gif-safe-area", "32", "--gif-layout-stability", "400", "--gif-debug", "--gif-accessibility", "--gif-event-captions",
            "--gif-intro", "Start", "--gif-outro", "Done", "--gif-intro-duration", "500", "--gif-outro-duration", "700", "--gif-result-outro",
            "--gif-no-coalesce", "--gif-sample-every", "3", "--pointer-contrast", "fixed", "--pointer-callout", "always",
            "--pointer-callout-threshold", "32", "--target-zoom", "none", "--target-zoom-threshold", "18", "--page-position", "always", "--no-pointer-focus-pulse", "--pointer-idle", "none",
            "--pointer-idle-threshold", "900", "--no-pointer-teleport-marker", "--mouse-down-hold", "250",
            "--gif-background", "#112233", "--gif-gradient-mode", "smooth", "--gif-high-contrast-palette"]);

        Assert.True(options.TryParse(result, out var encoding, out var error), error);
        Assert.Equal(GifDitherMode.Sierra, encoding.Dither);
        Assert.Equal(GifPaletteMode.Local, encoding.Palette);
        Assert.Equal(144, encoding.Colors);
        Assert.Equal(Path.GetFullPath(directory), encoding.KeepFramesDirectory);
        Assert.Equal(new GifFramingOptions("#panel", 16, 0.5, 640, 480, 1280, 720, 2, 32, 400), encoding.Framing);
        Assert.True(encoding.Diagnostics?.Action);
        Assert.True(encoding.Diagnostics?.Context);
        Assert.True(encoding.Diagnostics?.Target);
        Assert.True(encoding.Accessibility?.ContrastWarnings);
        Assert.True(encoding.EventCaptions?.Uploads);
        Assert.Equal("Start", encoding.TitleCards?.Intro);
        Assert.Equal("Done", encoding.TitleCards?.Outro);
        Assert.Equal(500, encoding.TitleCards?.IntroDuration);
        Assert.Equal(700, encoding.TitleCards?.OutroDuration);
        Assert.True(encoding.TitleCards?.ResultOutro);
        Assert.False(encoding.CaptureOptimization?.CoalesceDuplicates);
        Assert.Equal(3, encoding.CaptureOptimization?.SampleEvery);
        Assert.Equal(PointerContrastMode.Fixed, encoding.PointerEvidence?.Contrast);
        Assert.Equal(PointerTargetCalloutMode.Always, encoding.PointerEvidence?.TargetCallout);
        Assert.Equal(32, encoding.PointerEvidence?.TargetCalloutThreshold);
        Assert.Equal(PointerTargetCalloutMode.None, encoding.PointerEvidence?.TargetZoom);
        Assert.Equal(18, encoding.PointerEvidence?.TargetZoomThreshold);
        Assert.Equal(PointerTargetCalloutMode.Always, encoding.PointerEvidence?.PagePosition);
        Assert.False(encoding.PointerEvidence?.FocusPulse);
        Assert.Equal(PointerIdleMode.None, encoding.PointerEvidence?.Idle);
        Assert.Equal(900, encoding.PointerEvidence?.IdleThresholdMilliseconds);
        Assert.False(encoding.PointerEvidence?.TeleportMarker);
        Assert.Equal(250, encoding.PointerEvidence?.MouseDownHoldMilliseconds);
        Assert.Equal("#112233", encoding.Color?.Background);
        Assert.Equal(GifGradientMode.Smooth, encoding.Color?.GradientMode);
        Assert.True(encoding.Color?.HighContrastPalette);
    }

    [Theory]
    [InlineData("--gif-dither", "sparkle", "dither=")]
    [InlineData("--gif-palette", "shared-ish", "palette=")]
    [InlineData("--gif-colors", "1", "colors=")]
    [InlineData("--gif-colors", "257", "colors=")]
    [InlineData("--gif-scale", "0", "scale=")]
    [InlineData("--gif-scale", "1.1", "scale=")]
    [InlineData("--gif-max-width", "0", "maxWidth=")]
    [InlineData("--gif-max-height", "10001", "maxHeight=")]
    [InlineData("--gif-viewport", "wide", "viewport=")]
    [InlineData("--gif-pixel-ratio", "5", "pixelRatio=")]
    [InlineData("--gif-safe-area", "501", "safeArea=")]
    [InlineData("--gif-layout-stability", "5001", "layoutStability=")]
    [InlineData("--gif-crop-padding", "4", "requires crop=")]
    [InlineData("--gif-intro-duration", "0", "introDuration= must be greater than zero")]
    [InlineData("--gif-outro-duration", "-1", "outroDuration= must be greater than zero")]
    [InlineData("--gif-sample-every", "0", "sampleEvery= must be an integer from 1 to 100")]
    [InlineData("--gif-sample-every", "101", "sampleEvery= must be an integer from 1 to 100")]
    [InlineData("--pointer-contrast", "sometimes", "pointerContrast=")]
    [InlineData("--pointer-callout", "near", "targetCallout=")]
    [InlineData("--pointer-callout-threshold", "7", "targetCalloutThreshold=")]
    [InlineData("--target-zoom", "near", "targetZoom=")]
    [InlineData("--target-zoom-threshold", "101", "targetZoomThreshold=")]
    [InlineData("--page-position", "sometimes", "pagePosition=")]
    [InlineData("--pointer-idle", "spin", "pointerIdle=")]
    [InlineData("--pointer-idle-threshold", "99", "pointerIdleThreshold=")]
    [InlineData("--mouse-down-hold", "60001", "mouseDownHold=")]
    [InlineData("--gif-background", "not-a-color", "background=")]
    [InlineData("--gif-gradient-mode", "photographic", "gradientMode=")]
    public void TryParse_RejectsInvalidValues(string option, string value, string expected)
    {
        var options = GifEncodingCliOptions.Build();
        var result = Root(options).Parse([option, value]);

        Assert.False(options.TryParse(result, out _, out var error));
        Assert.Contains(expected, error, StringComparison.Ordinal);
    }

    private static RootCommand Root(GifEncodingCliOptions options) => new()
    {
        Options = { options.Dither, options.Palette, options.Colors, options.KeepFrames, options.Crop,
            options.CropPadding, options.Scale, options.MaxWidth, options.MaxHeight, options.Viewport, options.PixelRatio, options.SafeArea, options.LayoutStability, options.Debug, options.Accessibility, options.EventCaptions,
            options.Intro, options.Outro, options.IntroDuration, options.OutroDuration, options.ResultOutro, options.DisableCoalescing, options.SampleEvery,
            options.PointerContrast, options.PointerCallout, options.PointerCalloutThreshold, options.TargetZoom, options.TargetZoomThreshold, options.PagePosition, options.DisableFocusPulse,
            options.PointerIdle, options.PointerIdleThreshold, options.DisableTeleportMarker, options.MouseDownHold,
            options.Background, options.GradientMode, options.HighContrastPalette }
    };
}
