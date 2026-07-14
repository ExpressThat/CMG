using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CMG.Browser.Scripting.Recording;

internal static class GifSplitViewComposer
{
    public static byte[] Compose(IReadOnlyList<byte[]> captures, bool reserveSecondTile = false)
    {
        if (captures.Count == 0) return [];
        if (captures.Count == 1 && !reserveSecondTile) return captures[0];
        var images = captures.Select(bytes => Image.Load<Rgba32>(bytes)).ToArray();
        try
        {
            var tileWidth = images.Max(image => image.Width);
            var tileHeight = images.Max(image => image.Height);
            var columns = 2;
            var rows = Math.Max(1, (images.Length + columns - 1) / columns);
            const int gap = 6;
            using var output = new Image<Rgba32>(tileWidth * columns + gap, tileHeight * rows + gap * (rows - 1), Color.ParseHex("111827"));
            for (var index = 0; index < images.Length; index++)
            {
                images[index].Mutate(context => context.Resize(new ResizeOptions
                {
                    Size = new Size(tileWidth, tileHeight),
                    Mode = ResizeMode.Pad,
                    PadColor = Color.ParseHex("111827")
                }));
                output.Mutate(context => context.DrawImage(images[index],
                    new Point(index % columns * (tileWidth + gap), index / columns * (tileHeight + gap)), 1f));
            }
            using var stream = new MemoryStream();
            output.SaveAsPng(stream);
            return stream.ToArray();
        }
        finally
        {
            foreach (var image in images) image.Dispose();
        }
    }
}
