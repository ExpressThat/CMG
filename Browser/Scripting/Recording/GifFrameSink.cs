using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class GifFrameSink : IDisposable
{
    private readonly List<Image<Rgba32>> frames = [];
    private readonly GifQuality quality;
    private readonly GifEncodingOptions encoding;
    private readonly GifFramingOptions framing;

    public GifFrameSink(GifQuality quality = GifQuality.Highest, GifEncodingOptions? encoding = null, GifFramingOptions? framing = null)
    {
        this.quality = quality;
        this.encoding = encoding ?? new GifEncodingOptions();
        this.framing = framing ?? new GifFramingOptions();
    }

    public GifFrameAddResult AddFrame(
        byte[] pngBytes,
        int delayCentiseconds,
        bool? coalesceDuplicates = null)
    {
        SourceFrameCount++;
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        var image = Image.Load<Rgba32>(pngBytes);
        TrackColorMetadata(image);
        var resized = ResizeFrame(image);
        var colorAdjusted = ApplyColorOptions(image);
        var retainedPng = resized || colorAdjusted ? Png(image) : pngBytes;
        var coalesce = coalesceDuplicates ?? encoding.CaptureOptimization?.CoalesceDuplicates ?? true;
        if (coalesce && IsDuplicate(image) && TryMergeDelay(delayCentiseconds))
        {
            DuplicateFramesCoalesced++;
            image.Dispose();
            AddProcessingTime(started);
            return new(false, true, frames.Count - 1);
        }
        RetainSourceFrame(retainedPng, frames.Count);
        SetFrameMetadata(image.Frames.RootFrame.Metadata.GetGifMetadata(), delayCentiseconds);
        frames.Add(image);
        TrackRetainedFrame(image);
        AddProcessingTime(started);
        return new(true, false, frames.Count - 1);
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
            ColorTableMode = ColorTableMode(quality, encoding.Palette, encoding.Color),
            Quantizer = CreateQuantizer(quality, encoding)
        };
    }

    private static IQuantizer CreateQuantizer(GifQuality quality, GifEncodingOptions encoding)
    {
        var defaults = EncodingDefaults(quality, encoding);
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

    private static (ColorMatchingMode Matching, GifDitherMode Dither, float Scale, int Colors) EncodingDefaults(
        GifQuality quality,
        GifEncodingOptions encoding)
    {
        var defaults = QualityDefaults(quality);
        return encoding.Color switch
        {
            { HighContrastPalette: true } => (ColorMatchingMode.Exact, GifDitherMode.None, 1f, 256),
            { GradientMode: GifGradientMode.Smooth } => (ColorMatchingMode.Exact, GifDitherMode.FloydSteinberg, 1f, 256),
            { GradientMode: GifGradientMode.Text } => (ColorMatchingMode.Exact, GifDitherMode.None, 1f, 256),
            _ => defaults
        };
    }

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

    private static GifColorTableMode ColorTableMode(GifQuality quality, GifPaletteMode palette, GifColorOptions? color = null) => palette switch
    {
        GifPaletteMode.Global => GifColorTableMode.Global,
        GifPaletteMode.Local or GifPaletteMode.Adaptive => GifColorTableMode.Local,
        _ when color is { HighContrastPalette: true } or { GradientMode: GifGradientMode.Smooth } => GifColorTableMode.Local,
        _ when color is { GradientMode: GifGradientMode.Text } => GifColorTableMode.Global,
        _ => quality is GifQuality.Low or GifQuality.Archival ? GifColorTableMode.Local : GifColorTableMode.Global
    };

    private void RetainSourceFrame(byte[] pngBytes, int index)
    {
        if (encoding.KeepFramesDirectory is null) return;
        Directory.CreateDirectory(encoding.KeepFramesDirectory);
        File.WriteAllBytes(Path.Combine(encoding.KeepFramesDirectory, $"frame-{index + 1:0000}.png"), pngBytes);
    }

    private bool ResizeFrame(Image<Rgba32> image)
    {
        var factor = framing.Scale;
        if (framing.MaxWidth is int maxWidth) factor = Math.Min(factor, maxWidth / (double)image.Width);
        if (framing.MaxHeight is int maxHeight) factor = Math.Min(factor, maxHeight / (double)image.Height);
        factor = Math.Min(1d, factor);
        var width = Math.Max(1, (int)Math.Round(image.Width * factor));
        var height = Math.Max(1, (int)Math.Round(image.Height * factor));
        if (width == image.Width && height == image.Height) return false;
        image.Mutate(context => context.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Stretch,
            Sampler = KnownResamplers.Lanczos3
        }));
        return true;
    }

    private static byte[] Png(Image<Rgba32> image)
    {
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
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
