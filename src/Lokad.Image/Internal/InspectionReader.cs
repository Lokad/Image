namespace Lokad.Image.Internal;

/// <summary>Fixed-space container acquisition; byte admission includes read-ahead and skipped payloads.</summary>
internal sealed class InspectionReader : IDisposable
{
    internal const int WorkingBytes = 8192 + 256;
    private readonly Stream input;
    private readonly bool asynchronous;
    private byte[] buffer;
    private readonly bool adaptiveHeaders;
    private bool bulk;
    internal readonly byte[] Header;
    internal readonly OperationContext Context;
    private int first, last;
    private long acquired;
    private readonly long? maxAcquiredBytes;
    internal long BytesRead { get; private set; }
    private bool disposed;
    internal long Position => acquired - (last - first);
    internal long BytesTraversed => acquired;

    internal InspectionReader(Stream input, OperationContext context, bool asynchronous, long? maxAcquiredBytes = null,
        bool dimensionsOnly = false, bool adaptiveHeaders = false)
    {
        this.input = input; Context = context; this.asynchronous = asynchronous; this.maxAcquiredBytes = maxAcquiredBytes;
        this.adaptiveHeaders = adaptiveHeaders && input.CanSeek && !dimensionsOnly;
        // Seekable container inspection skips opaque payloads. Acquire bulk scratch
        // only if a later scan actually needs it; probes retain their small buffer.
        var capacity = dimensionsOnly || this.adaptiveHeaders ? 256 : 8192;
        context.Reserve(capacity + 256);
        try { buffer = new byte[capacity]; Header = new byte[32]; }
        catch { context.Release(capacity + 256); throw; }
    }

    private ValueTask<bool> FillAsync(int needed)
    {
        Context.Cancellation.ThrowIfCancellationRequested();
        if (adaptiveHeaders && bulk && buffer.Length == 256) PrepareBulkStorage();
        var requested = (int)Math.Min(buffer.Length, Context.Limits.MaxInputBytes - acquired + 1);
        if (maxAcquiredBytes is { } maximum)
        {
            if (BytesRead == maximum) throw new ImageLimitException(ImageLimitKind.AcquiredBytes, "Probe acquisition limit exceeded.");
            requested = (int)Math.Min(Math.Min(requested, needed), maximum - BytesRead);
        }
        if (!asynchronous) return ValueTask.FromResult(CompleteFill(input.Read(buffer.AsSpan(0, requested)), requested));
        var pending = input.ReadAsync(buffer.AsMemory(0, requested), Context.Cancellation);
        return pending.IsCompletedSuccessfully ? ValueTask.FromResult(CompleteFill(pending.Result, requested)) : AwaitFillAsync(pending, requested);
    }

    private void PrepareBulkStorage()
    {
        // Fill is called only after the previous buffer and any asynchronous read
        // have finished. No unread bytes or live users survive this replacement.
        buffer = [];
        Context.Release(256);
        Context.Reserve(8192);
        try
        {
            Context.Cancellation.ThrowIfCancellationRequested();
            buffer = new byte[8192];
        }
        catch { Context.Release(8192); throw; }
    }

    private async ValueTask<bool> AwaitFillAsync(ValueTask<int> pending, int requested)
        => CompleteFill(await pending.ConfigureAwait(false), requested);

    private bool CompleteFill(int read, int requested)
    {
        if ((uint)read > requested) throw new ImageDataException("Stream returned an invalid read count.");
        BytesRead += read;
        if (read > Context.Limits.MaxInputBytes - acquired)
            throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
        Context.Cancellation.ThrowIfCancellationRequested(); acquired += read; first = 0; last = read;
        return read != 0;
    }

    internal ValueTask<int> ByteAsync()
    {
        if (first != last) return ValueTask.FromResult((int)buffer[first++]);
        var pending = FillAsync(1);
        return pending.IsCompletedSuccessfully ? ValueTask.FromResult(pending.Result ? buffer[first++] : -1) : AwaitByteAsync(pending);
    }

    private async ValueTask<int> AwaitByteAsync(ValueTask<bool> pending)
        => await pending.ConfigureAwait(false) ? buffer[first++] : -1;

    internal ValueTask ReadAsync(int count)
    {
        if ((uint)count > Header.Length) throw new ArgumentOutOfRangeException(nameof(count));
        var copied = 0;
        while (copied < count)
        {
            if (first == last)
            {
                var pending = FillAsync(count - copied);
                if (!pending.IsCompletedSuccessfully) return AwaitReadAsync(count, copied, pending);
                if (!pending.Result) throw new ImageDataException("Truncated image header.");
            }
            copied += CopyHeader(count - copied, copied);
        }
        return ValueTask.CompletedTask;
    }

    private async ValueTask AwaitReadAsync(int count, int copied, ValueTask<bool> pending)
    {
        while (true)
        {
            if (!await pending.ConfigureAwait(false)) throw new ImageDataException("Truncated image header.");
            copied += CopyHeader(count - copied, copied);
            if (copied == count) return;
            pending = FillAsync(count - copied);
        }
    }

    private int CopyHeader(int remaining, int offset)
    {
        var n = Math.Min(remaining, last - first);
        buffer.AsSpan(first, n).CopyTo(Header.AsSpan(offset)); first += n; return n;
    }

    internal ValueTask SkipAsync(long count)
    {
        if (count < 0) throw new ImageDataException("Invalid image payload length.");
        Context.Cancellation.ThrowIfCancellationRequested();
        var buffered = (int)Math.Min(count, last - first); first += buffered; count -= buffered;
        if (count == 0) return ValueTask.CompletedTask;
        if (count > Context.Limits.MaxInputBytes - acquired)
            throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
        if (input.CanSeek)
        {
            var position = input.Position; var length = input.Length;
            if (position < 0 || length < position || count > length - position) throw new ImageDataException("Truncated image payload.");
            Context.Cancellation.ThrowIfCancellationRequested();
            if (input.Seek(count, SeekOrigin.Current) != position + count) throw new ImageDataException("Invalid stream seek result.");
            acquired += count; Context.Cancellation.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }
        return SkipForwardAsync(count);
    }

    private async ValueTask SkipForwardAsync(long count)
    {
        bulk = true;
        while (count != 0)
        {
            if (!await FillAsync((int)Math.Min(count, buffer.Length)).ConfigureAwait(false)) throw new ImageDataException("Truncated image payload.");
            var n = (int)Math.Min(count, last); first = n; count -= n;
        }
    }

    internal async ValueTask<int> JpegMarkerAsync(bool entropy)
    {
        if (entropy)
        {
            bulk = true;
            while (true)
            {
                if (first == last && !await FillAsync(buffer.Length).ConfigureAwait(false)) throw new ImageDataException("JPEG is missing EOI.");
                var n = buffer.AsSpan(first, last - first).IndexOf((byte)255);
                if (n < 0) { first = last; continue; }
                first += n + 1; break;
            }
        }
        else
        {
            // Probe reads can acquire the natural prefix/code pair together.
            // Keep a lone allowed byte ahead of the input-limit sentinel.
            if (first == last && !await FillAsync(Context.Limits.MaxInputBytes - acquired >= 2 ? 2 : 1).ConfigureAwait(false))
                throw new ImageDataException("Invalid JPEG marker.");
            if (buffer[first++] != 255) throw new ImageDataException("Invalid JPEG marker.");
        }
        int marker;
        do { marker = await ByteAsync().ConfigureAwait(false); } while (marker == 255);
        if (marker < 0) throw new ImageDataException("Truncated JPEG marker.");
        // A small complete JPEG may fit the initial read. Its final EOF probe
        // needs no entropy-scan scratch when that buffer has just been drained.
        if (marker == 217) bulk = false;
        return marker;
    }

    internal async ValueTask EndAsync()
    {
        if (await ByteAsync().ConfigureAwait(false) >= 0) throw new ImageDataException("Image has trailing bytes.");
    }

    internal void DeclaredEnd(long end)
    {
        if (end > Context.Limits.MaxInputBytes) throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
    }

    public void Dispose() { if (!disposed) { disposed = true; Context.Release(buffer.Length + 256); } }
}
