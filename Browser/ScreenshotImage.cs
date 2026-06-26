using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

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
        if (NormalizeType(options.Type) != "jpeg")
        {
            return bytes;
        }

        using var image = Image.Load(bytes);
        using var stream = new MemoryStream();
        image.SaveAsJpeg(stream, new JpegEncoder { Quality = options.Quality ?? 80 });
        return stream.ToArray();
    }
}
