using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

/// <summary>Canonical JPEG Huffman tables derived from the encoded length counts.</summary>
internal sealed class JpegHuffman
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
        int start, int end, int low, ref int eobRun)
        => reader.TryReadRefinementBlock(shortCodes, this, block, start, end, low, ref eobRun);

    internal bool TryReadCancelableRefinementBlock(ref JpegEntropyReader reader, Span<int> block,
        int start, int end, int low, ref int eobRun)
        => reader.TryReadBufferedCancelableRefinementBlock(shortCodes, this, block, start, end, low, ref eobRun)
            || reader.TryReadCancelableRefinementBlock(shortCodes, this, block, start, end, low, ref eobRun);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryReadBaselineAc(ref JpegEntropyReader reader, int remainingCoefficients, out int symbol, out int value)
        => reader.TryReadBaselineAc(shortCodes, remainingCoefficients, out symbol, out value);

    internal void ReadPlainCancelableAc(ref JpegEntropyReader reader, scoped Span<int> block, ref int coefficient)
        => reader.ReadPlainCancelableAc(shortCodes, block, ref coefficient);

    internal void ReadPlainBaselineAc(ref JpegEntropyReader reader, scoped Span<int> block, ref int coefficient)
        => reader.ReadPlainBaselineAc(shortCodes, block, ref coefficient);

    internal bool TryReadPlainBaselineBlock(ref JpegEntropyReader reader, JpegHuffman ac,
        scoped Span<int> block, ref int predictor, ref int coefficient)
        => reader.TryReadPlainBaselineBlock(shortCodes, ac.shortCodes, block, ref predictor, ref coefficient);

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
