namespace Lokad.Image.Internal;

/// <summary>Private encoded bytes, admitted until asynchronous delivery completes.</summary>
internal sealed class EncodedOutput : IDisposable
{
    private const int Overhead = 256;
    private const int BlockBytes = 65536;
    private const int NodeBytes = 64;
    private static readonly PixelStoragePool storage = new();
    private readonly OperationContext context;
    private ChunkStream? chunks;
    internal Stream Target => chunks ?? throw new ObjectDisposedException(nameof(EncodedOutput));

    internal EncodedOutput(OperationContext context)
    {
        this.context = context;
        chunks = new ChunkStream(context);
    }

    internal async ValueTask DeliverAsync(Stream output)
    {
        for (var chunk = chunks?.Head; chunk is not null; chunk = chunk.Next)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            // A caller can recover the rented array from ReadOnlyMemory.
            chunk.Bytes.AsSpan(chunk.Length).Clear();
            context.Cancellation.ThrowIfCancellationRequested();
            await output.WriteAsync(chunk.Bytes.AsMemory(0, chunk.Length), context.Cancellation).ConfigureAwait(false);
            context.Cancellation.ThrowIfCancellationRequested();
        }
    }

    public void Dispose()
    {
        chunks?.Dispose(); chunks = null;
    }

    private sealed class Chunk(byte[] bytes)
    {
        internal readonly byte[] Bytes = bytes;
        internal int Length;
        internal Chunk? Next;
    }

    // Only the new asynchronous entry points use this stream. Fixed encoder
    // callbacks remain synchronous, while the caller sees only WriteAsync.
    private sealed class ChunkStream(OperationContext context) : Stream
    {
        internal Chunk? Head;
        private Chunk? tail;
        private long length;
        private bool disposed;

        public override void Write(ReadOnlySpan<byte> data)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            context.Cancellation.ThrowIfCancellationRequested();
            if (data.Length > context.Limits.MaxOutputBytes - length)
                throw new ImageLimitException(ImageLimitKind.OutputBytes, "Encoded output limit exceeded.");
            while (!data.IsEmpty)
            {
                if (tail is null || tail.Length == tail.Bytes.Length)
                {
                    var overhead = NodeBytes + (Head is null ? Overhead : 0);
                    var available = Math.Max(0, context.RemainingWorkingBytes - overhead);
                    var capacity = (int)Math.Min(Math.Min(BlockBytes, context.Limits.MaxOutputBytes - length), available);
                    if (capacity == 0) throw new ImageLimitException(ImageLimitKind.WorkingBytes, "Working memory limit exceeded.");
                    context.Reserve(capacity + (long)overhead);
                    byte[]? bytes = null;
                    Chunk next;
                    try { bytes = context.RentPixels(storage, capacity); next = new Chunk(bytes); }
                    catch
                    {
                        if (bytes is not null) context.ReturnPixels(storage, bytes);
                        context.Release(capacity + (long)overhead); throw;
                    }
                    if (tail is null) Head = next; else tail.Next = next;
                    tail = next;
                }
                var count = Math.Min(data.Length, tail.Bytes.Length - tail.Length);
                data[..count].CopyTo(tail.Bytes.AsSpan(tail.Length));
                tail.Length += count; length += count; data = data[count..];
                context.Cancellation.ThrowIfCancellationRequested();
            }
        }
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
        public override void WriteByte(byte value) { Span<byte> one = stackalloc byte[1]; one[0] = value; Write(one); }
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                if (Head is not null) context.Release(Overhead);
                for (var chunk = Head; chunk is not null; chunk = chunk.Next)
                {
                    context.ReturnPixels(storage, chunk.Bytes); context.Release(chunk.Bytes.Length + (long)NodeBytes);
                }
                Head = tail = null;
            }
            base.Dispose(disposing);
        }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !disposed;
        public override long Length => length;
        public override long Position { get => length; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
