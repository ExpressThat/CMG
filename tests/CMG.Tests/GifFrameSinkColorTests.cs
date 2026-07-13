using CMG.Browser.Scripting.Recording;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace CMG.Tests;

public sealed partial class GifFrameSinkTests
{
    [Theory]
    [InlineData(GifGradientMode.Smooth, GifColorTableMode.Local, true)]
    [InlineData(GifGradientMode.Text, GifColorTableMode.Global, false)]
    public void CreateEncoder_GradientModeSelectsUsefulDefaults(
        GifGradientMode mode,
        GifColorTableMode table,
        bool expectsDither)
    {
        var encoder = GifFrameSink.CreateEncoder(
            encoding: new GifEncodingOptions(Color: new GifColorOptions(GradientMode: mode)));

        Assert.Equal(table, encoder.ColorTableMode);
        var quantizer = Assert.IsType<WuQuantizer>(encoder.Quantizer);
        Assert.Equal(expectsDither, quantizer.Options.Dither is not null);
        Assert.Equal(256, quantizer.Options.MaxColors);
    }

    [Fact]
    public void AddFrame_FlattensTransparencyOntoConfiguredBackground()
    {
        var directory = Directory.CreateTempSubdirectory("cmg-color-frames-");
        var frames = Path.Combine(directory.FullName, "frames");
        using var source = new Image<Rgba32>(1, 1, new Rgba32(255, 0, 0, 128));
        using var stream = new MemoryStream();
        source.SaveAsPng(stream);
        using var sink = new GifFrameSink(encoding: new GifEncodingOptions(
            KeepFramesDirectory: frames,
            Color: new GifColorOptions("#0000ff")));

        sink.AddFrame(stream.ToArray(), 10);

        using var retained = Image.Load<Rgba32>(Path.Combine(frames, "frame-0001.png"));
        var pixel = retained[0, 0];
        Assert.InRange(pixel.R, 127, 128);
        Assert.Equal(0, pixel.G);
        Assert.InRange(pixel.B, 127, 128);
        Assert.Equal(255, pixel.A);
        directory.Delete(recursive: true);
    }

    [Fact]
    public void AddFrame_ReportsPngColorMetadataChanges()
    {
        using var sink = new GifFrameSink();

        sink.AddFrame(PngWithGamma(0.45455f, Color.Red), 10);
        sink.AddFrame(PngWithGamma(1f, Color.Blue), 10);

        Assert.Equal(2, sink.GammaMetadataFrameCount);
        Assert.Equal(1, sink.ColorProfileChangeCount);
    }

    private static byte[] PngWithGamma(float gamma, Color color)
    {
        using var image = new Image<Rgba32>(8, 8, color);
        image.Metadata.GetPngMetadata().Gamma = gamma;
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
