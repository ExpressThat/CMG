using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CMG.Browser.Scripting.Recording;

public sealed partial class GifFrameSink
{
    public int ChangedRegionFrameCount { get; private set; }
    public long SpoolBytes { get; private set; }
    public int ParallelPreprocessedFrameCount { get; internal set; }
    public bool UsesStreamingGif =>
        encoding.Format is GifArtifactFormat.Gif && encoding.Palette is not GifPaletteMode.Global;
    internal string SpoolDirectory => spoolDirectory;

    private void StoreFrame(Image<Rgba32> image, int delay)
    {
        Directory.CreateDirectory(spoolDirectory);
        var bounds = ChangedBounds(previousFrame, image);
        var useDelta = bounds is Rectangle && bounds.Value.Width * bounds.Value.Height < image.Width * image.Height;
        var region = useDelta ? bounds!.Value : new Rectangle(0, 0, image.Width, image.Height);
        var path = Path.Combine(spoolDirectory, $"frame-{frames.Count + 1:000000}.png");
        using (var stored = image.Clone(context => context.Crop(region))) stored.SaveAsPng(path);
        frames.Add(new GifStoredFrame(
            path, region.X, region.Y, region.Width, region.Height, image.Width, image.Height, delay));
        if (useDelta) ChangedRegionFrameCount++;
        SpoolBytes += new FileInfo(path).Length;
        TrackRetainedFrame(image);
        previousFrame?.Dispose();
        previousFrame = image.Clone();
    }

    private static Rectangle? ChangedBounds(Image<Rgba32>? previous, Image<Rgba32> current)
    {
        if (previous is null || previous.Width != current.Width || previous.Height != current.Height) return null;
        var left = current.Width;
        var top = current.Height;
        var right = -1;
        var bottom = -1;
        current.ProcessPixelRows(previous, (currentRows, previousRows) =>
        {
            for (var y = 0; y < currentRows.Height; y++)
            {
                var row = currentRows.GetRowSpan(y);
                var old = previousRows.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    if (row[x].Equals(old[x])) continue;
                    left = Math.Min(left, x);
                    top = Math.Min(top, y);
                    right = Math.Max(right, x);
                    bottom = Math.Max(bottom, y);
                }
            }
        });
        return right < left ? null : new Rectangle(left, top, right - left + 1, bottom - top + 1);
    }

    private Image<Rgba32> LoadCanvas(int index, Image<Rgba32>? previous)
    {
        var frame = frames[index];
        using var patch = Image.Load<Rgba32>(frame.Path);
        Image<Rgba32> canvas;
        if (frame.IsDelta && previous is not null &&
            previous.Width == frame.CanvasWidth && previous.Height == frame.CanvasHeight)
        {
            canvas = previous.Clone(context => context.DrawImage(patch, new Point(frame.X, frame.Y), 1));
        }
        else
        {
            canvas = new Image<Rgba32>(frame.CanvasWidth, frame.CanvasHeight, Color.White);
            canvas.Mutate(context => context.DrawImage(patch, new Point(frame.X, frame.Y), 1));
        }
        SetFrameMetadata(canvas.Frames.RootFrame.Metadata.GetGifMetadata(), frame.Delay);
        return canvas;
    }

    internal void VisitCanvases(IReadOnlySet<int> selected, Action<int, Image<Rgba32>> visit)
    {
        Image<Rgba32>? previous = null;
        try
        {
            for (var index = 0; index < frames.Count; index++)
            {
                var canvas = LoadCanvas(index, previous);
                previous?.Dispose();
                previous = canvas;
                if (selected.Contains(index)) visit(index, canvas);
            }
        }
        finally
        {
            previous?.Dispose();
        }
    }
}
