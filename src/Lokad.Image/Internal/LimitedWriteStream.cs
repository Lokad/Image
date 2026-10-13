namespace Lokad.Image.Internal;

internal sealed class LimitedWriteStream(Stream output, int maximum, CancellationToken cancellation) : Stream
{
    private long written;
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => output.CanWrite;
    public override long Length => written;
    public override long Position { get => written; set => throw new NotSupportedException(); }
    public override void Write(ReadOnlySpan<byte> data)
    {
        cancellation.ThrowIfCancellationRequested();
        if (data.Length > maximum - written) throw new ImageLimitException("Encoded output limit exceeded.");
        output.Write(data); written += data.Length;
    }
    public override void Write(byte[] data, int offset, int count) => Write(data.AsSpan(offset, count));
    public override void WriteByte(byte value)
    {
        // Stream.WriteByte's default one-element array would allocate on every
        // entropy/header byte. Keep the same span write and admission checks.
        Span<byte> data = stackalloc byte[1]; data[0] = value; Write(data);
    }
    public override void Flush() => output.Flush();
    internal bool SupportsAdaptiveJpeg(long worstEncodedBytes)
    {
        // Plain MemoryStream has no caller callbacks; other streams retain the
        // original incremental encode and failure sequence.
        if (cancellation.CanBeCanceled || output.GetType() != typeof(MemoryStream)) return false;
        var memory = (MemoryStream)output;
        return memory.CanSeek && memory.CanWrite && memory.Position <= int.MaxValue - worstEncodedBytes;
    }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
