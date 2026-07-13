using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Browser.Scripting.Recording;

public sealed record GifColorComparison(
    string SourcePath,
    string GifPath,
    int Frame,
    int Width,
    int Height,
    double MeanAbsoluteError,
    double RootMeanSquareError,
    int MaximumChannelError,
    long ChangedPixels,
    long TransparencyChangedPixels,
    long PixelCount)
{
    public string Format()
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        var changedPercent = PixelCount == 0 ? 0 : ChangedPixels * 100d / PixelCount;
        return $"GIF_COLOR_DIFF source={Quote(SourcePath)} gif={Quote(GifPath)} frame={Frame} width={Width} height={Height} " +
            $"meanAbsoluteError={MeanAbsoluteError.ToString("F4", culture)} rootMeanSquareError={RootMeanSquareError.ToString("F4", culture)} " +
            $"maximumChannelError={MaximumChannelError} changedPixels={ChangedPixels} transparencyChangedPixels={TransparencyChangedPixels} pixelCount={PixelCount} " +
            $"changedPercent={changedPercent.ToString("F4", culture)}";
    }

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

public static class GifColorComparer
{
    public static GifColorComparison Compare(FileInfo source, FileInfo gif, int frame)
    {
        if (frame < 1) throw new ArgumentOutOfRangeException(nameof(frame), "Frame must be 1 or greater.");
        using var sourceImage = Image.Load<Rgba32>(source.FullName);
        using var gifImage = Image.Load<Rgba32>(gif.FullName);
        if (frame > gifImage.Frames.Count)
            throw new ArgumentOutOfRangeException(nameof(frame), $"GIF contains {gifImage.Frames.Count} frame(s); frame {frame} does not exist.");
        using var gifFrame = gifImage.Frames.CloneFrame(frame - 1);
        if (sourceImage.Width != gifFrame.Width || sourceImage.Height != gifFrame.Height)
            throw new InvalidOperationException(
                $"Source dimensions {sourceImage.Width}x{sourceImage.Height} do not match GIF frame dimensions {gifFrame.Width}x{gifFrame.Height}.");

        long absolute = 0;
        long squared = 0;
        long changed = 0;
        long transparencyChanged = 0;
        var maximum = 0;
        sourceImage.ProcessPixelRows(gifFrame, (sourceRows, gifRows) =>
        {
            for (var y = 0; y < sourceRows.Height; y++)
            {
                var sourceRow = sourceRows.GetRowSpan(y);
                var gifRow = gifRows.GetRowSpan(y);
                for (var x = 0; x < sourceRow.Length; x++)
                {
                    var pixelChanged = false;
                    ComparePixel(sourceRow[x], gifRow[x], ref absolute, ref squared, ref maximum, ref pixelChanged);
                    if (pixelChanged) changed++;
                    if (sourceRow[x].A != gifRow[x].A) transparencyChanged++;
                }
            }
        });

        var pixels = (long)sourceImage.Width * sourceImage.Height;
        var channels = pixels * 3d;
        return new(source.FullName, gif.FullName, frame, sourceImage.Width, sourceImage.Height,
            absolute / channels, Math.Sqrt(squared / channels), maximum, changed, transparencyChanged, pixels);
    }

    private static void ComparePixel(Rgba32 source, Rgba32 encoded, ref long absolute, ref long squared, ref int maximum, ref bool changed)
    {
        CompareChannel(CompositeWhite(source.R, source.A), CompositeWhite(encoded.R, encoded.A), ref absolute, ref squared, ref maximum, ref changed);
        CompareChannel(CompositeWhite(source.G, source.A), CompositeWhite(encoded.G, encoded.A), ref absolute, ref squared, ref maximum, ref changed);
        CompareChannel(CompositeWhite(source.B, source.A), CompositeWhite(encoded.B, encoded.A), ref absolute, ref squared, ref maximum, ref changed);
    }

    private static byte CompositeWhite(byte color, byte alpha) => (byte)((color * alpha + 255 * (255 - alpha) + 127) / 255);

    private static void CompareChannel(byte source, byte encoded, ref long absolute, ref long squared, ref int maximum, ref bool changed)
    {
        var difference = Math.Abs(source - encoded);
        absolute += difference;
        squared += difference * difference;
        maximum = Math.Max(maximum, difference);
        changed |= difference != 0;
    }
}
