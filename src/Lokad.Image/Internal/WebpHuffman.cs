namespace Lokad.Image.Internal;

/// <summary>Owned canonical complete prefix tree, including VP8L's zero-bit single leaf.</summary>
internal sealed class WebpHuffman
{
    private readonly int[] firstCode, counts, firstSymbol;
    private readonly ushort[] symbols;
    private readonly int[] shortCodes = [];
    private readonly int single;
    public int MemoryBytes { get; }

    public WebpHuffman(ReadOnlySpan<byte> lengths, OperationContext context)
    {
        Span<int> histogram = stackalloc int[16];
        histogram.Clear();
        var total = 0; var leaf = -1;
        context.Work(lengths.Length * 16L);
        for (var i = 0; i < lengths.Length; i++)
        {
            var length = lengths[i];
            if (length > 15) throw new ImageDataException("VP8L prefix length exceeds 15.");
            if (length != 0) { histogram[length]++; total++; leaf = i; }
        }
        if (total == 0 || (total == 1 && lengths[leaf] != 1)) throw new ImageDataException("Invalid empty or single-leaf VP8L tree.");
        if (total == 1)
        {
            MemoryBytes = 64; context.Reserve(MemoryBytes);
            single = leaf; firstCode = []; counts = []; firstSymbol = []; symbols = []; return;
        }
        var remaining = 1;
        for (var length = 1; length <= 15; length++)
        {
            remaining = remaining * 2 - histogram[length];
            if (remaining < 0) throw new ImageDataException("Oversubscribed VP8L prefix tree.");
        }
        if (remaining != 0) throw new ImageDataException("Incomplete VP8L prefix tree.");
        var maximum = 15;
        while (histogram[maximum] == 0) maximum--;
        var prefixBits = 0;
        for (var length = 1; length <= 8; length++)
            if (histogram[length] != 0) { prefixBits = Math.Min(8, maximum); break; }
        var entries = prefixBits == 0 ? 0 : 1 << prefixBits;
        MemoryBytes = 320 + total * 2 + (entries == 0 ? 0 : entries * sizeof(int) + 32);
        context.Reserve(MemoryBytes);
        if (entries != 0) context.Work(entries + 8L * total);
        firstCode = new int[16]; counts = new int[16]; firstSymbol = new int[16]; symbols = new ushort[total]; single = -1;
        var code = 0; var offset = 0;
        for (var length = 1; length <= 15; length++)
        {
            firstCode[length] = code; counts[length] = histogram[length]; firstSymbol[length] = offset;
            for (var i = 0; i < lengths.Length; i++)
                if (lengths[i] == length) symbols[offset++] = (ushort)i;
            code = (code + histogram[length]) << 1;
        }
        if (entries == 0) return;
        shortCodes = new int[entries];
        // Original canonical entries from the encoded lengths (RFC 9649,
        // section 3.7). Reverse each code for VP8L's LSB-first byte fields;
        // replicate only its unconsumed suffix. Zero entries use canonical
        // traversal. All storage and bounded construction work were admitted.
        for (var length = 1; length <= prefixBits; length++)
        for (var index = 0; index < counts[length]; index++)
        {
            var value = firstCode[length] + index; var reversed = 0;
            for (var bit = 0; bit < length; bit++) { reversed = reversed * 2 + (value & 1); value >>= 1; }
            var packed = symbols[firstSymbol[length] + index] << 4 | length;
            for (var prefix = reversed; prefix < entries; prefix += 1 << length) shortCodes[prefix] = packed;
        }
    }

    public int Read(ref WebpBitReader reader)
    {
        if (single >= 0) return single;
        if (shortCodes.Length != 0 && reader.TryReadHuffman(shortCodes, out var symbol)) return symbol;
        if (reader.TryReadCanonical(firstCode, counts, firstSymbol, symbols, out var canonical)) return canonical;
        var code = 0;
        for (var length = 1; length <= 15; length++)
        {
            code = code * 2 + reader.Read(1);
            var index = code - firstCode[length];
            if ((uint)index < (uint)counts[length]) return symbols[firstSymbol[length] + index];
        }
        throw new ImageDataException("Invalid VP8L prefix code.");
    }
}
