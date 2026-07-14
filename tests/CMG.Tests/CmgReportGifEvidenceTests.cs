using System.Text.Json;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class CmgReportGifEvidenceTests
{
    [Fact]
    public void JsonReport_ExposesStepFrameMapping()
    {
        var test = TestWithEvidence("flow.gif", "flow.timeline.json", success: true);

        using var document = JsonDocument.Parse(CmgJsonReportWriter.Write([test]));

        var evidence = Assert.Single(document.RootElement[0].GetProperty("steps")[0].GetProperty("gifEvidence").EnumerateArray());
        Assert.Equal(2, evidence.GetProperty("startFrameIndex").GetInt32());
        Assert.Equal(5, evidence.GetProperty("endFrameIndex").GetInt32());
        Assert.Equal(300, evidence.GetProperty("startTimeMilliseconds").GetInt32());
        Assert.Equal(4, evidence.GetProperty("capturedFrameCount").GetInt32());
        Assert.Equal(600, evidence.GetProperty("capturedDurationMilliseconds").GetInt32());
        Assert.Equal(4096, evidence.GetProperty("estimatedRgbaBytes").GetInt64());
    }

    [Fact]
    public void HtmlReport_EmbedsStartAndFailureFrameEvidence()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        WriteGif(path);
        try
        {
            var report = CmgHtmlReportWriter.Write([TestWithEvidence(path, "flow.timeline.json", success: false)]);

            Assert.Contains("href=\"#evidence-0-1-0\"", report, StringComparison.Ordinal);
            Assert.Contains("id=\"failure-0\"", report, StringComparison.Ordinal);
            Assert.Contains("Failure frame 1", report, StringComparison.Ordinal);
            Assert.Contains("data:image/png;base64,", report, StringComparison.Ordinal);
            Assert.Contains("Step Evidence", report, StringComparison.Ordinal);
            Assert.Contains("cost 4 frame(s), 600ms, 4096 retained RGBA bytes", report, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static CmgTestResult TestWithEvidence(string gifPath, string timelinePath, bool success)
    {
        var evidence = new CmgStepGifEvidence(gifPath, timelinePath, 2, 5, 300, 900, success ? null : 1, 4, 600, 4096);
        var step = new CmgStepResult(7, "click", success, [], success ? null : "failed", gifPath, 4, "step checkout", "click")
        {
            GifEvidence = [evidence]
        };
        return new CmgTestResult("checkout", "flow.cmgscript", success, [], step.Error, gifPath, [step]);
    }

    private static void WriteGif(string path)
    {
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.White), 10);
        sink.AddFrame(Png(Color.Red), 10);
        sink.AddFrame(Png(Color.Black), 10);
        sink.Save(path);
    }

    private static byte[] Png(Color color)
    {
        using var image = new Image<Rgba32>(8, 8, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
