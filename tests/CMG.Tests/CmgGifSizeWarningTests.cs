using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class CmgGifSizeWarningTests
{
    [Fact]
    public void GifSizeWarnings_EmitsWarningsForOversizedArtifacts()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        File.WriteAllBytes(path, new byte[4]);
        try
        {
            var test = new CmgTestResult("checkout", "flow.cmgscript", true, [], null, path, []);
            var warnings = CmgRunService.GifSizeWarnings(test, Options(threshold: 3));

            var warning = Assert.Single(warnings);
            Assert.Contains("GIF_WARN_SIZE", warning, StringComparison.Ordinal);
            Assert.Contains("test=\"checkout\"", warning, StringComparison.Ordinal);
            Assert.Contains("sizeBytes=4", warning, StringComparison.Ordinal);
            Assert.Contains("thresholdBytes=3", warning, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void GifSizeWarnings_SkipsWhenNoThreshold()
    {
        var test = new CmgTestResult("checkout", "flow.cmgscript", true, [], null, "missing.gif", []);

        Assert.Empty(CmgRunService.GifSizeWarnings(test, Options(threshold: null)));
    }

    [Fact]
    public void GifPaletteWarnings_EmitsWarningsForHighColorArtifacts()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var sink = new GifFrameSink();
        sink.AddFrame(GradientPng(), 10);
        sink.Save(path);
        try
        {
            var test = new CmgTestResult("checkout", "flow.cmgscript", true, [], null, path, []);
            var warnings = CmgRunService.GifPaletteWarnings(test);

            var warning = Assert.Single(warnings);
            Assert.Contains("GIF_WARN_PALETTE", warning, StringComparison.Ordinal);
            Assert.Contains("test=\"checkout\"", warning, StringComparison.Ordinal);
            Assert.Contains("thresholdColors=240", warning, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void GifPaletteWarnings_SkipsLowColorArtifacts()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var sink = new GifFrameSink();
        sink.AddFrame(SolidPng(Color.Red), 10);
        sink.Save(path);
        try
        {
            var test = new CmgTestResult("checkout", "flow.cmgscript", true, [], null, path, []);

            Assert.Empty(CmgRunService.GifPaletteWarnings(test));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void GifDurationGuard_FailsResultWhenArtifactExceedsThreshold()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var sink = new GifFrameSink();
        sink.AddFrame(SolidPng(Color.Red), 100);
        sink.AddFrame(SolidPng(Color.Blue), 100);
        sink.Save(path);
        try
        {
            var test = new CmgTestResult("checkout", "flow.cmgscript", true, [], null, path, []);
            var (result, output) = CmgRunService.ApplyGifDurationGuard(test, Options(threshold: null, maxDuration: 1500));

            Assert.False(result.Success);
            Assert.Contains("exceeded max 1500ms", result.Error);
            var line = Assert.Single(output);
            Assert.Contains("GIF_MAX_DURATION", line, StringComparison.Ordinal);
            Assert.Contains("durationMs=2000", line, StringComparison.Ordinal);
            Assert.Contains("thresholdMs=1500", line, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static CmgRunOptions Options(long? threshold, int? maxDuration = null) =>
        new(
            BrowserKind.Chrome,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            0,
            0,
            1,
            false,
            1,
            1,
            null,
            null,
            null,
            null,
            new Dictionary<string, string>(),
            GifWarnSizeBytes: threshold,
            GifMaxDurationMilliseconds: maxDuration);

    private static byte[] GradientPng()
    {
        using var image = new Image<Rgba32>(32, 8);
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    row[x] = new Rgba32((byte)(x * 8), (byte)(y * 31), (byte)((x + y) * 5));
                }
            }
        });

        return Png(image);
    }

    private static byte[] SolidPng(Color color)
    {
        using var image = new Image<Rgba32>(8, 8, color);
        return Png(image);
    }

    private static byte[] Png(Image<Rgba32> image)
    {
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
