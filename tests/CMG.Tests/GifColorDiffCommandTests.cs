using System.CommandLine;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

[Collection(ConsoleCommandTestCollection.Name)]
public sealed class GifColorDiffCommandTests
{
    [Fact]
    public void ColorDiff_PrintsQuantizationErrorMetrics()
    {
        var directory = Directory.CreateTempSubdirectory("cmg-color-diff-");
        var source = Path.Combine(directory.FullName, "source.png");
        var gif = Path.Combine(directory.FullName, "encoded.gif");
        var png = GradientPng(source);
        using (var sink = new GifFrameSink(encoding: new GifEncodingOptions(GifDitherMode.None, Colors: 2)))
        {
            sink.AddFrame(png, 10);
            sink.Save(gif);
        }

        var output = new StringWriter();
        var original = Console.Out;
        try
        {
            Console.SetOut(output);
            var exitCode = BuildRoot().Parse(["gif", "color-diff", source, gif]).Invoke();

            Assert.Equal(0, exitCode);
            var line = output.ToString();
            Assert.Contains("GIF_COLOR_DIFF", line, StringComparison.Ordinal);
            Assert.Contains("frame=1 width=32 height=16", line, StringComparison.Ordinal);
            Assert.True(ReadMetric(line, "meanAbsoluteError") > 0);
            Assert.True(ReadMetric(line, "changedPixels") > 0);
        }
        finally
        {
            Console.SetOut(original);
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void ColorDiff_RejectsOutOfRangeFrameWithReason()
    {
        var directory = Directory.CreateTempSubdirectory("cmg-color-frame-");
        var source = Path.Combine(directory.FullName, "source.png");
        var gif = Path.Combine(directory.FullName, "encoded.gif");
        var png = GradientPng(source);
        using (var sink = new GifFrameSink()) { sink.AddFrame(png, 10); sink.Save(gif); }
        var error = new StringWriter();
        var original = Console.Error;
        try
        {
            Console.SetError(error);
            var exitCode = BuildRoot().Parse(["gif", "color-diff", source, gif, "--frame", "2"]).Invoke();
            Assert.Equal(1, exitCode);
            Assert.Contains("contains 1 frame(s); frame 2 does not exist", error.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(original);
            directory.Delete(recursive: true);
        }
    }

    private static byte[] GradientPng(string path)
    {
        using var image = new Image<Rgba32>(32, 16);
        image.ProcessPixelRows(rows =>
        {
            for (var y = 0; y < rows.Height; y++)
                for (var x = 0; x < rows.Width; x++)
                    rows.GetRowSpan(y)[x] = new Rgba32((byte)(x * 8), (byte)(y * 16), (byte)((x + y) * 5));
        });
        image.SaveAsPng(path);
        return File.ReadAllBytes(path);
    }

    private static double ReadMetric(string output, string name)
    {
        var token = output.Split(' ', StringSplitOptions.RemoveEmptyEntries).Single(value => value.StartsWith($"{name}=", StringComparison.Ordinal));
        return double.Parse(token[(name.Length + 1)..], System.Globalization.CultureInfo.InvariantCulture);
    }

    private static RootCommand BuildRoot()
    {
        var root = new RootCommand();
        root.Subcommands.Add(new GifCommandBuilder().Build());
        return root;
    }
}
