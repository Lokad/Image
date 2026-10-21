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
        if (data.Length > maximum - written) throw new ImageLimitException(ImageLimitKind.OutputBytes, "Encoded output limit exceeded.");
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
    internal void PrepareStoredPngCapacity(long filteredBytes)
    {
        // Capacity is caller-owned output storage, not codec workspace. Keep
        // the original writes/counters and avoid geometric growth for a large
        // stored PNG. Small packets fit the first IDAT write without this help.
        if (filteredBytes < 65536 || filteredBytes > maximum - written
            || cancellation.CanBeCanceled || output.GetType() != typeof(MemoryStream)) return;
        // RFC1951 section3.2.4: at most one five-byte stored header per byte,
        // plus a final empty block; RFC1950 adds its six-byte wrapper. Include
        // PNG framing for every64KiB IDAT. This loose bound only selects the
        // optional capacity path; every original output-quota check remains.
        var worstZlib = filteredBytes * 6 + 11;
        var worstPng = worstZlib + 45 + 12 * ((worstZlib + 65535) / 65536);
        if (worstPng > maximum - written) return;
        var memory = (MemoryStream)output;
        if (!memory.CanSeek || !memory.CanWrite) return;
        // A size estimate, not a new output/admission bound. Ordinary writes
        // can still grow beyond it. Allow framing and modest stored overhead.
        var estimate = filteredBytes + filteredBytes / 200 + 1024;
        estimate += 45 + 12 * ((estimate + 65535) / 65536);
        if (estimate > Array.MaxLength - memory.Position) return;
        var capacity = (int)(memory.Position + estimate);
        if (capacity <= memory.Capacity) return;
        try { memory.Capacity = capacity; }
        // Exact MemoryStream refuses growth of a fixed buffer without changing
        // its state. Retain the original incremental write failure in that case.
        catch (NotSupportedException) { }
    }

    internal bool SupportsAdaptiveJpegPlanning(long worstEncodedBytes)
    {
        // Planning does not touch the caller stream. Only stock MemoryStream
        // needs its original position/Int32 capacity guard; do not query length,
        // position or seek support on callback/forward-only streams.
        if (output.GetType() != typeof(MemoryStream)) return true;
        var memory = (MemoryStream)output;
        return memory.CanSeek && memory.CanWrite && memory.Position <= int.MaxValue - worstEncodedBytes;
    }
    internal bool SupportsAdmittedJpegBytes(long worstEncodedBytes)
        => worstEncodedBytes > 0 && worstEncodedBytes <= maximum && !cancellation.CanBeCanceled
            && output.GetType() == typeof(MemoryStream) && SupportsAdaptiveJpegPlanning(worstEncodedBytes);

    // The adaptive JPEG caller has admitted the whole packet's upper bound.
    // Stock MemoryStream has no callbacks; keep one-byte writes and update the
    // counter only after success, including nonexpandable-stream failures.
    internal void WriteAdmittedJpegByte(byte value)
    {
        output.WriteByte(value); written++;
    }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
