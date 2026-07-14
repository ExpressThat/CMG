using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
        using var canvas = new Image<Rgba32>(gif.Width, gif.Height, Color.Transparent);
        try
        {
            for (var frameIndex = 0; frameIndex < gif.Frames.Count; frameIndex++)
            {
                var frame = gif.Frames[frameIndex];
                using var image = gif.Frames.CloneFrame(frameIndex);
                var metadata = frame.Metadata.GetGifMetadata();
                using var priorCanvas = canvas.Clone();
                canvas.Mutate(context => context.DrawImage(image, 1f));
                var pixels = PixelData(canvas);
                var delay = metadata.FrameDelay;
                if (kept.Count > 0 && PixelsEqual(previous, pixels))
                {
                    AddDelay(kept[^1], delay);
                }
                else
                {
                    previous = pixels;
                    kept.Add(CloneWithDelay(canvas, delay));
                }
                ApplyDisposal(canvas, priorCanvas, metadata.DisposalMethod);
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
        var metadata = clone.Frames.RootFrame.Metadata.GetGifMetadata();
        metadata.FrameDelay = delay;
        metadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;
        return clone;
    }

    private static void ApplyDisposal(
        Image<Rgba32> canvas,
        Image<Rgba32> priorCanvas,
        GifDisposalMethod disposal)
    {
        if (disposal == GifDisposalMethod.RestoreToPrevious)
        {
            canvas.Mutate(context => context.DrawImage(priorCanvas, PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.Src, 1f));
        }
        else if (disposal == GifDisposalMethod.RestoreToBackground)
        {
            canvas.ProcessPixelRows(accessor =>
            {
                for (var row = 0; row < accessor.Height; row++) accessor.GetRowSpan(row).Clear();
            });
        }
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
