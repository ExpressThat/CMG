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
            "--gif-crop", "#panel", "--gif-crop-padding", "16", "--gif-scale", "0.5", "--gif-max-width", "640", "--gif-max-height", "480", "--gif-debug", "--gif-accessibility", "--gif-event-captions",
            "--gif-intro", "Start", "--gif-outro", "Done", "--gif-intro-duration", "500", "--gif-outro-duration", "700", "--gif-result-outro"]);

        Assert.True(options.TryParse(result, out var encoding, out var error), error);
        Assert.Equal(GifDitherMode.Sierra, encoding.Dither);
        Assert.Equal(GifPaletteMode.Local, encoding.Palette);
        Assert.Equal(144, encoding.Colors);
        Assert.Equal(Path.GetFullPath(directory), encoding.KeepFramesDirectory);
        Assert.Equal(new GifFramingOptions("#panel", 16, 0.5, 640, 480), encoding.Framing);
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
    [InlineData("--gif-crop-padding", "4", "requires crop=")]
    [InlineData("--gif-intro-duration", "0", "introDuration= must be greater than zero")]
    [InlineData("--gif-outro-duration", "-1", "outroDuration= must be greater than zero")]
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
            options.CropPadding, options.Scale, options.MaxWidth, options.MaxHeight, options.Debug, options.Accessibility, options.EventCaptions,
            options.Intro, options.Outro, options.IntroDuration, options.OutroDuration, options.ResultOutro }
    };
}
