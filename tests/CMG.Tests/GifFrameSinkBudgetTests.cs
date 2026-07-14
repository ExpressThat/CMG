using System.Text.Json;
using CMG.Browser.Scripting.Recording;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class GifFrameSinkBudgetTests
{
    [Fact]
    public void Save_UsesQualityAndDownscaleFallbackForSizeBudget()
    {
        var directory = Directory.CreateTempSubdirectory();
        var baseline = Path.Combine(directory.FullName, "baseline.gif");
        var budgeted = Path.Combine(directory.FullName, "budgeted.gif");
        var png = NoisePng(360, 240);
        try
        {
            using (var sink = new GifFrameSink(GifQuality.Highest))
            {
                sink.AddFrame(png, 10);
                sink.Save(baseline);
            }
            var target = Math.Max(1024, new FileInfo(baseline).Length / 4);
            using var constrained = new GifFrameSink(GifQuality.Highest,
                new GifEncodingOptions(SizeBudget: new GifSizeBudgetOptions(target)));
            constrained.AddFrame(png, 10);
            constrained.Save(budgeted);

            Assert.True(constrained.BudgetApplied);
            Assert.True(constrained.BudgetAttempts > 1);
            Assert.True(new FileInfo(budgeted).Length < new FileInfo(baseline).Length);
            Assert.True(constrained.FinalBudgetScale < 1 || constrained.FinalBudgetQuality != GifQuality.Highest);
            Assert.Equal(new FileInfo(budgeted).Length, constrained.FinalSizeBytes);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Timeline_ReportsBudgetAndPerStepCaptureCost()
    {
        var directory = Directory.CreateTempSubdirectory();
        var gif = Path.Combine(directory.FullName, "flow.gif");
        var timeline = Path.Combine(directory.FullName, "flow.timeline.json");
        try
        {
            var encoding = new GifEncodingOptions(SizeBudget: new GifSizeBudgetOptions(1_000_000));
            using var sink = new GifFrameSink(encoding: encoding);
            sink.AddFrame(NoisePng(40, 30), 10);
            sink.Save(gif);
            var options = new ScriptRecordingOptions(gif, Encoding: encoding);
            GifTimelineWriter.Write(timeline, gif, options, sink, steps:
                [
                    new GifTimelineStep(1, 3, "click", "", true, 0, 0, 0, 100, null, null),
                    new GifTimelineStep(2, 4, "evaluate", "", true, 1, null, 100, 100, null, null)
                ]);

            using var document = JsonDocument.Parse(File.ReadAllText(timeline));
            var root = document.RootElement;
            Assert.Equal(1_000_000, root.GetProperty("captureDiagnostics").GetProperty("sizeBudgetBytes").GetInt64());
            var step = root.GetProperty("steps")[0];
            Assert.Equal(1, step.GetProperty("capturedFrameCount").GetInt32());
            Assert.Equal(100, step.GetProperty("capturedDurationMilliseconds").GetInt32());
            Assert.Equal(40 * 30 * 4, step.GetProperty("estimatedRgbaBytes").GetInt64());
            var nonVisual = root.GetProperty("steps")[1];
            Assert.Equal(0, nonVisual.GetProperty("capturedFrameCount").GetInt32());
            Assert.Equal(0, nonVisual.GetProperty("estimatedRgbaBytes").GetInt64());
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Save_NormalizesSmallerFramesBeforeBudgetEncoding()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        try
        {
            using (var sink = new GifFrameSink())
            {
                sink.AddFrame(SolidPng(20, 20, Color.Red), 10);
                sink.AddFrame(SolidPng(10, 10, Color.Blue), 10);
                sink.Save(path);
            }
            using var gif = Image.Load<Rgba32>(path);
            Assert.Equal(new Rgba32(0, 0, 255), gif.Frames[1][5, 5]);
            Assert.Equal(new Rgba32(255, 255, 255), gif.Frames[1][15, 15]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static byte[] NoisePng(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        var random = new Random(42);
        image.ProcessPixelRows(rows =>
        {
            for (var y = 0; y < rows.Height; y++)
            {
                var row = rows.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++) row[x] = new Rgba32((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            }
        });
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private static byte[] SolidPng(int width, int height, Color color)
    {
        using var image = new Image<Rgba32>(width, height, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
