using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Browser.Scripting.Recording;

public sealed record GifTrimResult(string InputPath, string OutputPath, int FramesBefore, int FramesAfter,
    int DurationBeforeMilliseconds, int DurationAfterMilliseconds)
{
    public string Format() => $"GIF_TRIM input={Quote(InputPath)} output={Quote(OutputPath)} framesBefore={FramesBefore} " +
        $"framesAfter={FramesAfter} durationBeforeMs={DurationBeforeMilliseconds} durationAfterMs={DurationAfterMilliseconds}";
    private static string Quote(string value) => $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}

public static class GifTrimmer
{
    public static GifTrimResult Trim(FileInfo input, FileInfo output, int? startFrame, int? endFrame, int? startTime, int? endTime)
    {
        ValidateModes(startFrame, endFrame, startTime, endTime);
        var format = Image.DetectFormat(input.FullName);
        if (!string.Equals(format.Name, "GIF", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Expected a GIF image, got {format.Name}.");
        using var gif = Image.Load<Rgba32>(input.FullName);
        var beforeDuration = Duration(gif);
        var ranges = startTime is not null || endTime is not null
            ? TimeRanges(gif, startTime ?? 0, endTime ?? beforeDuration)
            : FrameRanges(gif, startFrame ?? 0, endFrame ?? gif.Frames.Count - 1);
        if (ranges.Count == 0) throw new ArgumentException("The trim range does not contain any GIF frames.");
        using var trimmed = Build(gif, ranges);
        output.Directory?.Create();
        trimmed.SaveAsGif(output.FullName, GifFrameSink.CreateEncoder());
        return new(input.FullName, output.FullName, gif.Frames.Count, trimmed.Frames.Count, beforeDuration, Duration(trimmed));
    }

    private static void ValidateModes(int? sf, int? ef, int? st, int? et)
    {
        if ((sf is not null || ef is not null) && (st is not null || et is not null))
            throw new ArgumentException("Use either frame options or time options, not both.");
        if (sf < 0 || ef < 0 || st < 0 || et < 0) throw new ArgumentException("Trim values must be zero or greater.");
        if (sf > ef || st >= et) throw new ArgumentException("The trim start must be before the trim end.");
    }

    private static List<(int Index, int Delay)> FrameRanges(Image<Rgba32> gif, int start, int end)
    {
        if (start >= gif.Frames.Count || end >= gif.Frames.Count) throw new ArgumentOutOfRangeException(nameof(start), "Frame range exceeds the GIF frame count.");
        return Enumerable.Range(start, end - start + 1).Select(index => (index, Delay(gif.Frames[index]))).ToList();
    }

    private static List<(int Index, int Delay)> TimeRanges(Image<Rgba32> gif, int start, int end)
    {
        var ranges = new List<(int, int)>();
        var cursor = 0;
        for (var index = 0; index < gif.Frames.Count; index++)
        {
            var next = cursor + Delay(gif.Frames[index]);
            var overlap = Math.Max(0, Math.Min(next, end) - Math.Max(cursor, start));
            if (overlap > 0) ranges.Add((index, Math.Max(10, overlap)));
            cursor = next;
        }
        return ranges;
    }

    private static Image<Rgba32> Build(Image<Rgba32> source, IReadOnlyList<(int Index, int Delay)> ranges)
    {
        using var first = source.Frames.CloneFrame(ranges[0].Index);
        var result = first.Clone();
        SetDelay(result.Frames.RootFrame, ranges[0].Delay);
        result.Metadata.GetGifMetadata().RepeatCount = source.Metadata.GetGifMetadata().RepeatCount;
        foreach (var range in ranges.Skip(1))
        {
            using var frame = source.Frames.CloneFrame(range.Index);
            SetDelay(frame.Frames.RootFrame, range.Delay);
            result.Frames.AddFrame(frame.Frames.RootFrame);
        }
        return result;
    }

    private static int Duration(Image<Rgba32> gif)
    {
        var duration = 0;
        for (var index = 0; index < gif.Frames.Count; index++) duration += Delay(gif.Frames[index]);
        return duration;
    }
    private static int Delay(ImageFrame<Rgba32> frame) => frame.Metadata.GetGifMetadata().FrameDelay * 10;
    private static void SetDelay(ImageFrame<Rgba32> frame, int milliseconds) => frame.Metadata.GetGifMetadata().FrameDelay = Math.Max(1, (milliseconds + 9) / 10);
}
