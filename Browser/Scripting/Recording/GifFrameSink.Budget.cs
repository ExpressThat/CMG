using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class GifFrameSink
{
    public bool BudgetApplied { get; private set; }
    public bool BudgetMet { get; private set; }
    public int BudgetAttempts { get; private set; }
    public long? BudgetBytes => encoding.SizeBudget?.Bytes;
    public long FinalSizeBytes { get; private set; }
    public double FinalBudgetScale { get; private set; } = 1;
    public GifQuality FinalBudgetQuality { get; private set; }
    public int EncodedWidth { get; private set; }
    public int EncodedHeight { get; private set; }

    private void SaveWithBudget(string path)
    {
        var budget = encoding.SizeBudget;
        var candidates = Candidates(budget).ToArray();
        var extension = Path.GetExtension(path);
        var temporary = $"{path}.{Guid.NewGuid():N}.tmp{extension}";
        var best = $"{path}.{Guid.NewGuid():N}.best{extension}";
        var bestSize = long.MaxValue;
        try
        {
            foreach (var candidate in candidates)
            {
                Encode(temporary, candidate.Quality, candidate.Scale);
                BudgetAttempts++;
                var size = new FileInfo(temporary).Length;
                if (size < bestSize)
                {
                    File.Copy(temporary, best, overwrite: true);
                    bestSize = size;
                    FinalBudgetQuality = candidate.Quality;
                    FinalBudgetScale = candidate.Scale;
                }
                if (budget?.Bytes is not long limit || size <= limit) break;
                BudgetApplied = true;
            }
            File.Move(best, path, overwrite: true);
            FinalSizeBytes = bestSize;
            BudgetMet = budget?.Bytes is not long bytes || bestSize <= bytes;
            EncodedWidth = Math.Max(1, (int)Math.Round(frames.Max(frame => frame.CanvasWidth) * FinalBudgetScale));
            EncodedHeight = Math.Max(1, (int)Math.Round(frames.Max(frame => frame.CanvasHeight) * FinalBudgetScale));
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            if (File.Exists(best)) File.Delete(best);
        }
    }

    private IEnumerable<(GifQuality Quality, double Scale)> Candidates(GifSizeBudgetOptions? budget)
    {
        yield return (quality, 1);
        if (budget?.Bytes is null) yield break;
        if (budget.QualityFallback && encoding.Format is not GifArtifactFormat.Apng)
        {
            foreach (var fallback in QualityFallbacks(quality)) yield return (fallback, 1);
        }
        if (!budget.DownscaleFallback) yield break;
        var lowest = budget.QualityFallback ? GifQuality.Low : quality;
        foreach (var scale in new[] { .85, .7, .55, .4, .3 }) yield return (lowest, scale);
    }

    private static IEnumerable<GifQuality> QualityFallbacks(GifQuality requested) => requested switch
    {
        GifQuality.Archival => [GifQuality.Highest, GifQuality.High, GifQuality.Medium, GifQuality.Low],
        GifQuality.Highest => [GifQuality.High, GifQuality.Medium, GifQuality.Low],
        GifQuality.High => [GifQuality.Medium, GifQuality.Low],
        GifQuality.Medium => [GifQuality.Low],
        _ => []
    };

    private void Encode(string path, GifQuality selectedQuality, double scale)
    {
        if (encoding.Format is GifArtifactFormat.Gif && encoding.Palette is not GifPaletteMode.Global)
        {
            EncodeStreamingGif(path, selectedQuality, scale);
            return;
        }
        var sourceWidth = frames.Max(frame => frame.CanvasWidth);
        var sourceHeight = frames.Max(frame => frame.CanvasHeight);
        var width = Math.Max(1, (int)Math.Round(sourceWidth * scale));
        var height = Math.Max(1, (int)Math.Round(sourceHeight * scale));
        using var first = LoadCanvas(0, previous: null);
        using var gif = BudgetFrame(first, sourceWidth, sourceHeight, width, height);
        SetAnimationMetadata(gif, encoding.Format);
        Image<Rgba32>? previous = first.Clone();
        for (var index = 1; index < frames.Count; index++)
        {
            using var current = LoadCanvas(index, previous);
            previous.Dispose();
            previous = current.Clone();
            using var normalized = BudgetFrame(current, sourceWidth, sourceHeight, width, height);
            gif.Frames.AddFrame(normalized.Frames.RootFrame);
        }
        previous?.Dispose();
        EncodeArtifact(gif, path, selectedQuality);
    }

    private static Image<Rgba32> BudgetFrame(
        Image<Rgba32> frame,
        int sourceWidth,
        int sourceHeight,
        int width,
        int height)
    {
        var clone = NormalizeFrame(frame, sourceWidth, sourceHeight);
        if (sourceWidth == width && sourceHeight == height) return clone;
        clone.Mutate(context => context.Resize(new ResizeOptions
        {
            Size = new Size(width, height), Mode = ResizeMode.Stretch, Sampler = KnownResamplers.Lanczos3
        }));
        SetFrameMetadata(clone.Frames.RootFrame.Metadata.GetGifMetadata(),
            frame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay);
        return clone;
    }

    internal long EstimatedRgbaBytes(int startFrameIndex, int? endFrameIndex)
    {
        if (frames.Count == 0) return 0;
        var start = Math.Clamp(startFrameIndex, 0, frames.Count - 1);
        var end = Math.Clamp(endFrameIndex ?? start, start, frames.Count - 1);
        return frames.Skip(start).Take(end - start + 1).Sum(frame => frame.PixelBytes);
    }
}
