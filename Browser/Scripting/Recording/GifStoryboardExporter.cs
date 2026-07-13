using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CMG.Browser.Scripting.Recording;

public sealed record GifStoryboardResult(
    string InputPath,
    string OutputPath,
    int TotalFrames,
    int ExportedFrames,
    int Columns,
    int Width,
    int Height)
{
    public string Format() =>
        $"GIF_STORYBOARD input={Quote(InputPath)} output={Quote(OutputPath)} " +
        $"frames={ExportedFrames}/{TotalFrames} columns={Columns} width={Width} height={Height}";

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

public static class GifStoryboardExporter
{
    public static GifStoryboardResult Export(FileInfo input, FileInfo output, int columns, int? maxFrames)
    {
        if (columns < 1)
        {
            throw new ArgumentException("columns must be at least 1.", nameof(columns));
        }

        if (maxFrames is not null && maxFrames < 1)
        {
            throw new ArgumentException("maxFrames must be at least 1.", nameof(maxFrames));
        }

        var format = Image.DetectFormat(input.FullName);
        if (!string.Equals(format.Name, "GIF", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Expected a GIF image, got {format.Name}.");
        }

        using var gif = Image.Load<Rgba32>(input.FullName);
        var indexes = SelectedFrameIndexes(gif.Frames.Count, maxFrames).ToArray();
        var rows = (int)Math.Ceiling(indexes.Length / (double)columns);
        using var storyboard = new Image<Rgba32>(gif.Width * columns, gif.Height * rows, Color.White);
        for (var index = 0; index < indexes.Length; index++)
        {
            CopyFrame(gif, indexes[index], storyboard, index % columns * gif.Width, index / columns * gif.Height);
        }

        output.Directory?.Create();
        storyboard.SaveAsPng(output.FullName);
        return new(
            input.FullName,
            output.FullName,
            gif.Frames.Count,
            indexes.Length,
            columns,
            storyboard.Width,
            storyboard.Height);
    }

    private static IEnumerable<int> SelectedFrameIndexes(int frameCount, int? maxFrames)
    {
        var count = Math.Min(frameCount, maxFrames ?? frameCount);
        if (count == frameCount)
        {
            return Enumerable.Range(0, frameCount);
        }

        return Enumerable.Range(0, count)
            .Select(index => (int)Math.Round(index * (frameCount - 1) / (double)Math.Max(1, count - 1)))
            .Distinct();
    }

    private static void CopyFrame(Image<Rgba32> gif, int frameIndex, Image<Rgba32> storyboard, int offsetX, int offsetY)
    {
        using var frame = gif.Frames.CloneFrame(frameIndex);
        storyboard.Mutate(context => context.DrawImage(frame, new Point(offsetX, offsetY), 1f));
    }
}
