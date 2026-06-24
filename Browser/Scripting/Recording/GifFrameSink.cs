using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Browser.Scripting.Recording;

public sealed class GifFrameSink : IDisposable
{
    private readonly List<Image<Rgba32>> frames = [];

    public void AddFrame(byte[] pngBytes, int delayCentiseconds)
    {
        var image = Image.Load<Rgba32>(pngBytes);
        image.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = delayCentiseconds;
        frames.Add(image);
    }

    public void Save(string path)
    {
        if (frames.Count is 0)
        {
            return;
        }

        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var gif = frames[0].Clone();
        gif.Metadata.GetGifMetadata().RepeatCount = 0;

        for (var index = 1; index < frames.Count; index++)
        {
            gif.Frames.AddFrame(frames[index].Frames.RootFrame);
        }

        gif.SaveAsGif(fullPath);
    }

    public void Dispose()
    {
        foreach (var frame in frames)
        {
            frame.Dispose();
        }
    }
}
