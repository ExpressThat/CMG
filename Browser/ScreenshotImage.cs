using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace CMG.Browser;

public static class ScreenshotImage
{
    public static string NormalizeType(string? type) =>
        string.IsNullOrWhiteSpace(type) ? "png" :
        type.Equals("jpg", StringComparison.OrdinalIgnoreCase) ? "jpeg" :
        type.ToLowerInvariant();

    public static string MimeType(string type) => NormalizeType(type) == "jpeg" ? "image/jpeg" : "image/png";

    public static byte[] ConvertIfNeeded(byte[] bytes, ScreenshotOptions options)
    {
        if (options.Clip is { } clip)
        {
            return CropAndConvert(bytes, options, clip);
        }

        if (NormalizeType(options.Type) != "jpeg")
        {
            return bytes;
        }

        using var image = Image.Load(bytes);
        using var stream = new MemoryStream();
        image.SaveAsJpeg(stream, new JpegEncoder { Quality = options.Quality ?? 80 });
        return stream.ToArray();
    }

    private static byte[] CropAndConvert(byte[] bytes, ScreenshotOptions options, ScreenshotClip clip)
    {
        using var image = Image.Load(bytes);
        var x = Math.Clamp((int)Math.Floor(clip.X), 0, Math.Max(0, image.Width - 1));
        var y = Math.Clamp((int)Math.Floor(clip.Y), 0, Math.Max(0, image.Height - 1));
        var width = Math.Min((int)Math.Ceiling(clip.Width), image.Width - x);
        var height = Math.Min((int)Math.Ceiling(clip.Height), image.Height - y);
        image.Mutate(context => context.Crop(new Rectangle(x, y, Math.Max(1, width), Math.Max(1, height))));

        using var stream = new MemoryStream();
        if (NormalizeType(options.Type) == "jpeg")
        {
            image.SaveAsJpeg(stream, new JpegEncoder { Quality = options.Quality ?? 80 });
        }
        else
        {
            image.SaveAsPng(stream);
        }

        return stream.ToArray();
    }
}
