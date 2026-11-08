using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

/// <summary>Canonical JPEG Huffman tables derived from the encoded length counts.</summary>
internal sealed partial class JpegHuffman
{
    private readonly int[] firstCode;
    private readonly int[] counts;
    private readonly int[] firstSymbol;
    private readonly byte[] symbols;
    private readonly ushort[] shortCodes;
    private readonly ushort[]? longCodes;

    public JpegHuffman(ReadOnlySpan<byte> lengthCounts, ReadOnlySpan<byte> values, OperationContext context)
    {
        if (lengthCounts.Length != 16 || values.Length is < 1 or > 256)
            throw new ImageDataException("Invalid JPEG Huffman table size.");
        context.Reserve(17 * 4 * 3 + values.Length);
        firstCode = new int[17]; counts = new int[17]; firstSymbol = new int[17];
        var code = 0; var offset = 0;
        for (var length = 1; length <= 16; length++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var count = lengthCounts[length - 1];
            // JPEG reserves the all-ones code, including at the longest length,
            // so padding can never decode to a valid symbol.
            if (code + count >= 1 << length && count != 0)
                throw new ImageDataException("Oversubscribed JPEG Huffman table or all-ones code.");
            firstCode[length] = code; counts[length] = count; firstSymbol[length] = offset;
            offset += count; code = (code + count) << 1;
        }
        if (offset != values.Length) throw new ImageDataException("JPEG Huffman counts do not match symbols.");
        symbols = values.ToArray();
        // Original eight-bit prefixes derived from this encoded canonical
        // table (T.81 C.2). Packed symbol/length entries; zero means fallback.
        // Admit all additional storage before allocating it, on every decode.
        context.Reserve(256 * sizeof(ushort)); context.Cancellation.ThrowIfCancellationRequested(); shortCodes = new ushort[256];
        for (var length = 1; length <= 8; length++)
        for (var index = 0; index < counts[length]; index++)
        {
            var prefix = (firstCode[length] + index) << (8 - length);
            var packed = (ushort)(symbols[firstSymbol[length] + index] << 4 | length);
            shortCodes.AsSpan(prefix, 1 << (8 - length)).Fill(packed);
        }
        // Only tables with nine/ten-bit codes need the additional canonical
        // lookup. Fixed short-code tables keep their original storage.
        if (counts[9] != 0 || counts[10] != 0)
        {
            context.Reserve(1024 * sizeof(ushort)); context.Cancellation.ThrowIfCancellationRequested(); longCodes = new ushort[1024];
            for (var length = 9; length <= 10; length++)
            for (var index = 0; index < counts[length]; index++)
            {
                var prefix = (firstCode[length] + index) << (10 - length);
                var packed = (ushort)(symbols[firstSymbol[length] + index] << 4 | length);
                longCodes.AsSpan(prefix, 1 << (10 - length)).Fill(packed);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ref JpegEntropyReader reader)
    {
        if (reader.TryReadHuffman(shortCodes, out var symbol)) return symbol;
        return ReadLong(ref reader);
    }

    internal bool TryReadRefinementBlock(ref JpegEntropyReader reader, Span<int> block,
        int start, int end, int low, ref int eobRun, scoped ReadOnlySpan<uint> headers = default)
        => reader.TryReadRefinementBlock(shortCodes, this, block, start, end, low, ref eobRun, headers);

    internal bool TryReadCancelableRefinementBlock(ref JpegEntropyReader reader, Span<int> block,
        int start, int end, int low, ref int eobRun, scoped ReadOnlySpan<uint> headers = default)
        => reader.TryReadBufferedCancelableRefinementBlock(shortCodes, this, block, start, end, low, ref eobRun, headers)
            || reader.TryReadCancelableRefinementBlock(shortCodes, this, block, start, end, low, ref eobRun);

    // T.81 C.2/G.1.2: compile short code plus sign/EOB interpretation from
    // this encoded canonical table. Packed bits:0..3 code length,4..8 total
    // width,9 insert,10 EOB,11..15 zero run,16..31 signed insertion factor
    // or positive EOB count. Compile sign interpretation once per scan.
    // Zero leaves long/invalid/short tails to the
    // existing reader. The caller owns and admits the scan-local destination.
    internal void BuildRefinementHeaders(Span<uint> headers, CancellationToken cancellation)
    {
        if (headers.Length != 512) throw new ArgumentException("JPEG refinement headers have 512 entries.", nameof(headers));
        for (var prefix = 0; prefix < headers.Length; prefix++)
        {
            if ((prefix & 63) == 0) cancellation.ThrowIfCancellationRequested();
            headers[prefix] = 0;
            var packed = shortCodes[prefix >> 1]; var length = packed & 15;
            var symbol = packed >> 4; var size = symbol & 15; var run = symbol >> 4;
            if (length == 0 || size > 1) continue;
            var count = size == 1 ? 1 : run == 15 ? 0 : run;
            if (length + count > 9) continue;
            var field = (prefix >> (9 - length - count)) & ((1 << count) - 1);
            var kind = size == 1 ? 512 : run == 15 ? 0 : 1024;
            var value = size == 1 ? 2 * field - 1 : kind == 1024 ? (1 << run) + field : 0;
            var skip = kind == 0 ? 16 : run;
            headers[prefix] = (uint)(length | (length + count) << 4 | kind | skip << 11 | value << 16);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryReadBaselineAc(ref JpegEntropyReader reader, int remainingCoefficients, out int symbol, out int value)
        => reader.TryReadBaselineAc(shortCodes, remainingCoefficients, out symbol, out value);

    internal void ReadPlainCancelableAc(ref JpegEntropyReader reader, scoped Span<int> block, ref int coefficient)
        => reader.ReadPlainCancelableAc(shortCodes, block, ref coefficient);

    internal void ReadPlainBaselineAc(ref JpegEntropyReader reader, scoped Span<int> block, ref int coefficient)
        => reader.ReadPlainBaselineAc(this, block, ref coefficient);

    internal bool TryReadPlainBaselineBlock(ref JpegEntropyReader reader, JpegHuffman ac,
        scoped Span<int> block, ref int predictor, ref int coefficient)
        => reader.TryReadPlainBaselineBlock(shortCodes, ac, block, ref predictor, ref coefficient);

    internal bool TryReadCancelableBaselineBlock(ref JpegEntropyReader reader, JpegHuffman ac,
        scoped Span<int> block, ref int predictor, ref int coefficient)
        => reader.TryReadCancelableBaselineBlock(shortCodes, ac.shortCodes, block, ref predictor, ref coefficient);

    // Reuse the encoded table's existing nine/ten-bit lookup in an ordinary
    // bounded entropy window. No additional table preparation or capacity.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int LookupBaselinePrefix(ulong cache, int pending)
    {
        var packed = shortCodes[(int)(cache >> 56)];
        if (packed != 0 || longCodes is null) return packed;
        packed = longCodes[(int)(cache >> 54)];
        // Ordinary windows guarantee18bits. A long prefix plus its amplitude
        // can require20; leave an incomplete field entirely unconsumed.
        return (packed & 15) + ((packed >> 4) & 15) <= pending ? packed : 0;
    }

    // Keep canonical traversal out of the common short-code wrapper so the
    // caller can inline that wrapper without its cold loop and saved registers.
    private int ReadLong(ref JpegEntropyReader reader)
    {
        if (longCodes is not null && reader.TryReadLongHuffman(longCodes, out var symbol)) return symbol;
        var code = 0; var firstLength = 1;
        if (reader.TryReadCanonicalPrefix(shortCodes, out var prefix)) { code = prefix; firstLength = 9; }
        for (var length = firstLength; length <= 16; length++)
        {
            code = code << 1 | reader.ReadBits(1);
            var index = code - firstCode[length];
            if ((uint)index < (uint)counts[length]) return symbols[firstSymbol[length] + index];
        }
        throw new ImageDataException("Invalid JPEG Huffman code.");
    }
}
