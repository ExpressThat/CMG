using CMG.Browser.Scripting.Recording;
using CMG.Runner;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class CmgReportGifMetadataTests
{
    [Fact]
    public void JsonReport_IncludesGifMetadata()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var sink = new GifFrameSink();
        sink.AddFrame(Png(Color.White), 10);
        sink.AddFrame(Png(Color.Black), 25);
        sink.Save(path);

        try
        {
            var report = CmgJsonReportWriter.Write([Test(path)]);

            using var document = JsonDocument.Parse(report);
            var metadata = document.RootElement[0].GetProperty("gifMetadata")[0];
            Assert.Equal(path, metadata.GetProperty("path").GetString());
            Assert.Equal("gif", metadata.GetProperty("format").GetString());
            Assert.Equal("medium", metadata.GetProperty("quality").GetString());
            Assert.Equal(2, metadata.GetProperty("frames").GetInt32());
            Assert.Equal(350, metadata.GetProperty("durationMs").GetInt32());
            Assert.Equal(8, metadata.GetProperty("width").GetInt32());
            Assert.Equal(8, metadata.GetProperty("height").GetInt32());
            Assert.True(metadata.GetProperty("sizeBytes").GetInt64() > 0);
            Assert.Equal("2", metadata.GetProperty("paletteColors").GetString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void JsonReport_ReadsMp4MetadataFromMandatoryTimeline()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mp4");
        try
        {
            File.WriteAllBytes(path, [0, 1, 2]);
            File.WriteAllText(GifArtifactPaths.Timeline(path),
                """{"frameCount":12,"durationMilliseconds":2300,"width":762,"height":484,"fileSizeBytes":3}""");

            using var document = JsonDocument.Parse(CmgJsonReportWriter.Write([Test(path)]));
            var metadata = document.RootElement[0].GetProperty("gifMetadata")[0];
            Assert.Equal("mp4", metadata.GetProperty("format").GetString());
            Assert.Equal(12, metadata.GetProperty("frames").GetInt32());
            Assert.Equal(2300, metadata.GetProperty("durationMs").GetInt32());
            Assert.False(metadata.TryGetProperty("error", out _));
        }
        finally
        {
            File.Delete(path);
            File.Delete(GifArtifactPaths.Timeline(path));
        }
    }

    private static CmgTestResult Test(string path) =>
        new("checkout", "checkout.cmgscript", true, [], null, path, [])
        {
            GifQualities = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [path] = "medium"
            }
        };

    private static byte[] Png(Color color)
    {
        using var image = new Image<Rgba32>(8, 8, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
