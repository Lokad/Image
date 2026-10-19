namespace Lokad.Image.Tooling;

// Original resident, read-only source. Only asynchronous reads are permitted;
// both the managed and native benchmark callbacks complete immediately.
internal sealed class NonSeekableAsyncInput(byte[] source) : Stream
{
    private int position;
    private bool disposed;
    internal int AsyncReads { get; private set; }
    internal int EofReads { get; private set; }
    internal int SyncReads { get; private set; }
    internal int MetadataQueries { get; private set; }
    internal int BytesRead => position;
    internal bool Disposed => disposed;
    public override bool CanRead => !disposed;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length { get { MetadataQueries++; throw new NotSupportedException(); } }
    public override long Position
    {
        get { MetadataQueries++; throw new NotSupportedException(); }
        set { MetadataQueries++; throw new NotSupportedException(); }
    }
    public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this); cancellationToken.ThrowIfCancellationRequested(); AsyncReads++;
        var count = Math.Min(destination.Length, source.Length - position);
        source.AsSpan(position, count).CopyTo(destination.Span); position += count;
        if (count == 0) EofReads++;
        return ValueTask.FromResult(count);
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override int Read(byte[] buffer, int offset, int count) { SyncReads++; throw new InvalidOperationException("Synchronous read was requested."); }
    public override int Read(Span<byte> buffer) { SyncReads++; throw new InvalidOperationException("Synchronous read was requested."); }
    public override long Seek(long offset, SeekOrigin origin) { MetadataQueries++; throw new NotSupportedException(); }
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Flush() => throw new NotSupportedException();
    protected override void Dispose(bool disposing) { disposed = true; base.Dispose(disposing); }
}
