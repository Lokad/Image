namespace Lokad.Image.Internal;

/// <summary>Fixed-space container acquisition; byte admission includes read-ahead and skipped payloads.</summary>
internal sealed class InspectionReader : IDisposable
{
    internal const int WorkingBytes = 8192 + 256;
    private readonly Stream input;
    private readonly bool asynchronous;
    private readonly byte[] buffer;
    internal readonly byte[] Header;
    internal readonly OperationContext Context;
    private int first, last;
    private long acquired;
    private bool disposed;
    internal long Position => acquired - (last - first);

    internal InspectionReader(Stream input, OperationContext context, bool asynchronous)
    {
        this.input = input; Context = context; this.asynchronous = asynchronous;
        context.Reserve(WorkingBytes);
        try { buffer = new byte[8192]; Header = new byte[32]; }
        catch { context.Release(WorkingBytes); throw; }
    }

    private async ValueTask<bool> FillAsync()
    {
        Context.Cancellation.ThrowIfCancellationRequested();
        var requested = (int)Math.Min(buffer.Length, Context.Limits.MaxInputBytes - acquired + 1);
        var read = asynchronous
            ? await input.ReadAsync(buffer.AsMemory(0, requested), Context.Cancellation).ConfigureAwait(false)
            : input.Read(buffer.AsSpan(0, requested));
        if ((uint)read > requested) throw new ImageDataException("Stream returned an invalid read count.");
        if (read > Context.Limits.MaxInputBytes - acquired)
            throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
        Context.Cancellation.ThrowIfCancellationRequested(); acquired += read; first = 0; last = read;
        return read != 0;
    }

    internal async ValueTask<int> ByteAsync()
    {
        if (first == last && !await FillAsync().ConfigureAwait(false)) return -1;
        return buffer[first++];
    }

    internal async ValueTask ReadAsync(int count)
    {
        if ((uint)count > Header.Length) throw new ArgumentOutOfRangeException(nameof(count));
        var copied = 0;
        while (copied < count)
        {
            if (first == last && !await FillAsync().ConfigureAwait(false)) throw new ImageDataException("Truncated image header.");
            var n = Math.Min(count - copied, last - first);
            buffer.AsSpan(first, n).CopyTo(Header.AsSpan(copied)); first += n; copied += n;
        }
    }

    internal async ValueTask SkipAsync(long count)
    {
        if (count < 0) throw new ImageDataException("Invalid image payload length.");
        Context.Cancellation.ThrowIfCancellationRequested();
        var buffered = (int)Math.Min(count, last - first); first += buffered; count -= buffered;
        if (count == 0) return;
        if (count > Context.Limits.MaxInputBytes - acquired)
            throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
        if (input.CanSeek)
        {
            var position = input.Position; var length = input.Length;
            if (position < 0 || length < position || count > length - position) throw new ImageDataException("Truncated image payload.");
            Context.Cancellation.ThrowIfCancellationRequested();
            if (input.Seek(count, SeekOrigin.Current) != position + count) throw new ImageDataException("Invalid stream seek result.");
            acquired += count; Context.Cancellation.ThrowIfCancellationRequested();
        }
        else
        {
            while (count != 0)
            {
                if (!await FillAsync().ConfigureAwait(false)) throw new ImageDataException("Truncated image payload.");
                var n = (int)Math.Min(count, last); first = n; count -= n;
            }
        }
    }

    internal async ValueTask<int> JpegMarkerAsync(bool entropy)
    {
        if (entropy)
        {
            while (true)
            {
                if (first == last && !await FillAsync().ConfigureAwait(false)) throw new ImageDataException("JPEG is missing EOI.");
                var n = buffer.AsSpan(first, last - first).IndexOf((byte)255);
                if (n < 0) { first = last; continue; }
                first += n + 1; break;
            }
        }
        else if (await ByteAsync().ConfigureAwait(false) != 255) throw new ImageDataException("Invalid JPEG marker.");
        int marker;
        do { marker = await ByteAsync().ConfigureAwait(false); } while (marker == 255);
        if (marker < 0) throw new ImageDataException("Truncated JPEG marker.");
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

    public void Dispose() { if (!disposed) { disposed = true; Context.Release(WorkingBytes); } }
}
