using CMG.Browser.Scripting.Recording;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
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

    [Fact]
    public void Save_WritesFullFrameDisposalMetadata()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.White), 10);
        sink.AddFrame(Png(Color.Black), 10);

        sink.Save(path);

        using var gif = Image.Load<Rgba32>(path);
        foreach (var frame in gif.Frames)
        {
            var metadata = frame.Metadata.GetGifMetadata();
            Assert.Equal(GifDisposalMethod.RestoreToBackground, metadata.DisposalMethod);
        }
        File.Delete(path);
    }

    private static byte[] Png(Color color)
    {
        using var image = new Image<Rgba32>(8, 8, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
