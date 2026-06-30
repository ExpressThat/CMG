using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Browser.Scripting.Recording;

public sealed record GifOptimizeResult(
    string InputPath,
    string OutputPath,
    int FramesBefore,
    int FramesAfter,
    int DuplicateFramesRemoved,
    int DurationMilliseconds,
    long SizeBeforeBytes,
    long SizeAfterBytes)
{
    public string Format() =>
        $"GIF_OPTIMIZE input={Quote(InputPath)} output={Quote(OutputPath)} " +
        $"framesBefore={FramesBefore} framesAfter={FramesAfter} duplicateFramesRemoved={DuplicateFramesRemoved} " +
        $"durationMs={DurationMilliseconds} sizeBeforeBytes={SizeBeforeBytes} sizeAfterBytes={SizeAfterBytes}";

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

public static class GifOptimizer
{
    public static GifOptimizeResult RemoveDuplicateFrames(FileInfo input, FileInfo output)
    {
        var format = Image.DetectFormat(input.FullName);
        if (!string.Equals(format.Name, "GIF", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Expected a GIF image, got {format.Name}.");
        }

        using var gif = Image.Load<Rgba32>(input.FullName);
        using var optimized = BuildOptimizedGif(gif);
        output.Directory?.Create();
        optimized.SaveAsGif(output.FullName, GifFrameSink.CreateEncoder());

        var before = GifInspector.Inspect(input);
        var after = GifInspector.Inspect(output);
        return new(
            input.FullName,
            output.FullName,
            before.FrameCount,
            after.FrameCount,
            before.FrameCount - after.FrameCount,
            after.DurationMilliseconds,
            before.SizeBytes,
            after.SizeBytes);
    }

    private static Image<Rgba32> BuildOptimizedGif(Image<Rgba32> gif)
    {
        var kept = new List<Image<Rgba32>>();
        var previous = Array.Empty<Rgba32>();
        try
        {
            for (var frameIndex = 0; frameIndex < gif.Frames.Count; frameIndex++)
            {
                var frame = gif.Frames[frameIndex];
                using var image = gif.Frames.CloneFrame(frameIndex);
                var pixels = PixelData(image);
                var delay = frame.Metadata.GetGifMetadata().FrameDelay;
                if (kept.Count > 0 && PixelsEqual(previous, pixels))
                {
                    AddDelay(kept[^1], delay);
                    continue;
                }

                previous = pixels;
                kept.Add(CloneWithDelay(image, delay));
            }

            return BuildGif(kept, gif.Metadata.GetGifMetadata().RepeatCount);
        }
        finally
        {
            foreach (var image in kept)
            {
                image.Dispose();
            }
        }
    }

    private static Image<Rgba32> BuildGif(IReadOnlyList<Image<Rgba32>> frames, ushort repeatCount)
    {
        var output = frames[0].Clone();
        output.Metadata.GetGifMetadata().RepeatCount = repeatCount;
        for (var index = 1; index < frames.Count; index++)
        {
            output.Frames.AddFrame(frames[index].Frames.RootFrame);
        }

        return output;
    }

    private static Image<Rgba32> CloneWithDelay(Image<Rgba32> image, int delay)
    {
        var clone = image.Clone();
        clone.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = delay;
        return clone;
    }

    private static void AddDelay(Image<Rgba32> image, int delay)
    {
        var metadata = image.Frames.RootFrame.Metadata.GetGifMetadata();
        metadata.FrameDelay += delay;
    }

    private static Rgba32[] PixelData(Image<Rgba32> image)
    {
        var pixels = new Rgba32[image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        return pixels;
    }

    private static bool PixelsEqual(Rgba32[] left, Rgba32[] right) =>
        left.AsSpan().SequenceEqual(right);
}
