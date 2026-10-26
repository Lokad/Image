namespace Lokad.Image.Internal;

// Independent private record format: payload16/symbol8/table2/MCU-end1/count5.
// Count31 means no amplitude write; count0 preserves a zero-bit DC write.
internal sealed partial class JpegEntropyRecords : IDisposable
{
    private const long MetadataBytes = 128;
    private readonly OperationContext context;
    private readonly int maximumEntries;
    private int[]? storage;
    private int count;
    private long reserved;
    internal int Count => count;
    internal ReadOnlySpan<int> Values => GetStorage().AsSpan(0, count);

    internal JpegEntropyRecords(OperationContext context, int initialEntries)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialEntries);
        if (initialEntries > Array.MaxLength / 2)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "JPEG replay capacity exceeds the array limit.");
        this.context = context; maximumEntries = initialEntries * 2;
        var bytes = initialEntries * 4L + MetadataBytes;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(bytes); reserved = bytes;
        try { context.Cancellation.ThrowIfCancellationRequested(); storage = context.RentCoefficients(initialEntries); }
        catch { reserved = 0; context.Release(bytes); throw; }
    }

    private int[] GetStorage() => storage ?? throw new ObjectDisposedException(nameof(JpegEntropyRecords));

    internal Span<int> BeginBlock()
    {
        var owned = GetStorage();
        if (count > maximumEntries - 64)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "JPEG replay exceeds its record bound.");
        if (owned.Length - count < 64) Grow(owned);
        return GetStorage().AsSpan(count, 64);
    }

    internal void Commit(int records)
    {
        var owned = GetStorage();
        if ((uint)records > 64 || records > owned.Length - count)
            throw new ArgumentOutOfRangeException(nameof(records));
        count += records;
    }

    internal void MarkMcuEnd()
    {
        var owned = GetStorage();
        if (count == 0) throw new InvalidOperationException("An MCU must contain entropy records.");
        owned[count - 1] |= (int)EndMcuBit;
    }

    private void Grow(int[] old)
    {
        var bytes = maximumEntries * 4L;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(bytes);
        int[]? replacement = null;
        try
        {
            context.Cancellation.ThrowIfCancellationRequested(); replacement = context.RentCoefficients(maximumEntries);
            // At most one growth, with <=32 copied records per frame block.
            // Record storage is covered by the admitted capacity, including growth overlap.
            context.Cancellation.ThrowIfCancellationRequested(); old.AsSpan(0, count).CopyTo(replacement);
        }
        catch
        {
            if (replacement is not null) context.ReturnCoefficients(replacement);
            context.Release(bytes); throw;
        }
        storage = replacement; reserved += bytes;
        context.ReturnCoefficients(old); context.Release(old.Length * 4L); reserved -= old.Length * 4L;
    }

    internal void Write(JpegEncodingTable[] tables, JpegEntropyWriter writer)
        => WriteRecords(Values, tables, writer, context);

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref storage, null);
        if (owned is null) return;
        count = 0;
        try { context.ReturnCoefficients(owned); }
        finally { context.Release(Interlocked.Exchange(ref reserved, 0)); }
    }

    internal const uint EndMcuBit = 1U << 26;
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void Symbol(int table, int symbol, int[][] histograms, Span<int> target, ref int count)
    {
        histograms[table][symbol]++;
        target[count++] = unchecked((int)((uint)symbol << 16 | (uint)table << 24 | 31U << 27));
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void Coefficient(int table, int run, int value, int[][] histograms, Span<int> target, ref int count)
    {
        var bits = value == 0 ? 0 : System.Numerics.BitOperations.Log2((uint)Math.Abs(value)) + 1;
        var symbol = run * 16 + bits; histograms[table][symbol]++;
        var payload = value < 0 ? value + ((1 << bits) - 1) : value;
        target[count++] = unchecked((int)((uint)payload | (uint)symbol << 16 | (uint)table << 24 | (uint)bits << 27));
    }
    internal static int RecordBlock(ReadOnlySpan<short> coefficients, int component, Span<int> previous, int[][] histograms, Span<int> target)
    {
        var count = 0; var family = component == 0 ? 0 : 1;
        var difference = coefficients[0] - previous[component]; previous[component] = coefficients[0];
        Coefficient(family, 0, difference, histograms, target, ref count);
        if (System.Runtime.Intrinsics.X86.Avx512BW.IsSupported)
        {
            var mask = JpegNonzeroMask.Ac(coefficients); var position = 0;
            // Only the arithmetic is batched. Scalar emission preserves every
            // histogram/record write, including partial exceptional prefixes.
            var inputBytes = System.Runtime.InteropServices.MemoryMarshal.AsBytes(coefficients[..64]);
            var acHistogram = histograms[family + 2];
            var independent = !inputBytes.Overlaps(System.Runtime.InteropServices.MemoryMarshal.AsBytes(target))
                && !inputBytes.Overlaps(System.Runtime.InteropServices.MemoryMarshal.AsBytes(acHistogram.AsSpan()));
            if (System.Runtime.Intrinsics.X86.Avx512F.IsSupported && System.Runtime.Intrinsics.X86.Avx512CD.IsSupported && System.Numerics.BitOperations.PopCount(mask) >= 16 && independent)
            {
                Span<int> prepared = stackalloc int[64]; PrepareRecordValues(coefficients, prepared);
                while (mask != 0)
                {
                    var next = System.Numerics.BitOperations.TrailingZeroCount(mask); mask &= mask - 1;
                    var zeros = next - position - 1;
                    while (zeros >= 16) { Symbol(family + 2, 240, histograms, target, ref count); zeros -= 16; }
                    var packed = (uint)prepared[JpegTransform.Zigzag[next]];
                    var bits = (int)(packed >> 27); var symbol = zeros * 16 + bits;
                    acHistogram[symbol]++;
                    target[count++] = unchecked((int)((packed + (uint)(zeros << 20)) | (uint)(family + 2) << 24));
                    position = next;
                }
                if (position != 63) Symbol(family + 2, 0, histograms, target, ref count);
                return count;
            }
            while (mask != 0)
            {
                var next = System.Numerics.BitOperations.TrailingZeroCount(mask); mask &= mask - 1;
                var zeros = next - position - 1;
                while (zeros >= 16) { Symbol(family + 2, 240, histograms, target, ref count); zeros -= 16; }
                Coefficient(family + 2, zeros, coefficients[JpegTransform.Zigzag[next]], histograms, target, ref count);
                position = next;
            }
            if (position != 63) Symbol(family + 2, 0, histograms, target, ref count);
            return count;
        }
        var run = 0;
        for (var k = 1; k < 64; k++)
        {
            var value = coefficients[JpegTransform.Zigzag[k]];
            if (value == 0) { run++; continue; }
            while (run >= 16) { Symbol(family + 2, 240, histograms, target, ref count); run -= 16; }
            Coefficient(family + 2, run, value, histograms, target, ref count); run = 0;
        }
        if (run != 0) Symbol(family + 2, 0, histograms, target, ref count);
        return count;
    }

    internal static void WriteRecords(ReadOnlySpan<int> records, JpegEncodingTable[] tables, JpegEntropyWriter writer, OperationContext context)
    {
        var offset = 0;
        while (offset < records.Length)
        {
            context.Cancellation.ThrowIfCancellationRequested(); uint record;
            do
            {
                record = (uint)records[offset++]; var table = tables[(int)(record >> 24 & 3)];
                var symbol = (int)(record >> 16 & 255); var length = table.Lengths[symbol];
                if (length == 0) throw new ImageDataException("JPEG source changed between encoding passes.");
                var bits = (int)(record >> 27);
                if (bits == 31) writer.Write(table.Codes[symbol], length);
                else writer.WritePair(table.Codes[symbol], length, (int)(record & 65535), bits);
            } while ((record & EndMcuBit) == 0);
        }
    }
}
