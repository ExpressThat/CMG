using CMG.Browser.Scripting.Recording;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class GifTrimmerTests
{
    [Fact]
    public void Trim_FrameRangeKeepsInclusiveFramesAndTiming()
    {
        using var fixture = new TrimFixture();

        var result = GifTrimmer.Trim(fixture.Input, fixture.Output, 1, 2, null, null);

        Assert.Equal(2, result.FramesAfter);
        Assert.Equal(500, result.DurationAfterMilliseconds);
        Assert.Equal(2, GifInspector.Inspect(fixture.Output).FrameCount);
    }

    [Fact]
    public void Trim_TimeRangeAdjustsBoundaryFrameDelays()
    {
        using var fixture = new TrimFixture();

        var result = GifTrimmer.Trim(fixture.Input, fixture.Output, null, null, 50, 450);

        Assert.Equal(3, result.FramesAfter);
        Assert.Equal(400, result.DurationAfterMilliseconds);
    }

    [Fact]
    public void Trim_RejectsMixedFrameAndTimeModes()
    {
        using var fixture = new TrimFixture();

        var error = Assert.Throws<ArgumentException>(() => GifTrimmer.Trim(fixture.Input, fixture.Output, 0, null, 0, 100));

        Assert.Contains("either frame options or time options", error.Message, StringComparison.Ordinal);
    }

    private sealed class TrimFixture : IDisposable
    {
        public FileInfo Input { get; } = new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-input.gif"));
        public FileInfo Output { get; } = new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-output.gif"));

        public TrimFixture()
        {
            using var sink = new GifFrameSink();
            sink.AddFrame(Png(Color.Red), 10);
            sink.AddFrame(Png(Color.Green), 20);
            sink.AddFrame(Png(Color.Blue), 30);
            sink.Save(Input.FullName);
        }

        public void Dispose() { if (Input.Exists) Input.Delete(); if (Output.Exists) Output.Delete(); }

        private static byte[] Png(Color color)
        {
            using var image = new Image<Rgba32>(8, 8, color);
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            return stream.ToArray();
        }
    }
}
