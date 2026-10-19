namespace Lokad.Image.Tooling;

// Original caller-owned resident sink. Only asynchronous writes are permitted;
// callbacks copy immediately into independent growing output storage.
internal sealed class NonSeekableAsyncOutput : Stream
{
    private readonly MemoryStream storage = new();
    internal int AsyncWrites { get; private set; }
    internal int SyncWrites { get; private set; }
    internal int MetadataQueries { get; private set; }
    internal int AsyncFlushes { get; private set; }
    internal bool Disposed { get; private set; }
    internal int FailOnWrite { get; init; }
    internal Action? AfterWrite { get; init; }
    internal int Count => checked((int)storage.Length);
    internal ReadOnlySpan<byte> Written => storage.GetBuffer().AsSpan(0, Count);
    internal byte[] Snapshot() => storage.ToArray();
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => !Disposed;
    public override long Length { get { MetadataQueries++; throw new NotSupportedException(); } }
    public override long Position
    {
        get { MetadataQueries++; throw new NotSupportedException(); }
        set { MetadataQueries++; throw new NotSupportedException(); }
    }
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this); cancellationToken.ThrowIfCancellationRequested(); AsyncWrites++;
        if (AsyncWrites == FailOnWrite) throw new IOException("Original sink failure control.");
        storage.Write(buffer.Span); AfterWrite?.Invoke(); return ValueTask.CompletedTask;
    }
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(Disposed, this); cancellationToken.ThrowIfCancellationRequested(); AsyncFlushes++; return Task.CompletedTask;
    }
    public override void Write(byte[] buffer, int offset, int count) { SyncWrites++; throw new InvalidOperationException("Synchronous write requested."); }
    public override void Write(ReadOnlySpan<byte> buffer) { SyncWrites++; throw new InvalidOperationException("Synchronous write requested."); }
    public override void Flush() { SyncWrites++; throw new InvalidOperationException("Synchronous flush requested."); }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) { MetadataQueries++; throw new NotSupportedException(); }
    public override void SetLength(long value) { MetadataQueries++; throw new NotSupportedException(); }
    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed) { Disposed = true; storage.Dispose(); }
        base.Dispose(disposing);
    }
}
