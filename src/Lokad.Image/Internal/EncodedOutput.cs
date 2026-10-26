namespace Lokad.Image.Internal;

/// <summary>Private encoded bytes, admitted until asynchronous delivery completes.</summary>
internal sealed class EncodedOutput : IDisposable
{
    internal const int Overhead = 256;
    private const int BlockBytes = 65536;
    private const int NodeBytes = 64;
    private static readonly PixelStoragePool storage = new();
    private readonly OperationContext context;
    private ChunkStream? chunks;
    private MemoryStream? memory;
    private byte[]? contiguous;
    internal Stream Target => (Stream?)memory ?? chunks ?? throw new ObjectDisposedException(nameof(EncodedOutput));

    private EncodedOutput(OperationContext context) => this.context = context;

    internal static EncodedOutput Segmented(OperationContext context)
    {
        var result = new EncodedOutput(context);
        result.chunks = new ChunkStream(context);
        return result;
    }

    internal static EncodedOutput Contiguous(OperationContext context, int capacity)
    {
        context.Reserve((long)capacity + Overhead);
        byte[]? bytes = null;
        try
        {
            bytes = context.RentPixels(storage, capacity);
            var memory = new MemoryStream(bytes, 0, capacity, writable: true, publiclyVisible: true);
            memory.SetLength(0);
            return new EncodedOutput(context) { memory = memory, contiguous = bytes };
        }
        catch
        {
            if (bytes is not null) context.ReturnPixels(storage, bytes);
            context.Release((long)capacity + Overhead); throw;
        }
    }

    internal async ValueTask DeliverAsync(Stream output)
    {
        if (memory is { } encoded && contiguous is { } bytes)
        {
            // Stock MemoryStream writes bypass the chunk stream. Charge that
            // completed staging pass once, as well as each delivery attempt.
            var length = checked((int)encoded.Length);
            // A caller can recover an array from ReadOnlyMemory. Never expose
            // an earlier private packet through the unused backing capacity.
            context.Cancellation.ThrowIfCancellationRequested(); bytes.AsSpan(length).Clear();
            context.Cancellation.ThrowIfCancellationRequested();
            for (var offset = 0; offset < length;)
            {
                var count = Math.Min(BlockBytes, length - offset);
                context.Cancellation.ThrowIfCancellationRequested();
                await output.WriteAsync(bytes.AsMemory(offset, count), context.Cancellation).ConfigureAwait(false);
                context.Cancellation.ThrowIfCancellationRequested(); offset += count;
            }
        }
        else
        {
            for (var chunk = chunks?.Head; chunk is not null; chunk = chunk.Next)
            {
                context.Cancellation.ThrowIfCancellationRequested(); chunk.Bytes.AsSpan(chunk.Length).Clear();
                context.Cancellation.ThrowIfCancellationRequested();
                await output.WriteAsync(chunk.Bytes.AsMemory(0, chunk.Length), context.Cancellation).ConfigureAwait(false);
                context.Cancellation.ThrowIfCancellationRequested();
            }
        }
    }

    public void Dispose()
    {
        var bytes = Interlocked.Exchange(ref contiguous, null);
        memory?.Dispose(); memory = null;
        if (bytes is not null) { context.ReturnPixels(storage, bytes); context.Release((long)bytes.Length + Overhead); }
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
