using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CMG.Runner;

internal static class CmgGifFrameRenderer
{
    public static string? DataUri(string gifPath, int frameIndex)
    {
        try
        {
            using var gif = Image.Load<Rgba32>(gifPath);
            if (frameIndex < 0 || frameIndex >= gif.Frames.Count)
            {
                return null;
            }

            using var frame = gif.Frames.CloneFrame(frameIndex);
            frame.Mutate(context => context.Resize(new ResizeOptions
            {
                Size = new Size(360, 240),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            }));
            using var stream = new MemoryStream();
            frame.SaveAsPng(stream);
            return $"data:image/png;base64,{Convert.ToBase64String(stream.ToArray())}";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or UnknownImageFormatException)
        {
            return null;
        }
    }
}
