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
    private readonly GifEncodingOptions encoding;

    public GifFrameSink(GifQuality quality = GifQuality.Highest, GifEncodingOptions? encoding = null)
    {
        this.quality = quality;
        this.encoding = encoding ?? new GifEncodingOptions();
    }

    public void AddFrame(byte[] pngBytes, int delayCentiseconds)
    {
        RetainSourceFrame(pngBytes, frames.Count);
        var image = Image.Load<Rgba32>(pngBytes);
        SetFrameMetadata(image.Frames.RootFrame.Metadata.GetGifMetadata(), delayCentiseconds);
        frames.Add(image);
    }

    public int FrameCount => frames.Count;

    public int Width => frames.Count is 0 ? 0 : frames.Max(frame => frame.Width);

    public int Height => frames.Count is 0 ? 0 : frames.Max(frame => frame.Height);

    public int DurationMilliseconds => FrameDelaysCentiseconds.Sum() * 10;

    public IReadOnlyList<int> FrameDelaysMilliseconds => FrameDelaysCentiseconds.Select(delay => delay * 10).ToArray();

    private IEnumerable<int> FrameDelaysCentiseconds =>
        frames.Select(frame => frame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay);

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

        gif.SaveAsGif(fullPath, CreateEncoder(quality, encoding));
    }

    internal static GifEncoder CreateEncoder(GifQuality quality = GifQuality.Highest, GifEncodingOptions? encoding = null)
    {
        encoding ??= new GifEncodingOptions();
        return new GifEncoder
        {
            ColorTableMode = ColorTableMode(quality, encoding.Palette),
            Quantizer = CreateQuantizer(quality, encoding)
        };
    }

    private static IQuantizer CreateQuantizer(GifQuality quality, GifEncodingOptions encoding)
    {
        var defaults = QualityDefaults(quality);
        var options = new QuantizerOptions
        {
            ColorMatchingMode = defaults.Matching,
            Dither = Dither(encoding.Dither, defaults.Dither),
            DitherScale = defaults.Scale,
            MaxColors = encoding.Colors ?? defaults.Colors
        };
        return quality is GifQuality.Low ? new OctreeQuantizer(options) : new WuQuantizer(options);
    }

    private static (ColorMatchingMode Matching, GifDitherMode Dither, float Scale, int Colors) QualityDefaults(GifQuality quality) =>
        quality switch
        {
            GifQuality.Low => (ColorMatchingMode.Coarse, GifDitherMode.None, 1f, 64),
            GifQuality.Medium => (ColorMatchingMode.Hybrid, GifDitherMode.Bayer, 0.35f, 128),
            GifQuality.High => (ColorMatchingMode.Exact, GifDitherMode.FloydSteinberg, 0.5f, 256),
            GifQuality.Archival => (ColorMatchingMode.Exact, GifDitherMode.FloydSteinberg, 1f, 256),
            _ => (ColorMatchingMode.Exact, GifDitherMode.FloydSteinberg, 0.75f, 256)
        };

    private static SixLabors.ImageSharp.Processing.Processors.Dithering.IDither? Dither(
        GifDitherMode selected,
        GifDitherMode fallback) => (selected is GifDitherMode.Default ? fallback : selected) switch
        {
            GifDitherMode.None => null,
            GifDitherMode.Bayer => KnownDitherings.Bayer4x4,
            GifDitherMode.Atkinson => KnownDitherings.Atkinson,
            GifDitherMode.Sierra => KnownDitherings.Sierra3,
            _ => KnownDitherings.FloydSteinberg
        };

    private static GifColorTableMode ColorTableMode(GifQuality quality, GifPaletteMode palette) => palette switch
    {
        GifPaletteMode.Global => GifColorTableMode.Global,
        GifPaletteMode.Local or GifPaletteMode.Adaptive => GifColorTableMode.Local,
        _ => quality is GifQuality.Low or GifQuality.Archival ? GifColorTableMode.Local : GifColorTableMode.Global
    };

    private void RetainSourceFrame(byte[] pngBytes, int index)
    {
        if (encoding.KeepFramesDirectory is null) return;
        Directory.CreateDirectory(encoding.KeepFramesDirectory);
        File.WriteAllBytes(Path.Combine(encoding.KeepFramesDirectory, $"frame-{index + 1:0000}.png"), pngBytes);
    }

    private static Image<Rgba32> NormalizeFrame(Image<Rgba32> frame, int width, int height)
    {
        if (frame.Width == width && frame.Height == height)
        {
            var clone = frame.Clone();
            SetFrameMetadata(
                clone.Frames.RootFrame.Metadata.GetGifMetadata(),
                frame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay);
            return clone;
        }

        var normalized = new Image<Rgba32>(width, height, Color.White);
        normalized.Mutate(context => context.DrawImage(frame, new Point(0, 0), 1f));
        SetFrameMetadata(
            normalized.Frames.RootFrame.Metadata.GetGifMetadata(),
            frame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay);

        return normalized;
    }

    private static void SetFrameMetadata(GifFrameMetadata metadata, int delayCentiseconds)
    {
        metadata.FrameDelay = delayCentiseconds;
        metadata.HasTransparency = false;
        metadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;
    }

    public void Dispose()
    {
        foreach (var frame in frames)
        {
            frame.Dispose();
        }
    }
}
