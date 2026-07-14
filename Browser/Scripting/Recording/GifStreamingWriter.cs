using System.Text;

namespace CMG.Browser.Scripting.Recording;

internal static class GifStreamingWriter
{
    public static void Write(string path, IReadOnlyList<string> framePaths)
    {
        if (framePaths.Count == 0) return;
        var first = GifQuantizedFrame.Read(framePaths[0]);
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: false);
        Header(writer, first.CanvasWidth, first.CanvasHeight);
        foreach (var framePath in framePaths) Frame(writer, GifQuantizedFrame.Read(framePath));
        writer.Write((byte)0x3B);
    }

    private static void Header(BinaryWriter writer, int width, int height)
    {
        writer.Write(Encoding.ASCII.GetBytes("GIF89a"));
        writer.Write((ushort)Math.Min(ushort.MaxValue, width));
        writer.Write((ushort)Math.Min(ushort.MaxValue, height));
        writer.Write((byte)0x70);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write(new byte[] { 0x21, 0xFF, 0x0B });
        writer.Write(Encoding.ASCII.GetBytes("NETSCAPE2.0"));
        writer.Write(new byte[] { 0x03, 0x01, 0x00, 0x00, 0x00 });
    }

    private static void Frame(BinaryWriter writer, GifQuantizedFrame frame)
    {
        var tableSize = TableSize(frame.Palette.Length);
        var sizeCode = Math.Max(0, (int)Math.Log2(tableSize) - 1);
        writer.Write(new byte[] { 0x21, 0xF9, 0x04, 0x04 });
        writer.Write((ushort)Math.Min(ushort.MaxValue, Math.Max(1, frame.Delay)));
        writer.Write(new byte[] { 0x00, 0x00 });
        writer.Write((byte)0x2C);
        writer.Write((ushort)Math.Min(ushort.MaxValue, frame.X));
        writer.Write((ushort)Math.Min(ushort.MaxValue, frame.Y));
        writer.Write((ushort)Math.Min(ushort.MaxValue, frame.Width));
        writer.Write((ushort)Math.Min(ushort.MaxValue, frame.Height));
        writer.Write((byte)(0x80 | sizeCode));
        for (var index = 0; index < tableSize; index++)
        {
            var color = index < frame.Palette.Length ? frame.Palette[index] : default;
            writer.Write(color.R); writer.Write(color.G); writer.Write(color.B);
        }
        GifLzwWriter.Write(writer, frame.Pixels, Math.Max(2, (int)Math.Log2(tableSize)));
    }

    private static int TableSize(int colors)
    {
        var size = 2;
        while (size < colors && size < 256) size <<= 1;
        return size;
    }
}
