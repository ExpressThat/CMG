using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
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
        {
            throw new NotSupportedException($"Expected a GIF image, got {format.Name}.");
        }

        using var image = Image.Load<Rgba32>(file.FullName);
        var metadata = image.Metadata.GetGifMetadata();
        var palette = PaletteMode(image);
        var colors = CountColors(image);
        return new GifInspection(
            file.FullName,
            image.Width,
            image.Height,
            image.Frames.Count,
            DurationMilliseconds(image),
            file.Length,
            palette,
            colors >= PaletteOverflowThreshold ? ">256" : colors.ToString(System.Globalization.CultureInfo.InvariantCulture),
            HasTransparency(image),
            metadata.RepeatCount);
    }

    private static int DurationMilliseconds(Image<Rgba32> image)
    {
        var duration = 0;
        foreach (var frame in image.Frames)
        {
            duration += frame.Metadata.GetGifMetadata().FrameDelay * 10;
        }

        return duration;
    }

    private static bool HasTransparency(Image<Rgba32> image)
    {
        foreach (var frame in image.Frames)
        {
            if (frame.Metadata.GetGifMetadata().HasTransparency)
            {
                return true;
            }
        }

        return false;
    }

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
