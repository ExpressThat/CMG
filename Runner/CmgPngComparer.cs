using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Runner;

public static class CmgPngComparer
{
    public static double Compare(byte[] expectedPng, byte[] actualPng)
    {
        using var expected = Image.Load<Rgba32>(expectedPng);
        using var actual = Image.Load<Rgba32>(actualPng);
        if (expected.Width != actual.Width || expected.Height != actual.Height)
        {
            return 1;
        }

        double total = 0;
        for (var y = 0; y < expected.Height; y++)
        {
            var expectedRow = expected.DangerousGetPixelRowMemory(y).Span;
            var actualRow = actual.DangerousGetPixelRowMemory(y).Span;
            for (var x = 0; x < expected.Width; x++)
            {
                total += PixelDifference(expectedRow[x], actualRow[x]);
            }
        }

        return total / (expected.Width * expected.Height);
    }

    private static double PixelDifference(Rgba32 left, Rgba32 right)
    {
        var dr = Math.Abs(left.R - right.R) / 255d;
        var dg = Math.Abs(left.G - right.G) / 255d;
        var db = Math.Abs(left.B - right.B) / 255d;
        var da = Math.Abs(left.A - right.A) / 255d;
        return (dr + dg + db + da) / 4d;
    }
}
