using CMG.Browser.Scripting;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class RecordingArtifactFormatTests
{
    [Theory]
    [InlineData("gif", GifArtifactFormat.Gif, ".gif")]
    [InlineData("apng", GifArtifactFormat.Apng, ".apng")]
    [InlineData("webp", GifArtifactFormat.Webp, ".webp")]
    [InlineData("mp4", GifArtifactFormat.Mp4, ".mp4")]
    public void Format_ParsesAndResolvesExtension(string value, GifArtifactFormat expected, string extension)
    {
        var format = GifArtifactFormatParser.Parse(value, "gif");
        Assert.Equal(expected, format);
        Assert.Equal(extension, Path.GetExtension(GifArtifactFormatParser.WithExtension("flow.gif", format)));
    }

    [Fact]
    public void Format_RejectsMismatchedExplicitExtension()
    {
        var error = Assert.Throws<ScriptExecutionException>(() =>
            GifArtifactFormatParser.WithExtension("flow.webp", GifArtifactFormat.Apng));
        Assert.Contains("requires a '.apng' output path", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(GifArtifactFormat.Apng, ".apng")]
    [InlineData(GifArtifactFormat.Webp, ".webp")]
    public void Save_WritesAnimatedTrueColorArtifact(GifArtifactFormat format, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
        try
        {
            using var sink = new GifFrameSink(GifQuality.Highest, new GifEncodingOptions(Format: format));
            sink.AddFrame(Png(new Rgba32(240, 10, 10)), 12, coalesceDuplicates: false);
            sink.AddFrame(Png(new Rgba32(10, 20, 240)), 23, coalesceDuplicates: false);
            sink.Save(path);

            var inspection = GifInspector.InspectRecording(new FileInfo(path));
            Assert.Equal(2, inspection.FrameCount);
            Assert.InRange(inspection.DurationMilliseconds, 340, 360);
            Assert.Equal("truecolor", inspection.Palette);
            using var decoded = Image.Load<Rgba32>(path);
            Assert.Equal(new Rgba32(240, 10, 10, 255), decoded.Frames[0][0, 0]);
            Assert.Equal(new Rgba32(10, 20, 240, 255), decoded.Frames[1][0, 0]);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Save_Mp4WithoutFfmpeg_ExplainsEveryResolutionOption()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mp4");
        using var sink = new GifFrameSink(encoding: new GifEncodingOptions(
            Format: GifArtifactFormat.Mp4, FfmpegPath: Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.exe")));
        sink.AddFrame(Png(new Rgba32(40, 50, 60)), 10);

        var error = Assert.Throws<ScriptExecutionException>(() => sink.Save(path));
        Assert.Contains("requires FFmpeg", error.Message, StringComparison.Ordinal);
        Assert.Contains("CMG_FFMPEG", error.Message, StringComparison.Ordinal);
        Assert.Contains("ffmpeg=<path>", error.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void RunnerPath_UsesConfiguredFormatExtension()
    {
        var options = new CmgRunOptions(
            BrowserKind.Chrome, new DirectoryInfo(Path.GetTempPath()), null, null, null, null,
            null, null, 0, 0, 1, false, 1, 1, null, null, null, null,
            new Dictionary<string, string>(),
            GifEncoding: new GifEncodingOptions(Format: GifArtifactFormat.Webp));
        var test = new CmgTestCase("flow.cmgscript", "checkout", [], new Dictionary<string, string>());

        var path = CmgRunService.BuildGifPath(test, options);

        Assert.EndsWith("chrome-checkout.webp", path!.FullName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sidecars_DoNotCollideAcrossFormatsWithSameBaseName()
    {
        Assert.EndsWith("flow.timeline.json", GifArtifactPaths.Timeline("flow.gif"), StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("flow.apng.timeline.json", GifArtifactPaths.Timeline("flow.apng"), StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("flow.webp.timeline.json", GifArtifactPaths.Timeline("flow.webp"), StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(GifArtifactPaths.Frames("flow.apng"), GifArtifactPaths.Frames("flow.webp"));
        Assert.NotEqual(GifArtifactPaths.Narration("flow.apng"), GifArtifactPaths.Narration("flow.webp"));
    }

    [Fact]
    public void ApngBudget_DoesNotRetryMeaninglessGifQualityFallbacks()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.apng");
        try
        {
            using var sink = new GifFrameSink(encoding: new GifEncodingOptions(
                Format: GifArtifactFormat.Apng,
                SizeBudget: new GifSizeBudgetOptions(1, QualityFallback: true, DownscaleFallback: false)));
            sink.AddFrame(Png(new Rgba32(20, 40, 60)), 10);
            sink.Save(path);
            Assert.Equal(1, sink.BudgetAttempts);
            Assert.False(sink.BudgetMet);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static byte[] Png(Rgba32 color)
    {
        using var image = new Image<Rgba32>(3, 2, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
