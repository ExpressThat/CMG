using System.Text;
using CMG.Browser.Scripting.Recording;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class GifStillPdfWriterTests
{
    [Fact]
    public void Write_ExportsDistinctStepResultsAndFinalState()
    {
        var directory = Directory.CreateTempSubdirectory();
        var path = Path.Combine(directory.FullName, "review.pdf");
        try
        {
            using var sink = new GifFrameSink();
            sink.AddFrame(Png(Color.Red), 10, false);
            sink.AddFrame(Png(Color.Green), 10, false);
            sink.AddFrame(Png(Color.Blue), 10, false);
            var steps = new[]
            {
                new GifTimelineStep(1, 1, "click", "", true, 0, 1, 0, 100, null, null)
            };

            GifStillPdfWriter.Write(path, "journey.gif", sink, steps);

            var bytes = File.ReadAllBytes(path);
            var text = Encoding.ASCII.GetString(bytes);
            Assert.StartsWith("%PDF-1.4", text, StringComparison.Ordinal);
            Assert.Equal(2, Count(text, "/Type /Page "));
            Assert.Contains("/Title (journey.gif step review)", text, StringComparison.Ordinal);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void ReviewOption_ResolvesDefaultAndRejectsBlankPath()
    {
        var options = GifReviewOptions.FromOptions(
            new Dictionary<string, string> { ["stillPdf"] = "true" }, "gif");
        Assert.EndsWith("flow.steps.pdf", options.ResolveStillPdfPath("flow.gif"), StringComparison.OrdinalIgnoreCase);

        var exception = Assert.Throws<CMG.Browser.Scripting.ScriptExecutionException>(() =>
            GifReviewOptions.FromOptions(new Dictionary<string, string> { ["stillPdf"] = " " }, "gif"));
        Assert.Contains("stillPdf= must be true, false, or a file path", exception.Message, StringComparison.Ordinal);
    }

    private static int Count(string value, string token) =>
        value.Split(token, StringSplitOptions.None).Length - 1;

    private static byte[] Png(Color color)
    {
        using var image = new Image<Rgba32>(12, 8, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
