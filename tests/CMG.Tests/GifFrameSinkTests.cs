using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using System.Text.Json;
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
    [InlineData("archival", GifQuality.Archival)]
    [InlineData("best", GifQuality.Highest)]
    [InlineData("high", GifQuality.High)]
    [InlineData("medium", GifQuality.Medium)]
    [InlineData("low", GifQuality.Low)]
    public void TryParse_AcceptsQualityNames(string value, GifQuality expected)
    {
        Assert.True(GifQualityParser.TryParse(value, out var quality));
        Assert.Equal(expected, quality);
    }

    [Theory]
    [InlineData("ring", ClickPulseStyle.Ring)]
    [InlineData("ripple", ClickPulseStyle.Ripple)]
    [InlineData("dot", ClickPulseStyle.Dot)]
    [InlineData("crosshair", ClickPulseStyle.Crosshair)]
    [InlineData("none", ClickPulseStyle.None)]
    public void TryParse_AcceptsClickPulseStyles(string value, ClickPulseStyle expected)
    {
        Assert.True(ClickPulseStyleParser.TryParse(value, out var style));
        Assert.Equal(expected, style);
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
    public void CreateEncoder_ArchivalUsesFrameLocalMaximumPalette()
    {
        var encoder = GifFrameSink.CreateEncoder(GifQuality.Archival);

        Assert.Equal(GifColorTableMode.Local, encoder.ColorTableMode);
        var quantizer = Assert.IsType<WuQuantizer>(encoder.Quantizer);
        Assert.Equal(256, quantizer.Options.MaxColors);
        Assert.Equal(1f, quantizer.Options.DitherScale);
    }

    [Fact]
    public void CreateEncoder_ExplicitControlsOverridePreset()
    {
        var options = new GifEncodingOptions(GifDitherMode.None, GifPaletteMode.Global, 32);
        var encoder = GifFrameSink.CreateEncoder(GifQuality.Archival, options);

        Assert.Equal(GifColorTableMode.Global, encoder.ColorTableMode);
        var quantizer = Assert.IsType<WuQuantizer>(encoder.Quantizer);
        Assert.Equal(32, quantizer.Options.MaxColors);
        Assert.Null(quantizer.Options.Dither);
    }

    [Fact]
    public void AddFrame_KeepFramesWritesOriginalPngBytes()
    {
        var directory = Directory.CreateTempSubdirectory("cmg-gif-frames-");
        var png = Png(Color.HotPink);
        using var sink = new GifFrameSink(encoding: new GifEncodingOptions(KeepFramesDirectory: directory.FullName));

        sink.AddFrame(png, 10);

        Assert.Equal(png, File.ReadAllBytes(Path.Combine(directory.FullName, "frame-0001.png")));
        directory.Delete(recursive: true);
    }

    [Fact]
    public void AddFrame_AppliesScaleAndMaximumDimensionsBeforeEncodingAndRetention()
    {
        var directory = Directory.CreateTempSubdirectory("cmg-scaled-frames-");
        var gif = Path.Combine(directory.FullName, "scaled.gif");
        var frames = Path.Combine(directory.FullName, "frames");
        using var sink = new GifFrameSink(
            encoding: new GifEncodingOptions(KeepFramesDirectory: frames),
            framing: new GifFramingOptions(Scale: 0.75, MaxWidth: 40, MaxHeight: 100));
        sink.AddFrame(Png(Color.CornflowerBlue, 100, 80), 10);

        sink.Save(gif);

        Assert.Equal(40, sink.Width);
        Assert.Equal(32, sink.Height);
        using var retained = Image.Load<Rgba32>(Path.Combine(frames, "frame-0001.png"));
        Assert.Equal(40, retained.Width);
        Assert.Equal(32, retained.Height);
        directory.Delete(recursive: true);
    }

    [Fact]
    public void AddFrame_CoalescesExactDuplicatesAndPreservesDuration()
    {
        using var sink = new GifFrameSink();
        var png = Png(Color.White);

        sink.AddFrame(png, 10);
        sink.AddFrame(png, 20);
        sink.AddFrame(png, 30);

        Assert.Equal(3, sink.SourceFrameCount);
        Assert.Equal(1, sink.FrameCount);
        Assert.Equal(2, sink.DuplicateFramesCoalesced);
        Assert.Equal(600, sink.DurationMilliseconds);
        Assert.Equal(1, sink.BlankFrameCount);
    }

    [Fact]
    public void AddFrame_CoalescingCanBeDisabled()
    {
        var optimization = new GifCaptureOptimizationOptions(CoalesceDuplicates: false);
        using var sink = new GifFrameSink(encoding: new GifEncodingOptions(CaptureOptimization: optimization));
        var png = Png(Color.White);

        sink.AddFrame(png, 10);
        sink.AddFrame(png, 20);

        Assert.Equal(2, sink.FrameCount);
        Assert.Equal(0, sink.DuplicateFramesCoalesced);
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

    [Fact]
    public void WriteTimeline_CapturesGifMetadata()
    {
        var directory = Directory.CreateTempSubdirectory("cmg-gif-timeline-");
        var gifPath = Path.Combine(directory.FullName, "flow.gif");
        var timelinePath = Path.Combine(directory.FullName, "flow.timeline.json");
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.White), 10);
        sink.AddFrame(Png(Color.Black), 25);
        sink.Save(gifPath);

        var written = GifTimelineWriter.Write(
            timelinePath,
            gifPath,
            new ScriptRecordingOptions(gifPath, HoldOnFailureMilliseconds: 1500,
                Encoding: new GifEncodingOptions(GifDitherMode.None, GifPaletteMode.Local, 32, directory.FullName),
                Framing: new GifFramingOptions("#panel", 12, 0.5, 640, 480)),
            sink,
            [new GifTimelineCheckpoint("after click", 7, 1, 100)],
            [new GifTimelineStep(3, 7, "click", "step checkout", false, 0, 1, 0, 350, 1, "failed")]);

        using var json = JsonDocument.Parse(File.ReadAllText(written));
        var root = json.RootElement;
        Assert.Equal(2, root.GetProperty("version").GetInt32());
        Assert.Equal(2, root.GetProperty("frameCount").GetInt32());
        Assert.Equal(2, root.GetProperty("captureDiagnostics").GetProperty("sourceFrameCount").GetInt32());
        Assert.Equal(350, root.GetProperty("durationMilliseconds").GetInt32());
        Assert.Equal(1500, root.GetProperty("timing").GetProperty("holdOnFailureMilliseconds").GetInt32());
        var encoding = root.GetProperty("encoding");
        Assert.Equal("none", encoding.GetProperty("dither").GetString());
        Assert.Equal("local", encoding.GetProperty("palette").GetString());
        Assert.Equal(32, encoding.GetProperty("colors").GetInt32());
        Assert.Equal(directory.FullName, encoding.GetProperty("keepFramesDirectory").GetString());
        var framing = root.GetProperty("framing");
        Assert.Equal("#panel", framing.GetProperty("crop").GetString());
        Assert.Equal(12, framing.GetProperty("cropPadding").GetInt32());
        Assert.Equal(0.5, framing.GetProperty("scale").GetDouble());
        Assert.Equal(640, framing.GetProperty("maxWidth").GetInt32());
        Assert.Equal(480, framing.GetProperty("maxHeight").GetInt32());
        var pointerEvidence = root.GetProperty("pointerEvidence");
        Assert.Equal("auto", pointerEvidence.GetProperty("contrast").GetString());
        Assert.Equal("auto", pointerEvidence.GetProperty("targetCallout").GetString());
        Assert.Equal(1200, pointerEvidence.GetProperty("idleThresholdMilliseconds").GetInt32());
        Assert.True(pointerEvidence.GetProperty("teleportMarker").GetBoolean());
        Assert.Equal([100, 250], root.GetProperty("frameDelaysMilliseconds").EnumerateArray().Select(value => value.GetInt32()).ToArray());
        var checkpoint = Assert.Single(root.GetProperty("checkpoints").EnumerateArray());
        Assert.Equal("after click", checkpoint.GetProperty("name").GetString());
        Assert.Equal(7, checkpoint.GetProperty("lineNumber").GetInt32());
        Assert.Equal(1, checkpoint.GetProperty("frameIndex").GetInt32());
        Assert.Equal(100, checkpoint.GetProperty("timeMilliseconds").GetInt32());
        var step = Assert.Single(root.GetProperty("steps").EnumerateArray());
        Assert.Equal(3, step.GetProperty("sequence").GetInt32());
        Assert.Equal("step checkout", step.GetProperty("context").GetString());
        Assert.Equal(1, step.GetProperty("failureFrameIndex").GetInt32());
        directory.Delete(recursive: true);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("off")]
    [InlineData("none")]
    public void ResolveTimeline_DisabledValuesSkip(string value)
    {
        Assert.Null(GifTimelinePath.Resolve("flow.gif", value));
    }

    private static byte[] Png(Color color, int width = 8, int height = 8)
    {
        using var image = new Image<Rgba32>(width, height, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
