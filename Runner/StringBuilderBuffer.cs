using System.Buffers;
using System.Text;

namespace CMG.Runner;

internal sealed class StringBuilderBuffer : IBufferWriter<byte>
{
    private readonly StringBuilder builder;
    private byte[] buffer = new byte[1024];

    public StringBuilderBuffer(StringBuilder builder)
    {
        this.builder = builder;
    }

    public void Advance(int count)
    {
        builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return buffer;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        Ensure(sizeHint);
        return buffer;
    }

    private void Ensure(int sizeHint)
    {
        if (sizeHint > buffer.Length)
        {
            buffer = new byte[sizeHint];
        }
    }
}
