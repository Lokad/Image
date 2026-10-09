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
    public override void Flush() => output.Flush();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
