namespace CMG.Browser.Scripting.Recording;

internal sealed record GifFrameDescriptor(int Left, int Top, int Width, int Height, int Disposal);

internal static class GifFrameDescriptorReader
{
    public static IReadOnlyList<GifFrameDescriptor> Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        var signature = new string(reader.ReadChars(6));
        if (signature is not ("GIF87a" or "GIF89a"))
            throw new InvalidDataException("The input is not a valid GIF data stream.");

        ReadLogicalScreen(reader);
        var frames = new List<GifFrameDescriptor>();
        var disposal = 0;
        while (stream.Position < stream.Length)
        {
            switch (reader.ReadByte())
            {
                case 0x21:
                    disposal = ReadExtension(reader, disposal);
                    break;
                case 0x2C:
                    frames.Add(ReadImage(reader, disposal));
                    disposal = 0;
                    break;
                case 0x3B:
                    return frames;
                default:
                    throw new InvalidDataException("The GIF contains an unexpected block marker.");
            }
        }

        throw new InvalidDataException("The GIF data stream has no trailer.");
    }

    private static void ReadLogicalScreen(BinaryReader reader)
    {
        reader.ReadUInt16();
        reader.ReadUInt16();
        var packed = reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        if ((packed & 0x80) != 0) SkipBytes(reader, ColorTableBytes(packed));
    }

    private static int ReadExtension(BinaryReader reader, int currentDisposal)
    {
        var label = reader.ReadByte();
        if (label != 0xF9)
        {
            SkipSubBlocks(reader);
            return currentDisposal;
        }

        var size = reader.ReadByte();
        if (size != 4) throw new InvalidDataException("The GIF has an invalid graphic control extension.");
        var packed = reader.ReadByte();
        reader.ReadUInt16();
        reader.ReadByte();
        if (reader.ReadByte() != 0) throw new InvalidDataException("The GIF graphic control extension is not terminated.");
        return (packed >> 2) & 0x07;
    }

    private static GifFrameDescriptor ReadImage(BinaryReader reader, int disposal)
    {
        var left = reader.ReadUInt16();
        var top = reader.ReadUInt16();
        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();
        var packed = reader.ReadByte();
        if (width == 0 || height == 0) throw new InvalidDataException("The GIF contains an empty image descriptor.");
        if ((packed & 0x80) != 0) SkipBytes(reader, ColorTableBytes(packed));
        reader.ReadByte();
        SkipSubBlocks(reader);
        return new(left, top, width, height, disposal);
    }

    private static int ColorTableBytes(byte packed) => 3 * (1 << ((packed & 0x07) + 1));

    private static void SkipSubBlocks(BinaryReader reader)
    {
        while (true)
        {
            var size = reader.ReadByte();
            if (size == 0) return;
            SkipBytes(reader, size);
        }
    }

    private static void SkipBytes(BinaryReader reader, int count)
    {
        if (reader.BaseStream.Seek(count, SeekOrigin.Current) > reader.BaseStream.Length)
            throw new EndOfStreamException("The GIF data stream ended unexpectedly.");
    }
}
