namespace Lokad.Image.Internal;

/// <summary>Canonical JPEG Huffman tables derived from the encoded length counts.</summary>
internal sealed class JpegHuffman
{
    private readonly int[] firstCode;
    private readonly int[] counts;
    private readonly int[] firstSymbol;
    private readonly byte[] symbols;

    public JpegHuffman(ReadOnlySpan<byte> lengthCounts, ReadOnlySpan<byte> values, OperationContext context)
    {
        if (lengthCounts.Length != 16 || values.Length is < 1 or > 256)
            throw new ImageDataException("Invalid JPEG Huffman table size.");
        context.Reserve(17 * 4 * 3 + values.Length);
        firstCode = new int[17]; counts = new int[17]; firstSymbol = new int[17];
        var code = 0; var offset = 0;
        for (var length = 1; length <= 16; length++)
        {
            context.Work(1);
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
    }

    public int Read(ref JpegEntropyReader reader)
    {
        var code = 0;
        for (var length = 1; length <= 16; length++)
        {
            code = code << 1 | reader.ReadBits(1);
            var index = code - firstCode[length];
            if ((uint)index < (uint)counts[length]) return symbols[firstSymbol[length] + index];
        }
        throw new ImageDataException("Invalid JPEG Huffman code.");
    }
}
