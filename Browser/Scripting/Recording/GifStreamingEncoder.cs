using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class GifFrameSink
{
    private void EncodeStreamingGif(string path, GifQuality selectedQuality, double scale)
    {
        var directory = Path.Combine(spoolDirectory, $"quantized-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var canvasWidth = frames.Max(frame => frame.CanvasWidth);
            var canvasHeight = frames.Max(frame => frame.CanvasHeight);
            var paths = new string[frames.Count];
            Parallel.For(0, frames.Count, new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Math.Min(Environment.ProcessorCount, 8))
            }, index =>
            {
                paths[index] = QuantizeStoredFrame(
                    frames[index], selectedQuality, scale, directory, index, canvasWidth, canvasHeight);
                Interlocked.Increment(ref parallelPreprocessedFrameCount);
            });
            ParallelPreprocessedFrameCount = parallelPreprocessedFrameCount;
            GifStreamingWriter.Write(path, paths);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private int parallelPreprocessedFrameCount;

    private string QuantizeStoredFrame(
        GifStoredFrame frame, GifQuality selectedQuality, double scale, string directory, int index,
        int sourceCanvasWidth, int sourceCanvasHeight)
    {
        using var patch = Image.Load<Rgba32>(frame.Path);
        var normalize = !frame.IsDelta &&
            (frame.CanvasWidth != sourceCanvasWidth || frame.CanvasHeight != sourceCanvasHeight);
        using var image = normalize
            ? NormalizedPatch(patch, sourceCanvasWidth, sourceCanvasHeight)
            : patch.Clone();
        var width = Math.Max(1, (int)Math.Round(image.Width * scale));
        var height = Math.Max(1, (int)Math.Round(image.Height * scale));
        if (width != image.Width || height != image.Height)
            image.Mutate(context => context.Resize(new ResizeOptions
            {
                Size = new Size(width, height), Mode = ResizeMode.Stretch, Sampler = KnownResamplers.Lanczos3
            }));
        image.Mutate(context => context.Quantize(CreateQuantizer(selectedQuality, encoding)));
        var path = Path.Combine(directory, $"frame-{index + 1:000000}.qf");
        GifQuantizedFrame.Write(path, image,
            normalize ? 0 : Math.Max(0, (int)Math.Round(frame.X * scale)),
            normalize ? 0 : Math.Max(0, (int)Math.Round(frame.Y * scale)),
            Math.Max(1, (int)Math.Round(sourceCanvasWidth * scale)),
            Math.Max(1, (int)Math.Round(sourceCanvasHeight * scale)),
            frame.Delay);
        return path;
    }

    private static Image<Rgba32> NormalizedPatch(Image<Rgba32> patch, int width, int height)
    {
        var canvas = new Image<Rgba32>(width, height, Color.White);
        canvas.Mutate(context => context.DrawImage(patch, Point.Empty, 1));
        return canvas;
    }
}
