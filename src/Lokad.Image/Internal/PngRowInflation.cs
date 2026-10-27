namespace Lokad.Image.Internal;

// Private, speculative output of the framing walk. RFC 1951 history remains
// filtered and in wire byte order; reconstructed rows never overwrite it.
// A 64-KiB buffer retains the preceding 32 KiB when it slides. Row/filter and
// checksum decisions are published only after framing, or replayed by the
// original reader when this output cannot certify the complete raster. Row
// reconstruction remains after its per-row cancellation checkpoint.
internal sealed class PngRowInflation
{
    internal const int MaxRows = 65536;
    internal const int HistoryBytes = 65536;
    private readonly ImageBuffer? result;
    private readonly byte[] history;
    private readonly byte[] filters, previous;
    private readonly int rowBytes, rowCount;
    private int position, consumed, rowOffset = -1, rowIndex, filter;
    private uint adlerA = 1, adlerB;
    private bool valid = true;

    public PngRowInflation(ImageBuffer result)
    {
        this.result = result; rowBytes = result.Stride; rowCount = result.Height;
        history = new byte[HistoryBytes];
        filters = new byte[result.Height]; previous = new byte[result.Stride];
    }

    // The operation owns and admits this history; no raster or row copies escape.
    public PngRowInflation(int rowBytes, int rowCount, byte[] history)
    {
        if (history.Length != HistoryBytes) throw new ArgumentException("Expected bounded Deflate history.", nameof(history));
        this.rowBytes = rowBytes; this.rowCount = rowCount; this.history = history;
        filters = previous = [];
    }

    public Span<byte> Writable
    {
        get { EnsureSpace(); return history.AsSpan(position); }
    }

    public void Advance(int count) => position += count;

    public void Literal(byte value)
    {
        EnsureSpace(); history[position++] = value;
    }

    public void Match(int length, int distance)
    {
        EnsureSpace();
        if (distance == 1) history.AsSpan(position, length).Fill(history[position - 1]);
        else
        {
            var copied = Math.Min(length, distance);
            history.AsSpan(position - distance, copied).CopyTo(history.AsSpan(position, copied));
            while (copied < length)
            {
                var count = Math.Min(copied, length - copied);
                history.AsSpan(position, count).CopyTo(history.AsSpan(position + copied, count));
                copied += count;
            }
        }
        position += length;
    }

    public void Stored(ReadOnlySpan<byte> bytes)
    {
        while (!bytes.IsEmpty)
        {
            var writable = Writable; var count = Math.Min(writable.Length, bytes.Length);
            bytes[..count].CopyTo(writable); Advance(count); bytes = bytes[count..];
        }
    }

    private void EnsureSpace()
    {
        // A Deflate match has at most 258 bytes. Keep it contiguous, including
        // overlapping copies at the largest declared history distance.
        if (history.Length - position >= 258) return;
        Consume();
        history.AsSpan(position - 32768, 32768).CopyTo(history);
        position = consumed = 32768;
    }

    public bool Finish(uint checksum)
    {
        Consume();
        return valid && rowIndex == rowCount && rowOffset == -1
            && (adlerB << 16 | adlerA) == checksum;
    }

    public void DecodeRows(OperationContext context)
    {
        var result = this.result ?? throw new InvalidOperationException("Validation has no retained raster.");
        for (var y = 0; y < result.Height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var row = result.Pixels.Slice(y * result.Stride, result.Stride);
            ReadOnlySpan<byte> prior = result.BitDepth == 8 && y > 0
                ? result.Pixels.Slice((y - 1) * result.Stride, result.Stride) : previous;
            PngCodec.Unfilter(row, prior, result.BitDepth / 2, filters[y], context.Cancellation);
            // Word prediction needs the preceding row in wire byte order,
            // before in-place conversion to native-endian public samples.
            if (result.BitDepth == 16) PngCodec.CopyRgbaRow(row, previous, false, context.Cancellation);
            PngCodec.CopyRgbaRow(row, row,
                result.BitDepth == 16 && BitConverter.IsLittleEndian, context.Cancellation);
        }
    }

    private void Consume()
    {
        var bytes = history.AsSpan(consumed, position - consumed);
        consumed = position;
        if (!valid) return;
        Adler32.Append(bytes, ref adlerA, ref adlerB);
        while (!bytes.IsEmpty)
        {
            if (rowOffset == -1)
            {
                filter = bytes[0]; bytes = bytes[1..]; rowOffset = 0;
                if (filter > 4 || rowIndex >= rowCount) { valid = false; return; }
                if (result is not null) filters[rowIndex] = (byte)filter;
            }
            var count = Math.Min(rowBytes - rowOffset, bytes.Length);
            if (result is not null) bytes[..count].CopyTo(result.Pixels.Slice(rowIndex * rowBytes + rowOffset, count));
            bytes = bytes[count..]; rowOffset += count;
            if (rowOffset != rowBytes) continue;
            rowOffset = -1; rowIndex++;
        }
    }
}
