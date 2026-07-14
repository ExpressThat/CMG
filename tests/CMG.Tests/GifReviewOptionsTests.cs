using System.Text.Json;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class GifReviewOptionsTests
{
    [Fact]
    public void ReviewMetadata_FlowsIntoNarrationJsonAndHtmlReports()
    {
        var directory = Directory.CreateTempSubdirectory();
        var gif = Path.Combine(directory.FullName, "checkout.gif");
        var timeline = Path.ChangeExtension(gif, ".timeline.json");
        var narration = Path.ChangeExtension(gif, ".narration.txt");
        try
        {
            var review = new GifReviewOptions("auto", "{name}: {steps} steps, {duration}ms, {outcome}", "Checkout release evidence");
            var encoding = new GifEncodingOptions(Review: review);
            using var sink = new GifFrameSink(encoding: encoding);
            sink.AddFrame(Png(), 10);
            sink.Save(gif);
            var steps = new[] { new GifTimelineStep(1, 3, "click", "checkout", true, 0, 0, 0, 100, null, null) };
            GifNarrationWriter.Write(narration, gif, review, sink, steps);
            GifTimelineWriter.Write(timeline, gif, new ScriptRecordingOptions(gif, Encoding: encoding), sink, steps: steps);
            var test = new CmgTestResult("checkout", "flow.cmgscript", true, [], null, gif, []);

            using var json = JsonDocument.Parse(CmgJsonReportWriter.Write([test]));
            var metadata = json.RootElement[0].GetProperty("gifMetadata")[0];
            Assert.Equal("Checkout release evidence", metadata.GetProperty("description").GetString());
            Assert.Equal(Path.GetFullPath(narration), metadata.GetProperty("narrationPath").GetString());
            Assert.Contains("1 steps, 100ms, passed", metadata.GetProperty("altText").GetString(), StringComparison.Ordinal);
            var html = CmgHtmlReportWriter.Write([test]);
            Assert.Contains("alt=\"checkout: 1 steps, 100ms, passed\"", html, StringComparison.Ordinal);
            Assert.Contains("Screen-reader narration", html, StringComparison.Ordinal);
            var junit = CmgJUnitReportWriter.Write([test]);
            Assert.Contains("name=\"cmg.gif.narrationPath\"", junit, StringComparison.Ordinal);
            Assert.Contains("name=\"cmg.gif.altText\"", junit, StringComparison.Ordinal);
            Assert.Contains("name=\"cmg.gif.description\"", junit, StringComparison.Ordinal);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void InvalidNarrationValueExplainsAcceptedForms()
    {
        var exception = Assert.Throws<CMG.Browser.Scripting.ScriptExecutionException>(() =>
            GifReviewOptions.FromOptions(new Dictionary<string, string> { ["narrationSidecar"] = " " }, "gif"));

        Assert.Contains("true, false, or a file path", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AltText_RejectsUnknownPlaceholdersAndUsesExplicitOutcome()
    {
        var exception = Assert.Throws<CMG.Browser.Scripting.ScriptExecutionException>(() =>
            GifReviewOptions.FromOptions(new Dictionary<string, string> { ["altText"] = "{unknown}" }, "gif"));
        Assert.Contains("Supported placeholders", exception.Message, StringComparison.Ordinal);

        using var sink = new GifFrameSink();
        sink.AddFrame(Png(), 10);
        var review = new GifReviewOptions(AltText: "{outcome}");
        Assert.Equal("skipped", review.RenderAltText("flow.gif", sink, [], GifRecordingOutcome.Skipped));
    }

    private static byte[] Png()
    {
        using var image = new Image<Rgba32>(8, 8, Color.White);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
