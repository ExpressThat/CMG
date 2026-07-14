using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CMG.Browser.Scripting.Recording;

internal static class GifFrameCompositor
{
    public static IReadOnlyList<Image<Rgba32>> SelectedFrames(
        Image<Rgba32> gif,
        IReadOnlyList<GifFrameDescriptor> descriptors,
        ISet<int> selected)
    {
        if (descriptors.Count != gif.Frames.Count)
            throw new InvalidDataException(
                $"GIF descriptor count {descriptors.Count} does not match decoded frame count {gif.Frames.Count}.");

        var results = new List<Image<Rgba32>>(selected.Count);
        using var canvas = new Image<Rgba32>(gif.Width, gif.Height, Color.Transparent);
        for (var index = 0; index < gif.Frames.Count; index++)
        {
            var descriptor = descriptors[index];
            var bounds = Bounds(descriptor, gif.Width, gif.Height);
            using var prior = descriptor.Disposal == 3 ? canvas.Clone() : null;
            using var decoded = gif.Frames.CloneFrame(index);
            using var patch = decoded.Clone(context => context.Crop(bounds));
            canvas.Mutate(context => context.DrawImage(patch, bounds.Location, 1f));
            if (selected.Contains(index)) results.Add(canvas.Clone());
            ApplyDisposal(canvas, prior, bounds, descriptor.Disposal);
        }

        return results;
    }

    private static Rectangle Bounds(GifFrameDescriptor descriptor, int width, int height)
    {
        var bounds = new Rectangle(descriptor.Left, descriptor.Top, descriptor.Width, descriptor.Height);
        var clipped = Rectangle.Intersect(bounds, new Rectangle(0, 0, width, height));
        if (clipped.Width == 0 || clipped.Height == 0)
            throw new InvalidDataException("A GIF frame lies outside the logical screen.");
        return clipped;
    }

    private static void ApplyDisposal(
        Image<Rgba32> canvas,
        Image<Rgba32>? prior,
        Rectangle bounds,
        int disposal)
    {
        if (disposal == 3 && prior is not null)
        {
            canvas.Mutate(context => context.DrawImage(
                prior,
                PixelColorBlendingMode.Normal,
                PixelAlphaCompositionMode.Src,
                1f));
        }
        else if (disposal == 2)
        {
            canvas.ProcessPixelRows(accessor =>
            {
                for (var row = bounds.Top; row < bounds.Bottom; row++)
                    accessor.GetRowSpan(row).Slice(bounds.Left, bounds.Width).Clear();
            });
        }
    }
}
