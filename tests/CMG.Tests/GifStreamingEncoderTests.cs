using CMG.Browser.Scripting.Recording;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CMG.Tests;

public sealed class GifStreamingEncoderTests
{
    [Fact]
    public void QuantizedFrame_RoundTripsPaletteAndRows()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.qf");
        try
        {
            using var image = new Image<Rgba32>(8, 8, Color.Blue);
            image.Mutate(context => context.Quantize());

            GifQuantizedFrame.Write(path, image, 0, 0, 8, 8, 10);
            var stored = GifQuantizedFrame.Read(path);

            Assert.Equal(64, stored.Pixels.Length);
            Assert.All(stored.Pixels, pixel => Assert.InRange(pixel, 0, stored.Palette.Length - 1));
            Assert.All(stored.Pixels, pixel => Assert.Equal(new Rgba32(0, 0, 255, 255), stored.Palette[pixel]));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Save_StreamsChangedRegionsWithBoundedCaptureMemory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        try
        {
            using var sink = new GifFrameSink(encoding: new GifEncodingOptions(Dither: GifDitherMode.None));
            for (var frame = 0; frame < 20; frame++) sink.AddFrame(MovingPixel(frame), 2, false);

            sink.Save(path);

            Assert.Equal(19, sink.ChangedRegionFrameCount);
            Assert.InRange(sink.PeakRetainedPixelBytes, 1, 100 * 80 * 4 * 2);
            Assert.Equal(20, sink.ParallelPreprocessedFrameCount);
            using var decoded = Image.Load<Rgba32>(path);
            Assert.Equal(20, decoded.Frames.Count);
            Assert.Equal(new Rgba32(240, 40, 40, 255), decoded.Frames[^1][19, 20]);
            Assert.Equal(new Rgba32(248, 250, 252, 255), decoded.Frames[^1][18, 20]);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Save_RoundTripsLargeIndexedFrameAcrossLzwCodeWidths()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var source = IndexedNoise();
        try
        {
            using var sink = new GifFrameSink(encoding: new GifEncodingOptions(
                Dither: GifDitherMode.None, Colors: 16));
            sink.AddFrame(source, 10);
            sink.Save(path);

            using var expected = Image.Load<Rgba32>(source);
            using var actual = Image.Load<Rgba32>(path);
            Assert.Equal(expected.Width, actual.Width);
            Assert.Equal(expected.Height, actual.Height);
            for (var y = 0; y < expected.Height; y++)
                for (var x = 0; x < expected.Width; x++)
                    Assert.Equal(expected[x, y], actual[x, y]);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void ExplicitGlobalPalette_UsesWholeAnimationFallback()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        try
        {
            using var sink = new GifFrameSink(encoding: new GifEncodingOptions(Palette: GifPaletteMode.Global));
            sink.AddFrame(MovingPixel(1), 10);
            sink.AddFrame(MovingPixel(2), 20);
            sink.Save(path);

            Assert.False(sink.UsesStreamingGif);
            using var decoded = Image.Load<Rgba32>(path);
            Assert.Equal(2, decoded.Frames.Count);
            var duration = 0;
            foreach (var frame in decoded.Frames) duration += frame.Metadata.GetGifMetadata().FrameDelay * 10;
            Assert.Equal(300, duration);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Dispose_RemovesPrivateFrameSpool()
    {
        var sink = new GifFrameSink();
        sink.AddFrame(MovingPixel(1), 10);
        var directory = sink.SpoolDirectory;

        sink.Dispose();

        Assert.False(Directory.Exists(directory));
    }

    private static byte[] MovingPixel(int x)
    {
        using var image = new Image<Rgba32>(100, 80, new Rgba32(248, 250, 252));
        image[x, 20] = new Rgba32(240, 40, 40);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private static byte[] IndexedNoise()
    {
        var palette = new[]
        {
            Color.Red, Color.Blue, Color.Green, Color.Yellow,
            Color.Cyan, Color.Magenta, Color.White, Color.Black
        };
        using var image = new Image<Rgba32>(180, 90);
        var random = new Random(42);
        for (var y = 0; y < image.Height; y++)
            for (var x = 0; x < image.Width; x++) image[x, y] = palette[random.Next(palette.Length)];
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
