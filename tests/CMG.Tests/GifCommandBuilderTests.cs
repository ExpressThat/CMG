using System.CommandLine;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

[Collection(ConsoleCommandTestCollection.Name)]
public sealed class GifCommandBuilderTests
{
    [Fact]
    public void Inspect_PrintsGifMetadata()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.Red), 10);
        sink.AddFrame(Png(Color.Blue), 25);
        sink.Save(path);

        var output = new StringWriter();
        var original = Console.Out;
        try
        {
            Console.SetOut(output);
            var exitCode = BuildRoot().Parse(["gif", "inspect", path]).Invoke();

            Assert.Equal(0, exitCode);
            var line = output.ToString();
            Assert.Contains("GIF_INSPECT", line, StringComparison.Ordinal);
            Assert.Contains("frames=2", line, StringComparison.Ordinal);
            Assert.Contains("durationMs=350", line, StringComparison.Ordinal);
            Assert.Contains("width=8 height=8", line, StringComparison.Ordinal);
            Assert.Contains("sizeBytes=", line, StringComparison.Ordinal);
            Assert.Contains("palette=", line, StringComparison.Ordinal);
            Assert.Contains("paletteColors=2", line, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(original);
            File.Delete(path);
        }
    }

    [Fact]
    public void Inspect_MissingFile_ReturnsFailure()
    {
        var error = new StringWriter();
        var original = Console.Error;
        try
        {
            Console.SetError(error);
            var exitCode = BuildRoot().Parse(["gif", "inspect", "missing.gif"]).Invoke();

            Assert.Equal(1, exitCode);
            Assert.Contains("was not found", error.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(original);
        }
    }

    [Fact]
    public void Inspect_NonGif_ReturnsFailure()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        File.WriteAllBytes(path, Png(Color.Green));
        var error = new StringWriter();
        var original = Console.Error;
        try
        {
            Console.SetError(error);
            var exitCode = BuildRoot().Parse(["gif", "inspect", path]).Invoke();

            Assert.Equal(1, exitCode);
            Assert.Contains("Expected a GIF image", error.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(original);
            File.Delete(path);
        }
    }

    [Fact]
    public void Compare_PrintsMetadataDeltas()
    {
        var before = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-before.gif");
        var after = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-after.gif");
        using var beforeSink = new GifFrameSink();
        beforeSink.AddFrame(Png(Color.Red), 10);
        beforeSink.Save(before);
        using var afterSink = new GifFrameSink();
        afterSink.AddFrame(Png(Color.Red), 10);
        afterSink.AddFrame(Png(Color.Blue), 25);
        afterSink.Save(after);
        var output = new StringWriter();
        var original = Console.Out;
        try
        {
            Console.SetOut(output);
            var exitCode = BuildRoot().Parse(["gif", "compare", before, after]).Invoke();

            Assert.Equal(0, exitCode);
            var line = output.ToString();
            Assert.Contains("GIF_COMPARE", line, StringComparison.Ordinal);
            Assert.Contains("framesDelta=1", line, StringComparison.Ordinal);
            Assert.Contains("durationMsDelta=250", line, StringComparison.Ordinal);
            Assert.Contains("sameDimensions=true", line, StringComparison.Ordinal);
            Assert.Contains("paletteColorsBefore=1", line, StringComparison.Ordinal);
            Assert.Contains("paletteColorsAfter=2", line, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(original);
            File.Delete(before);
            File.Delete(after);
        }
    }

    [Fact]
    public void Compare_MissingFile_ReturnsFailure()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.Red), 10);
        sink.Save(path);
        var error = new StringWriter();
        var original = Console.Error;
        try
        {
            Console.SetError(error);
            var exitCode = BuildRoot().Parse(["gif", "compare", "missing.gif", path]).Invoke();

            Assert.Equal(1, exitCode);
            Assert.Contains("GIF before file", error.ToString(), StringComparison.Ordinal);
            Assert.Contains("was not found", error.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(original);
            File.Delete(path);
        }
    }

    [Fact]
    public void Presets_PrintsPresetFamilies()
    {
        var output = new StringWriter();
        var original = Console.Out;
        try
        {
            Console.SetOut(output);
            var exitCode = BuildRoot().Parse(["gif", "presets"]).Invoke();

            Assert.Equal(0, exitCode);
            var text = output.ToString();
            Assert.Contains("GIF_PRESETS quality=highest,high,medium,low defaultQuality=highest", text, StringComparison.Ordinal);
            Assert.Contains("pointerSpeed=slow,normal,fast,instant,multiplier", text, StringComparison.Ordinal);
            Assert.Contains("pointerEasing=linear,ease-in,ease-out,ease-in-out,spring", text, StringComparison.Ordinal);
            Assert.Contains("clickPulse=ring,ripple,dot,crosshair,none defaultClickPulse=ring", text, StringComparison.Ordinal);
            Assert.Contains("defaultHoldAfterActionMs=350", text, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(original);
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
