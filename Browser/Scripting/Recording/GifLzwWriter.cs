namespace CMG.Browser.Scripting.Recording;

internal static class GifLzwWriter
{
    public static void Write(BinaryWriter writer, ReadOnlySpan<byte> pixels, int minimumCodeSize)
    {
        writer.Write((byte)minimumCodeSize);
        var blocks = new GifBlockWriter(writer);
        var bits = new GifBitWriter(blocks);
        var clear = 1 << minimumCodeSize;
        var end = clear + 1;
        var next = end + 1;
        var codeSize = minimumCodeSize + 1;
        var dictionary = new Dictionary<int, int>(4096);
        bits.Write(clear, codeSize);
        if (pixels.Length > 0)
        {
            var prefix = (int)pixels[0];
            for (var index = 1; index < pixels.Length; index++)
            {
                var suffix = pixels[index];
                var key = (prefix << 8) | suffix;
                if (dictionary.TryGetValue(key, out var code))
                {
                    prefix = code;
                    continue;
                }
                bits.Write(prefix, codeSize);
                if (next < 4096)
                {
                    dictionary[key] = next++;
                    if (next > 1 << codeSize && codeSize < 12) codeSize++;
                }
                else
                {
                    bits.Write(clear, codeSize);
                    dictionary.Clear();
                    codeSize = minimumCodeSize + 1;
                    next = end + 1;
                }
                prefix = suffix;
            }
            bits.Write(prefix, codeSize);
        }
        bits.Write(end, codeSize);
        bits.Finish();
        blocks.Finish();
    }

    private sealed class GifBitWriter(GifBlockWriter output)
    {
        private int buffer;
        private int count;

        public void Write(int code, int bits)
        {
            buffer |= code << count;
            count += bits;
            while (count >= 8)
            {
                output.Write((byte)(buffer & 0xFF));
                buffer >>= 8;
                count -= 8;
            }
        }

        public void Finish()
        {
            if (count > 0) output.Write((byte)(buffer & 0xFF));
            buffer = 0;
            count = 0;
        }
    }

    private sealed class GifBlockWriter(BinaryWriter writer)
    {
        private readonly byte[] block = new byte[255];
        private int count;

        public void Write(byte value)
        {
            block[count++] = value;
            if (count == block.Length) Flush();
        }

        public void Finish()
        {
            Flush();
            writer.Write((byte)0);
        }

        private void Flush()
        {
            if (count == 0) return;
            writer.Write((byte)count);
            writer.Write(block, 0, count);
            count = 0;
        }
    }
}
