using System.CommandLine;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

[Collection(ConsoleCommandTestCollection.Name)]
public sealed class GifStoryboardCommandTests
{
    [Fact]
    public void Storyboard_ExportsContactSheet()
    {
        var input = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var output = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.Red), 10);
        sink.AddFrame(Png(Color.Blue), 10);
        sink.AddFrame(Png(Color.Green), 10);
        sink.Save(input);
        var writer = new StringWriter();
        var original = Console.Out;
        try
        {
            Console.SetOut(writer);
            var exitCode = BuildRoot().Parse(["gif", "storyboard", input, "--output", output, "--columns", "2"]).Invoke();

            Assert.Equal(0, exitCode);
            Assert.Contains("GIF_STORYBOARD", writer.ToString(), StringComparison.Ordinal);
            Assert.Contains("frames=3/3", writer.ToString(), StringComparison.Ordinal);
            using var storyboard = Image.Load<Rgba32>(output);
            Assert.Equal(16, storyboard.Width);
            Assert.Equal(16, storyboard.Height);
        }
        finally
        {
            Console.SetOut(original);
            File.Delete(input);
            File.Delete(output);
        }
    }

    [Fact]
    public void Storyboard_RequiresOutput()
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
            var exitCode = BuildRoot().Parse(["gif", "storyboard", input]).Invoke();

            Assert.Equal(1, exitCode);
            Assert.Contains("requires --output", error.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(original);
            File.Delete(input);
        }
    }

    [Fact]
    public void Storyboard_RejectsInvalidColumns()
    {
        var input = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var output = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.Red), 10);
        sink.Save(input);
        var error = new StringWriter();
        var original = Console.Error;
        try
        {
            Console.SetError(error);
            var exitCode = BuildRoot().Parse(["gif", "storyboard", input, "--output", output, "--columns", "0"]).Invoke();

            Assert.Equal(1, exitCode);
            Assert.Contains("columns must be at least 1", error.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(original);
            File.Delete(input);
            File.Delete(output);
        }
    }

    [Fact]
    public void Storyboard_CompositesTransparentFramePixelsOntoWhite()
    {
        var input = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var output = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        try
        {
            using (var gif = new Image<Rgba32>(8, 8, Color.White))
            using (var delta = new Image<Rgba32>(8, 8, Color.Transparent))
            {
                delta[4, 4] = Color.Blue;
                var metadata = delta.Frames.RootFrame.Metadata.GetGifMetadata();
                metadata.HasTransparency = true;
                metadata.DisposalMethod = GifDisposalMethod.NotDispose;
                gif.Frames.AddFrame(delta.Frames.RootFrame);
                gif.SaveAsGif(input);
            }

            GifStoryboardExporter.Export(new(input), new(output), columns: 2, maxFrames: null);

            using var storyboard = Image.Load<Rgba32>(output);
            Assert.Equal(new Rgba32(255, 255, 255, 255), storyboard[8, 0]);
            Assert.True(storyboard[12, 4].B > 200);
        }
        finally
        {
            File.Delete(input);
            File.Delete(output);
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
