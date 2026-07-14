using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Browser.Scripting.Recording;

public sealed record GifInspection(
    string Path,
    int Width,
    int Height,
    int FrameCount,
    int DurationMilliseconds,
    long SizeBytes,
    string Palette,
    string PaletteColors,
    bool Transparent,
    ushort RepeatCount)
{
    public string Format() =>
        $"GIF_INSPECT path={Quote(Path)} frames={FrameCount} durationMs={DurationMilliseconds} " +
        $"width={Width} height={Height} sizeBytes={SizeBytes} palette={Palette} " +
        $"paletteColors={PaletteColors} transparent={Transparent.ToString().ToLowerInvariant()} repeat={RepeatCount}";

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

public static class GifInspector
{
    private const int PaletteOverflowThreshold = 257;

    public static GifInspection Inspect(FileInfo file)
    {
        var format = Image.DetectFormat(file.FullName);
        if (!string.Equals(format.Name, "GIF", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Expected a GIF image, got {format.Name}.");
        return InspectRecording(file, format.Name);
    }

    public static GifInspection InspectRecording(FileInfo file)
    {
        var format = Image.DetectFormat(file.FullName);
        return InspectRecording(file, format.Name);
    }

    private static GifInspection InspectRecording(FileInfo file, string formatName)
    {
        if (!new[] { "GIF", "PNG", "WEBP" }.Contains(formatName, StringComparer.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Expected GIF, APNG, or WebP visual evidence, got {formatName}.");
        }

        using var image = Image.Load<Rgba32>(file.FullName);
        var gif = formatName.Equals("GIF", StringComparison.OrdinalIgnoreCase);
        var palette = gif ? PaletteMode(image) : "truecolor";
        var colors = CountColors(image);
        return new GifInspection(
            file.FullName,
            image.Width,
            image.Height,
            image.Frames.Count,
            DurationMilliseconds(image, formatName),
            file.Length,
            palette,
            colors >= PaletteOverflowThreshold ? ">256" : colors.ToString(System.Globalization.CultureInfo.InvariantCulture),
            HasTransparency(image),
            RepeatCount(image, formatName));
    }

    private static int DurationMilliseconds(Image<Rgba32> image, string format)
    {
        var duration = 0;
        foreach (var frame in image.Frames)
        {
            duration += format.ToUpperInvariant() switch
            {
                "PNG" => (int)Math.Round(frame.Metadata.GetPngMetadata().FrameDelay.ToDouble() * 1000),
                "WEBP" => (int)frame.Metadata.GetWebpMetadata().FrameDelay,
                _ => frame.Metadata.GetGifMetadata().FrameDelay * 10
            };
        }

        return duration;
    }

    private static bool HasTransparency(Image<Rgba32> image)
    {
        foreach (var frame in image.Frames)
        {
            var transparent = false;
            frame.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height && !transparent; y++)
                    foreach (var pixel in accessor.GetRowSpan(y))
                        if (pixel.A < 255) { transparent = true; break; }
            });
            if (transparent)
            {
                return true;
            }
        }

        return false;
    }

    private static ushort RepeatCount(Image<Rgba32> image, string format) => format.ToUpperInvariant() switch
    {
        "PNG" => (ushort)Math.Min(ushort.MaxValue, image.Metadata.GetPngMetadata().RepeatCount),
        "WEBP" => (ushort)Math.Min(ushort.MaxValue, image.Metadata.GetWebpMetadata().RepeatCount),
        _ => image.Metadata.GetGifMetadata().RepeatCount
    };

    private static string PaletteMode(Image<Rgba32> image)
    {
        var modes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var frame in image.Frames)
        {
            modes.Add(frame.Metadata.GetGifMetadata().ColorTableMode.ToString().ToLowerInvariant());
        }

        return modes.Count is 1 ? modes.Single() : "mixed";
    }

    private static int CountColors(Image<Rgba32> image)
    {
        var colors = new HashSet<uint>();
        foreach (var frame in image.Frames)
        {
            frame.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height && colors.Count < PaletteOverflowThreshold; y++)
                {
                    foreach (var pixel in accessor.GetRowSpan(y))
                    {
                        colors.Add(pixel.PackedValue);
                        if (colors.Count >= PaletteOverflowThreshold)
                        {
                            break;
                        }
                    }
                }
            });
        }

        return colors.Count;
    }
}
