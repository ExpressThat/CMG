using CMG.Browser.Scripting.Recording;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class GifFrameSinkDisposalTests
{
    [Fact]
    public void Save_UsesFullFrameDisposalForMovingPointerFrames()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        try
        {
            using (var sink = new GifFrameSink(encoding: new GifEncodingOptions(Dither: GifDitherMode.None)))
            {
                sink.AddFrame(Frame(pointerX: 8), 10);
                sink.AddFrame(Frame(pointerX: 32), 10);
                sink.Save(path);
            }

            using var gif = Image.Load<Rgba32>(path);
            Assert.Equal(2, gif.Frames.Count);
            for (var index = 0; index < gif.Frames.Count; index++)
                Assert.Equal(GifDisposalMethod.RestoreToBackground, gif.Frames[index].Metadata.GetGifMetadata().DisposalMethod);
            Assert.True(gif.Frames[1][4, 30].B > 150);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static byte[] Frame(int pointerX)
    {
        using var image = new Image<Rgba32>(48, 36, new Rgba32(20, 80, 210));
        for (var y = 6; y < 12; y++)
            for (var x = pointerX; x < pointerX + 5; x++) image[x, y] = Color.White;
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
