using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Lokad.Image.Internal;

/// <summary>Fixed-space container acquisition; byte admission includes read-ahead and skipped payloads.</summary>
internal class InspectionReader : OperationContext, IDisposable
{
    internal const int WorkingBytes = 8192 + 256;
    private const int HeaderBytes = 32;
    private readonly Stream input;
    private readonly bool asynchronous;
    private byte[] buffer;
    private readonly bool adaptiveHeaders;
    private readonly bool directFields;
    private bool bulk;
    internal readonly byte[] Header;
    internal OperationContext Context => this;
    private int first, last;
    private long acquired;
    private readonly long? maxAcquiredBytes;
    internal long BytesRead { get; private set; }
    private bool disposed;
    internal long Position => acquired - (last - first);
    internal long BytesTraversed => acquired;

    internal InspectionReader(Stream input, ImageLimits limits, CancellationToken cancellation, bool asynchronous,
        long? maxAcquiredBytes = null, bool dimensionsOnly = false, bool adaptiveHeaders = false)
        : base(limits, cancellation)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(input);
            this.input = input; this.asynchronous = asynchronous; this.maxAcquiredBytes = maxAcquiredBytes;
            this.adaptiveHeaders = adaptiveHeaders && input.CanSeek && !dimensionsOnly;
            directFields = dimensionsOnly && maxAcquiredBytes.HasValue;
            // The acquisition owner also owns its admission scope. No context or
            // workspace escapes a failed constructor, including reserve refusal.
            var capacity = dimensionsOnly || this.adaptiveHeaders ? 256 : 8192;
            Reserve(capacity + 256);
            buffer = new byte[capacity]; Header = directFields ? buffer : new byte[HeaderBytes];
        }
        catch { base.Dispose(); throw; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask<bool> FillAsync(int needed, int offset = 0)
    {
        Context.Cancellation.ThrowIfCancellationRequested();
        if (adaptiveHeaders && bulk && buffer.Length == 256) PrepareBulkStorage();
        int requested;
        if (maxAcquiredBytes is { } maximum)
        {
            if (BytesRead == maximum) throw new ImageLimitException(ImageLimitKind.AcquiredBytes, "Probe acquisition limit exceeded.");
            // Every bounded caller already fits needed bytes at this offset.
            requested = (int)Math.Min(needed, Math.Min(Context.Limits.MaxInputBytes - acquired + 1, maximum - BytesRead));
        }
        else requested = (int)Math.Min(buffer.Length - offset, Context.Limits.MaxInputBytes - acquired + 1);
        first = last = offset;
        int read;
        if (!asynchronous) read = input.Read(buffer.AsSpan(offset, requested));
        else
        {
            var pending = input.ReadAsync(buffer.AsMemory(offset, requested), Context.Cancellation);
            if (!pending.IsCompletedSuccessfully) return AwaitFillAsync(pending, requested);
            read = pending.Result;
        }
        return ValueTask.FromResult(CompleteFill(read, requested));
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CompleteFill(int read, int requested)
    {
        if ((uint)read > requested) throw new ImageDataException("Stream returned an invalid read count.");
        BytesRead += read;
        if (read > Context.Limits.MaxInputBytes - acquired)
            throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
        Context.Cancellation.ThrowIfCancellationRequested(); acquired += read; last = first + read;
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
        if ((uint)count > HeaderBytes) throw new ArgumentOutOfRangeException(nameof(count));
        var copied = 0;
        while (copied < count)
        {
            if (first == last)
            {
                var pending = FillAsync(count - copied, directFields ? copied : 0);
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
            pending = FillAsync(count - copied, directFields ? copied : 0);
        }
    }

    private int CopyHeader(int remaining, int offset)
    {
        var n = Math.Min(remaining, last - first);
        // Dimension reads acquire exactly the needed field bytes, with no unread
        // prefetch between fields. Assemble partial reads in the same workspace.
        if (!directFields) buffer.AsSpan(first, n).CopyTo(Header.AsSpan(offset));
        first += n; return n;
    }

    internal ValueTask SkipAsync(long count, bool nextJpegMarker = false)
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
        bulk = true;
        // Only this bounded JPEG probe consumes the following marker immediately.
        // A live token retains the original boundary between the two acquisitions.
        var mergeMarker = nextJpegMarker && directFields && asynchronous && !Context.Cancellation.CanBeCanceled;
        while (count != 0)
        {
            var pending = FillAsync(SkipReadSize(count, mergeMarker));
            if (!pending.IsCompletedSuccessfully) return AwaitSkipForwardAsync(count, pending, mergeMarker);
            if (!pending.Result) throw new ImageDataException("Truncated image payload.");
            var n = (int)Math.Min(count, last); first = n; count -= n;
        }
        return ValueTask.CompletedTask;
    }

    private int SkipReadSize(long count, bool mergeMarker)
    {
        if (mergeMarker && count < buffer.Length)
        {
            // Match the following marker read and its encoded-input sentinel.
            var markerBytes = Context.Limits.MaxInputBytes - acquired - count >= 2 ? 2 : 1;
            if (count + markerBytes <= buffer.Length) return (int)count + markerBytes;
        }
        return (int)Math.Min(count, buffer.Length);
    }

    private async ValueTask AwaitSkipForwardAsync(long count, ValueTask<bool> pending, bool mergeMarker)
    {
        while (true)
        {
            if (!await pending.ConfigureAwait(false)) throw new ImageDataException("Truncated image payload.");
            var n = (int)Math.Min(count, last); first = n; count -= n;
            if (count == 0) return;
            pending = FillAsync(SkipReadSize(count, mergeMarker));
        }
    }

    // Only complete ordinary prefix/code pairs bypass async setup. Filler,
    // fragmented reads and every error still use the shared marker parser.
    internal ValueTask<int> JpegHeaderMarkerAsync()
    {
        if (first == last)
        {
            var pending = FillAsync(Context.Limits.MaxInputBytes - acquired >= 2 ? 2 : 1);
            if (!pending.IsCompletedSuccessfully) return AwaitJpegHeaderMarkerAsync(pending);
            if (!pending.Result) throw new ImageDataException("Invalid JPEG marker.");
        }
        if (last - first < 2 || buffer[first] != 255 || buffer[first + 1] == 255)
            return JpegMarkerAsync(false);
        var marker = buffer[first + 1]; first += 2;
        if (marker == 217) bulk = false;
        return ValueTask.FromResult((int)marker);
    }

    private async ValueTask<int> AwaitJpegHeaderMarkerAsync(ValueTask<bool> pending)
    {
        if (!await pending.ConfigureAwait(false)) throw new ImageDataException("Invalid JPEG marker.");
        return await JpegHeaderMarkerAsync().ConfigureAwait(false);
    }

    internal async ValueTask<int> JpegMarkerAsync(bool entropy)
    {
    Scan:
        if (entropy)
        {
            bulk = true;
            while (true)
            {
                if (first == last && !await FillAsync(buffer.Length).ConfigureAwait(false)) throw new ImageDataException("JPEG is missing EOI.");
                var n = EntropyMarkerPrefix(buffer.AsSpan(first, last - first));
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
        do { marker = first != last ? buffer[first++] : await ByteAsync().ConfigureAwait(false); } while (marker == 255);
        if (marker < 0) throw new ImageDataException("Truncated JPEG marker.");
        // Stuffed FF00 is scan data, not a container item. Continue within this
        // acquisition; real/restart markers still reach the outer grammar.
        if (entropy && marker == 0) goto Scan;
        // A small complete JPEG may fit the initial read. Its final EOF probe
        // needs no entropy-scan scratch when that buffer has just been drained.
        if (marker == 217) bulk = false;
        return marker;
    }

    // Skip complete FF00 data pairs in batches. A final FF, filler prefix or
    // real marker returns to the shared parser at its original byte position.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int EntropyMarkerPrefix(ReadOnlySpan<byte> value)
    {
        var offset = 0;
        if (Vector256.IsHardwareAccelerated)
        {
            ref var start = ref MemoryMarshal.GetReference(value);
            var prefix = Vector256.Create((byte)255);
            while (value.Length - offset >= Vector256<byte>.Count)
            {
                var data = Vector256.LoadUnsafe(ref start, (nuint)offset);
                var matches = Vector256.ExtractMostSignificantBits(Vector256.Equals(data, prefix));
                if (matches != 0)
                {
                    var zeros = Vector256.ExtractMostSignificantBits(Vector256.Equals(data, Vector256<byte>.Zero));
                    // Bit i of zeros >> 1 describes the byte after prefix i.
                    // The final lane is deliberately returned for suffix parsing.
                    var markers = matches & ~(zeros >> 1);
                    if (markers != 0) return offset + BitOperations.TrailingZeroCount(markers);
                }
                offset += Vector256<byte>.Count;
            }
        }
        while (offset < value.Length)
        {
            var n = value[offset..].IndexOf((byte)255);
            if (n < 0) return -1;
            offset += n;
            if (offset + 1 == value.Length || value[offset + 1] != 0) return offset;
            offset += 2;
        }
        return -1;
    }

    internal async ValueTask EndAsync()
    {
        if (await ByteAsync().ConfigureAwait(false) >= 0) throw new ImageDataException("Image has trailing bytes.");
    }

    internal void DeclaredEnd(long end)
    {
        if (end > Context.Limits.MaxInputBytes) throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
    }

    public new void Dispose()
    {
        if (disposed) return;
        disposed = true;
        // Without a shared budget, Reserve never creates a scope/reservation.
        // Finish local accounting without disposing an empty shared owner.
        if (HasMemoryBudget) DisposeAdmitted();
        else Release(buffer.Length + 256);
    }

    private void DisposeAdmitted()
    {
        try { Release(buffer.Length + 256); }
        finally { base.Dispose(); }
    }
}
