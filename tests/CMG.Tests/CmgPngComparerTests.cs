using CMG.Runner;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class CmgPngComparerTests
{
    [Fact]
    public void Compare_ReturnsZeroForIdenticalImages()
    {
        var image = CreatePng(new Rgba32(255, 0, 0));

        Assert.Equal(0, CmgPngComparer.Compare(image, image));
    }

    [Fact]
    public void Compare_ReturnsDifferenceForChangedImages()
    {
        var red = CreatePng(new Rgba32(255, 0, 0));
        var blue = CreatePng(new Rgba32(0, 0, 255));

        Assert.True(CmgPngComparer.Compare(red, blue) > 0);
    }

    private static byte[] CreatePng(Rgba32 color)
    {
        using var image = new Image<Rgba32>(2, 2, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
