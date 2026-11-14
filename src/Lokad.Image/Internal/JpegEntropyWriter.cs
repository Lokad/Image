using System;
using System.IO;
using System.Threading;

namespace Lokad.Image.Internal;

// Borrows output/context. Lazily admits its bounded byte buffer through the
// same operation root and releases it before that context ends. Failure disposal
// drops pending bytes; only explicit alignment publishes a partial final chunk.
internal sealed class JpegEntropyWriter(Stream output, OperationContext context) : IDisposable
{
    internal const int Capacity = 512;
    internal const long StorageBytes = Capacity + 128;
    private byte[]? buffer;
    private int used;
    // A smaller first full chunk avoids oversized initial growth in caller
    // streams after small JPEG headers. Partial restart flushes keep this bound.
    private int flushAt = Capacity / 2;
    // At most seven pending bits plus two 16-bit entropy groups are live.
    private ulong bits;
    private int available;
    private bool disposed;

    private void Emit(byte value)
    {
        if (buffer is null)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            context.Reserve(StorageBytes);
            try { context.Cancellation.ThrowIfCancellationRequested(); buffer = new byte[Capacity]; }
            catch { context.Release(StorageBytes); throw; }
        }
        buffer[used++] = value;
        if (used == flushAt) { Flush(); flushAt = Capacity; }
    }

    private void Flush()
    {
        if (used == 0) return;
        context.Cancellation.ThrowIfCancellationRequested();
        output.Write(buffer.AsSpan(0, used));
        used = 0;
    }

    public void Write(int value, int count)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (count is < 0 or > 16 || value < 0 || value >= 1 << count) throw new ArgumentOutOfRangeException(nameof(value));
        context.Cancellation.ThrowIfCancellationRequested();
        Append((uint)value, count, ref bits, ref available);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void Append(ulong value, int count, ref ulong bits, ref int available)
    {
        bits = bits << count | value; available += count;
        while (available >= 8)
        {
            available -= 8;
            var next = (byte)(bits >> available);
            Emit(next);
            if (next == 255) Emit(0);
        }
    }

    internal void Replay(ReadOnlySpan<int> records, ReadOnlySpan<uint> bank)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        // Production owns this writer privately, replays once and abandons it
        // on failure. Publish the reservoir only after successful replay;
        // buffered output and its admission still use their actual lifetimes.
        var pending = bits; var pendingCount = available; var offset = 0;
        while (offset < records.Length)
        {
            context.Cancellation.ThrowIfCancellationRequested(); uint record;
            do
            {
                record = (uint)records[offset++]; var packed = bank[(int)(record >> 16 & 1023)];
                var length = (int)(packed >> 16); var code = (int)(packed & 65535);
                if (length == 0) throw new ImageDataException("JPEG source changed between encoding passes.");
                var amplitudeCount = (int)(record >> 27);
                var amplitude = amplitudeCount == 31 ? 0 : (int)(record & 65535);
                if (amplitudeCount == 31) amplitudeCount = 0;
                if (length > 16 || code >= 1 << length) throw new ArgumentOutOfRangeException(nameof(bank));
                if (amplitudeCount > 16 || amplitude >= 1 << amplitudeCount) throw new ArgumentOutOfRangeException(nameof(records));
                context.Cancellation.ThrowIfCancellationRequested();
                Append((ulong)(uint)code << amplitudeCount | (uint)amplitude, length + amplitudeCount, ref pending, ref pendingCount);
            } while ((record & JpegEntropyRecords.EndMcuBit) == 0);
        }
        bits = pending; available = pendingCount;
    }

    internal void WriteBlock(ReadOnlySpan<short> coefficients, int component, Span<int> previous, JpegEncodingTable[] tables)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        // Like replay, direct encoding owns this writer privately and abandons
        // failed state. Keep the reservoir local through a complete block.
        var pending = bits; var pendingCount = available;
        var family = component == 0 ? 0 : 1;
        var dc = tables[family]; var ac = tables[family + 2];
        var acCodes = ac.Codes; var acLengths = ac.Lengths;
        var difference = coefficients[0] - previous[component]; previous[component] = coefficients[0];
        Coefficient(difference, 0, dc.Codes, dc.Lengths, ref pending, ref pendingCount);
        if (System.Runtime.Intrinsics.X86.Avx512BW.IsSupported)
        {
            var mask = JpegNonzeroMask.Ac(coefficients); var position = 0;
            while (mask != 0)
            {
                var next = System.Numerics.BitOperations.TrailingZeroCount(mask); mask &= mask - 1;
                var zeros = next - position - 1;
                while (zeros >= 16) { Code(240, 0, 0, acCodes, acLengths, ref pending, ref pendingCount); zeros -= 16; }
                Coefficient(coefficients[JpegTransform.Zigzag[next]], zeros, acCodes, acLengths, ref pending, ref pendingCount);
                position = next;
            }
            if (position != 63) Code(0, 0, 0, acCodes, acLengths, ref pending, ref pendingCount);
        }
        else
        {
            var run = 0;
            for (var k = 1; k < 64; k++)
            {
                var value = coefficients[JpegTransform.Zigzag[k]];
                if (value == 0) { run++; continue; }
                while (run >= 16) { Code(240, 0, 0, acCodes, acLengths, ref pending, ref pendingCount); run -= 16; }
                Coefficient(value, run, acCodes, acLengths, ref pending, ref pendingCount); run = 0;
            }
            if (run != 0) Code(0, 0, 0, acCodes, acLengths, ref pending, ref pendingCount);
        }
        bits = pending; available = pendingCount;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void Coefficient(int value, int run, ushort[] codes, byte[] lengths, ref ulong pending, ref int pendingCount)
    {
        // Only Int16 AC values or differences of two private Int16 DC values
        // reach this helper. Their magnitude fits16 bits, excluding Int32.MinValue.
        System.Diagnostics.Debug.Assert(value is >= -65535 and <= 65535);
        var sign = value >> 31; var magnitude = (value ^ sign) - sign;
        var count = 32 - System.Numerics.BitOperations.LeadingZeroCount((uint)magnitude);
        var amplitude = value + (sign & ((1 << count) - 1));
        Code(run * 16 + count, amplitude, count, codes, lengths, ref pending, ref pendingCount);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void Code(int symbol, int amplitude, int amplitudeCount, ushort[] codes, byte[] lengths, ref ulong pending, ref int pendingCount)
    {
        var length = lengths[symbol];
        if (length == 0) throw new ImageDataException("JPEG source changed between encoding passes.");
        var code = codes[symbol];
        if (length > 16 || code >= 1 << length) throw new ArgumentOutOfRangeException(nameof(codes));
        if (amplitudeCount > 16 || amplitude < 0 || amplitude >= 1 << amplitudeCount) throw new ArgumentOutOfRangeException(nameof(amplitude));
        context.Cancellation.ThrowIfCancellationRequested();
        Append((ulong)code << amplitudeCount | (uint)amplitude, length + amplitudeCount, ref pending, ref pendingCount);
    }

    public void Align()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (available != 0) Write((1 << (8 - available)) - 1, 8 - available);
        bits = 0; available = 0;
        Flush();
    }

    public void Restart(int index)
    {
        Align(); output.WriteByte(255); output.WriteByte((byte)(0xD0 + (index & 7)));
    }

    public void Dispose()
    {
        // Failure disposal drops pending bytes, performs no output I/O and
        // releases only owned storage. Output/context/root remain borrowed.
        disposed = true; used = 0;
        if (Interlocked.Exchange(ref buffer, null) is not null) context.Release(StorageBytes);
    }
}
