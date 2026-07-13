using System.Security.Cryptography;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class GifFrameSink
{
    private const int MaximumGifDelayCentiseconds = ushort.MaxValue;
    private byte[]? previousSignature;
    private long retainedPixelBytes;

    public int SourceFrameCount { get; private set; }
    public int DuplicateFramesCoalesced { get; private set; }
    public int SampledFramesSkipped { get; private set; }
    public int BlankFrameCount { get; private set; }
    public long PeakRetainedPixelBytes { get; private set; }
    public double ProcessingMilliseconds { get; private set; }

    public bool TrySkipSampledFrame(int delay)
    {
        if (!TryMergeDelay(delay)) return false;
        SourceFrameCount++;
        SampledFramesSkipped++;
        return true;
    }

    private bool TryMergeDelay(int delay)
    {
        if (frames.Count is 0) return false;
        var metadata = frames[^1].Frames.RootFrame.Metadata.GetGifMetadata();
        if (metadata.FrameDelay > MaximumGifDelayCentiseconds - delay) return false;
        metadata.FrameDelay += delay;
        return true;
    }

    private bool IsDuplicate(Image<Rgba32> image)
    {
        if (frames.Count is 0 || frames[^1].Width != image.Width || frames[^1].Height != image.Height) return false;
        var signature = PixelSignature(image);
        return previousSignature is not null && signature.AsSpan().SequenceEqual(previousSignature);
    }

    private void TrackRetainedFrame(Image<Rgba32> image)
    {
        previousSignature = PixelSignature(image);
        retainedPixelBytes += (long)image.Width * image.Height * 4;
        PeakRetainedPixelBytes = Math.Max(PeakRetainedPixelBytes, retainedPixelBytes);
        if (IsMostlyBlank(image)) BlankFrameCount++;
    }

    private static byte[] PixelSignature(Image<Rgba32> image)
    {
        var pixels = new Rgba32[checked(image.Width * image.Height)];
        image.CopyPixelDataTo(pixels);
        return SHA256.HashData(MemoryMarshal.AsBytes(pixels.AsSpan()));
    }

    private static bool IsMostlyBlank(Image<Rgba32> image)
    {
        var blank = 0L;
        var sampled = 0L;
        var stride = Math.Max(1, (int)Math.Sqrt(image.Width * (double)image.Height / 10_000));
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y += stride)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x += stride)
                {
                    var pixel = row[x];
                    var white = pixel.R >= 248 && pixel.G >= 248 && pixel.B >= 248;
                    var black = pixel.R <= 7 && pixel.G <= 7 && pixel.B <= 7;
                    if (pixel.A <= 7 || white || black) blank++;
                    sampled++;
                }
            }
        });
        return sampled > 0 && blank / (double)sampled >= .995;
    }

    private void AddProcessingTime(long started) =>
        ProcessingMilliseconds += System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;
}

public sealed record GifFrameAddResult(bool Retained, bool DelayMergedToPrevious, int FrameIndex);
