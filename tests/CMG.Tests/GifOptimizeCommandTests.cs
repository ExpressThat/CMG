using System.CommandLine;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

[Collection(ConsoleCommandTestCollection.Name)]
public sealed class GifOptimizeCommandTests
{
    [Fact]
    public void Optimize_RemovesConsecutiveDuplicateFramesAndPreservesDuration()
    {
        var input = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var output = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-optimized.gif");
        using var sink = new GifFrameSink(encoding: new GifEncodingOptions(
            CaptureOptimization: new GifCaptureOptimizationOptions(CoalesceDuplicates: false)));
        sink.AddFrame(Png(Color.Red), 10);
        sink.AddFrame(Png(Color.Red), 10);
        sink.AddFrame(Png(Color.Blue), 10);
        sink.AddFrame(Png(Color.Blue), 10);
        sink.Save(input);
        var writer = new StringWriter();
        var original = Console.Out;
        try
        {
            Console.SetOut(writer);
            var exitCode = BuildRoot().Parse(["gif", "optimize", input, "--output", output]).Invoke();

            Assert.Equal(0, exitCode);
            var text = writer.ToString();
            Assert.Contains("GIF_OPTIMIZE", text, StringComparison.Ordinal);
            Assert.Contains("framesBefore=4", text, StringComparison.Ordinal);
            Assert.Contains("framesAfter=2", text, StringComparison.Ordinal);
            Assert.Contains("duplicateFramesRemoved=2", text, StringComparison.Ordinal);
            Assert.Equal(400, GifInspector.Inspect(new FileInfo(output)).DurationMilliseconds);
        }
        finally
        {
            Console.SetOut(original);
            File.Delete(input);
            File.Delete(output);
        }
    }

    [Fact]
    public void Optimize_RequiresOutput()
    {
        var input = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.Red), 10);
        sink.Save(input);
        var error = new StringWriter();
        var original = Console.Error;
        try
        {
            Console.SetError(error);
            var exitCode = BuildRoot().Parse(["gif", "optimize", input]).Invoke();

            Assert.Equal(1, exitCode);
            Assert.Contains("requires --output", error.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(original);
            File.Delete(input);
        }
    }

    private static RootCommand BuildRoot()
    {
        var root = new RootCommand();
        root.Subcommands.Add(new GifCommandBuilder().Build());
        return root;
    }

    private static byte[] Png(Color color)
    {
        using var image = new Image<Rgba32>(8, 8, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
