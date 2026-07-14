using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Browser.Scripting.Recording;

internal sealed record GifQuantizedFrame(
    int X, int Y, int Width, int Height, int CanvasWidth, int CanvasHeight,
    int Delay, Rgba32[] Palette, byte[] Pixels)
{
    public static void Write(
        string path, Image<Rgba32> frame,
        int x, int y, int canvasWidth, int canvasHeight, int delay)
    {
        var colors = new Dictionary<uint, byte>();
        var pixels = new byte[checked(frame.Width * frame.Height)];
        var offset = 0;
        frame.ProcessPixelRows(rows =>
        {
            for (var row = 0; row < rows.Height; row++)
                foreach (var pixel in rows.GetRowSpan(row))
                {
                    if (!colors.TryGetValue(pixel.PackedValue, out var index))
                    {
                        if (colors.Count >= 256) throw new InvalidOperationException("Quantized GIF frame exceeds 256 colors.");
                        colors[pixel.PackedValue] = index = (byte)colors.Count;
                    }
                    pixels[offset++] = index;
                }
        });
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);
        writer.Write(x); writer.Write(y); writer.Write(frame.Width); writer.Write(frame.Height);
        writer.Write(canvasWidth); writer.Write(canvasHeight); writer.Write(delay);
        writer.Write(colors.Count);
        foreach (var packed in colors.Keys)
        {
            var color = new Rgba32(packed);
            writer.Write(color.R); writer.Write(color.G); writer.Write(color.B); writer.Write(color.A);
        }
        writer.Write(pixels);
    }

    public static GifQuantizedFrame Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        var x = reader.ReadInt32(); var y = reader.ReadInt32();
        var width = reader.ReadInt32(); var height = reader.ReadInt32();
        var canvasWidth = reader.ReadInt32(); var canvasHeight = reader.ReadInt32();
        var delay = reader.ReadInt32();
        var palette = new Rgba32[reader.ReadInt32()];
        for (var index = 0; index < palette.Length; index++)
            palette[index] = new Rgba32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        return new(x, y, width, height, canvasWidth, canvasHeight, delay, palette,
            reader.ReadBytes(checked(width * height)));
    }
}
