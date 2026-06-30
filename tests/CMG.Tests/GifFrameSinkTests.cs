using CMG.Browser.Scripting.Recording;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace CMG.Tests;

public sealed class GifFrameSinkTests
{
    [Fact]
    public void CreateEncoder_UsesColorFidelitySettings()
    {
        var encoder = GifFrameSink.CreateEncoder();

        Assert.Equal(GifColorTableMode.Global, encoder.ColorTableMode);
        var quantizer = Assert.IsType<WuQuantizer>(encoder.Quantizer);
        Assert.Equal(256, quantizer.Options.MaxColors);
        Assert.Equal(ColorMatchingMode.Exact, quantizer.Options.ColorMatchingMode);
        Assert.NotNull(quantizer.Options.Dither);
        Assert.Equal(0.75f, quantizer.Options.DitherScale);
    }

    [Theory]
    [InlineData("highest", GifQuality.Highest)]
    [InlineData("best", GifQuality.Highest)]
    [InlineData("high", GifQuality.High)]
    [InlineData("medium", GifQuality.Medium)]
    [InlineData("low", GifQuality.Low)]
    public void TryParse_AcceptsQualityNames(string value, GifQuality expected)
    {
        Assert.True(GifQualityParser.TryParse(value, out var quality));
        Assert.Equal(expected, quality);
    }

    [Fact]
    public void CreateEncoder_LowUsesCompactPalette()
    {
        var encoder = GifFrameSink.CreateEncoder(GifQuality.Low);

        Assert.Equal(GifColorTableMode.Local, encoder.ColorTableMode);
        var quantizer = Assert.IsType<OctreeQuantizer>(encoder.Quantizer);
        Assert.Equal(64, quantizer.Options.MaxColors);
        Assert.Null(quantizer.Options.Dither);
    }
}
