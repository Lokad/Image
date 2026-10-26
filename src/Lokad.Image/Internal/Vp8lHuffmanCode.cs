namespace Lokad.Image.Internal;

// Canonical words borrow no source population. Lengths and words are independent
// of the histogram's lifetime and remain admitted until this owner is disposed.
internal sealed class Vp8lHuffmanCode : IDisposable
{
    private readonly OperationContext context;
    private Vp8lHuffmanLengths? lengths;
    private ushort[]? words;
    private readonly bool single;
    internal ReadOnlySpan<byte> Lengths => (lengths ?? throw new ObjectDisposedException(nameof(Vp8lHuffmanCode))).Values;
    private Vp8lHuffmanCode(OperationContext context, Vp8lHuffmanLengths lengths, ushort[] words, bool single)
    { this.context = context; this.lengths = lengths; this.words = words; this.single = single; }

    internal static Vp8lHuffmanCode Build(ReadOnlySpan<int> populations, OperationContext context, int maximum = 15)
    {
        var lengths = Vp8lHuffmanLengths.Build(populations, context, maximum); long reserved = 0;
        try
        {
            context.Reserve(populations.Length * 2L); reserved = populations.Length * 2L; context.Cancellation.ThrowIfCancellationRequested();
            var words = new ushort[populations.Length];
            Span<int> counts = stackalloc int[16]; counts.Clear();
            Span<int> next = stackalloc int[16]; next.Clear();
            var nonzero = 0;
            foreach (var length in lengths.Values) { counts[length]++; if (length != 0) nonzero++; }
            var code = 0;
            for (var depth = 1; depth < 16; depth++)
            { code = (code + (depth == 1 ? 0 : counts[depth - 1])) << 1; next[depth] = code; }
            for (var symbol = 0; symbol < populations.Length; symbol++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var depth = lengths.Values[symbol]; if (depth == 0) continue;
                var value = next[depth]++; var reversed = 0;
                for (var i = 0; i < depth; i++) { reversed = reversed * 2 + (value & 1); value >>= 1; }
                words[symbol] = (ushort)reversed;
            }
            return new Vp8lHuffmanCode(context, lengths, words, nonzero <= 1);
        }
        catch { lengths.Dispose(); if (reserved != 0) context.Release(reserved); throw; }
    }

    internal void Write(WebpBitWriter writer, int symbol)
    {
        var storage = words ?? throw new ObjectDisposedException(nameof(Vp8lHuffmanCode));
        var length = Lengths[symbol];
        if (length == 0) throw new InvalidOperationException("Symbol is absent from its Huffman alphabet.");
        // The stored length remains one for a single-symbol tree, while data
        // consumes no bits (RFC 9649 section 5.2.1).
        writer.Write(storage[symbol], single ? 0 : length);
    }

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref words, null);
        if (owned is not null) context.Release(owned.Length * 2L);
        Interlocked.Exchange(ref lengths, null)?.Dispose();
    }
}
