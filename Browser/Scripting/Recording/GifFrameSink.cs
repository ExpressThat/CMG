using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace CMG.Browser.Scripting.Recording;

public sealed class GifFrameSink : IDisposable
{
    private readonly List<Image<Rgba32>> frames = [];
    private readonly GifQuality quality;

    public GifFrameSink(GifQuality quality = GifQuality.Highest)
    {
        this.quality = quality;
    }

    public void AddFrame(byte[] pngBytes, int delayCentiseconds)
    {
        var image = Image.Load<Rgba32>(pngBytes);
        image.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = delayCentiseconds;
        frames.Add(image);
    }

    public void Save(string path)
    {
        if (frames.Count is 0)
        {
            return;
        }

        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var width = frames.Max(frame => frame.Width);
        var height = frames.Max(frame => frame.Height);
        using var gif = NormalizeFrame(frames[0], width, height);
        gif.Metadata.GetGifMetadata().RepeatCount = 0;

        for (var index = 1; index < frames.Count; index++)
        {
            using var normalized = NormalizeFrame(frames[index], width, height);
            gif.Frames.AddFrame(normalized.Frames.RootFrame);
        }

        gif.SaveAsGif(fullPath, CreateEncoder(quality));
    }

    internal static GifEncoder CreateEncoder(GifQuality quality = GifQuality.Highest) => new()
    {
        ColorTableMode = quality is GifQuality.Low ? GifColorTableMode.Local : GifColorTableMode.Global,
        Quantizer = CreateQuantizer(quality)
    };

    private static IQuantizer CreateQuantizer(GifQuality quality) =>
        quality switch
        {
            GifQuality.Low => new OctreeQuantizer(new QuantizerOptions
            {
                ColorMatchingMode = ColorMatchingMode.Coarse,
                Dither = null,
                MaxColors = 64
            }),
            GifQuality.Medium => new WuQuantizer(new QuantizerOptions
            {
                ColorMatchingMode = ColorMatchingMode.Hybrid,
                Dither = KnownDitherings.Bayer4x4,
                DitherScale = 0.35f,
                MaxColors = 128
            }),
            GifQuality.High => new WuQuantizer(new QuantizerOptions
            {
                ColorMatchingMode = ColorMatchingMode.Exact,
                Dither = KnownDitherings.FloydSteinberg,
                DitherScale = 0.5f,
                MaxColors = 256
            }),
            _ => new WuQuantizer(new QuantizerOptions
            {
                ColorMatchingMode = ColorMatchingMode.Exact,
                Dither = KnownDitherings.FloydSteinberg,
                DitherScale = 0.75f,
                MaxColors = 256
            })
        };

    private static Image<Rgba32> NormalizeFrame(Image<Rgba32> frame, int width, int height)
    {
        if (frame.Width == width && frame.Height == height)
        {
            return frame.Clone();
        }

        var normalized = new Image<Rgba32>(width, height, Color.White);
        normalized.Mutate(context => context.DrawImage(frame, new Point(0, 0), 1f));
        normalized.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay =
            frame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay;

        return normalized;
    }

    public void Dispose()
    {
        foreach (var frame in frames)
        {
            frame.Dispose();
        }
    }
}
