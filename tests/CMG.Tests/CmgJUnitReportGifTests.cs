using CMG.Browser.Scripting.Recording;
using CMG.Runner;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class CmgJUnitReportGifTests
{
    [Fact]
    public void JUnitReport_IncludesGifPathProperty()
    {
        var report = CmgJUnitReportWriter.Write([TestWithGif("artifacts\\flow.gif", success: true)]);

        Assert.Contains("name=\"cmg.gif.path\"", report);
        Assert.Contains("value=\"artifacts\\flow.gif\"", report);
    }

    [Fact]
    public void JUnitReport_IncludesFailureFrameIndexForFailedGif()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.Red), 10);
        sink.AddFrame(Png(Color.Blue), 10);
        sink.Save(path);
        try
        {
            var report = CmgJUnitReportWriter.Write([TestWithGif(path, success: false)]);

            Assert.Contains("name=\"cmg.gif.path\"", report);
            Assert.Contains("name=\"cmg.gif.failureFrameIndex\" value=\"1\"", report);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void JUnitReport_NumbersMultipleGifProperties()
    {
        var report = CmgJUnitReportWriter.Write([TestWithGif("first.gif;second.gif", success: true)]);

        Assert.Contains("name=\"cmg.gif.path.1\" value=\"first.gif\"", report);
        Assert.Contains("name=\"cmg.gif.path.2\" value=\"second.gif\"", report);
    }

    private static CmgTestResult TestWithGif(string path, bool success) =>
        new("checkout", "checkout.cmgscript", success, [], success ? null : "failed", path, []);

    private static byte[] Png(Color color)
    {
        using var image = new Image<Rgba32>(8, 8, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
