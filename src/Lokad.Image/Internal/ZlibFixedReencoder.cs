using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

// Independent RFC1951 3.2.5..3.2.7 entropy recoding. Input is a private BCL
// zlib stream; match finding, literal bytes, lengths, distances and Adler stay.
internal static class ZlibFixedReencoder
{
    internal const int WorkspaceBytes = 32768;
    internal const int TableWork = 131072;
    internal static long RequiredWork(long encodedBytes) => encodedBytes * 16 + TableWork;
    private static readonly ushort[] Fixed = BuildFixed();

    internal static bool TryRecode(ReadOnlySpan<byte> input, Span<byte> output, int decodedBytes,
        OperationContext context, out int written)
    {
        written = 0;
        if (context.Cancellation.CanBeCanceled || input.Length < 8 || decodedBytes < 0
            || input.Overlaps(output) || input[0] != 0x78 || (input[1] & 32) != 0
            || ((input[0] << 8) | input[1]) % 31 != 0
            || context.RemainingWork < RequiredWork(input.Length)) return false;
        Span<int> literals = stackalloc int[286], distances = stackalloc int[30];
        literals.Clear(); distances.Clear();
        var raw = input[2..^4]; var reader = new Reader(raw); long extraBits = 0;
        context.Work(input.Length * 8L);
        if (!Scan(ref reader, decodedBytes, literals, distances, ref extraBits)) return false;
        literals[256] = 1; // Merge fixed blocks; retain exactly one final EOB.
        context.Work(TableWork);
        Span<byte> ll = stackalloc byte[286], dd = stackalloc byte[30];
        if (!BuildLengths(literals, ll) || !BuildLengths(distances, dd)) return false;
        var nl = 286; while (nl > 257 && ll[nl - 1] == 0) nl--;
        var nd = 30; while (nd > 1 && dd[nd - 1] == 0) nd--;
        long bits = 3 + 5 + 5 + 4 + 19 * 3 + 4 * (nl + nd) + extraBits;
        for (var i = 0; i < 286; i++) bits += (long)literals[i] * ll[i];
        for (var i = 0; i < 30; i++) bits += (long)distances[i] * dd[i];
        var bytes = (bits + 7) / 8 + 6;
        if (bytes >= input.Length || bytes > output.Length) return false;
        Span<ushort> lc = stackalloc ushort[286], dc = stackalloc ushort[30];
        Codes(ll, lc); Codes(dd, dc);
        context.Work(input.Length * 8L);
        var writer = new Writer(output.Slice(2, (int)bytes - 6));
        writer.Put(1, 1); writer.Put(2, 2); writer.Put(nl - 257, 5); writer.Put(nd - 1, 5); writer.Put(15, 4);
        // A complete four-bit alphabet for length values 0..15 avoids a third
        // adaptive tree. Repeat symbols 16..18 are absent (RFC1951 3.2.7).
        ReadOnlySpan<byte> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
        foreach (var symbol in order) writer.Put(symbol <= 15 ? 4 : 0, 3);
        for (var i = 0; i < nl; i++) writer.Put(Reverse(ll[i], 4), 4);
        for (var i = 0; i < nd; i++) writer.Put(Reverse(dd[i], 4), 4);
        reader = new Reader(raw); var final = false;
        while (!final)
        {
            reader.Read(1, out var flag); final = flag != 0; reader.Read(2, out _);
            while (true)
            {
                reader.Symbol(out var symbol);
                if (symbol == 256) break;
                writer.Put(lc[symbol], ll[symbol]);
                if (symbol < 256) continue;
                Length(symbol, out _, out var width); reader.Read(width, out var value); writer.Put(value, width);
                reader.Read(5, out var code); var distance = Reverse(code, 5);
                writer.Put(dc[distance], dd[distance]);
                Distance(distance, out _, out width); reader.Read(width, out value); writer.Put(value, width);
            }
        }
        writer.Put(lc[256], ll[256]); writer.Finish();
        if (writer.Count != bytes - 6) throw new InvalidOperationException("Deflate bit estimate differs.");
        input[..2].CopyTo(output); input[^4..].CopyTo(output.Slice((int)bytes - 4, 4));
        written = (int)bytes; return true;
    }

    private static bool Scan(ref Reader reader, int expected, scoped Span<int> ll, scoped Span<int> dd, ref long extraBits)
    {
        var decoded = 0; var final = false;
        while (!final)
        {
            if (!reader.Read(1, out var flag) || !reader.Read(2, out var type) || type != 1) return false;
            final = flag != 0;
            while (true)
            {
                if (!reader.Symbol(out var symbol) || symbol > 285) return false;
                ll[symbol]++;
                if (symbol == 256) break;
                if (symbol < 256) { if (decoded == expected) return false; decoded++; continue; }
                Length(symbol, out var length, out var width);
                if (!reader.Read(width, out var value)) return false;
                length += value; extraBits += width;
                if (!reader.Read(5, out var code)) return false;
                var distance = Reverse(code, 5); if (distance >= 30) return false;
                Distance(distance, out var offset, out width);
                if (!reader.Read(width, out value) || offset + value > decoded || length > expected - decoded) return false;
                extraBits += width; dd[distance]++; decoded += length;
            }
        }
        return decoded == expected && reader.ConsumedBytes == reader.Length;
    }

    internal static bool BuildLengths(ReadOnlySpan<int> frequencies, Span<byte> lengths)
    {
        if (frequencies.Length is < 1 or > 286 || lengths.Length != frequencies.Length) throw new ArgumentException("Invalid Huffman alphabet.");
        lengths.Clear(); var symbols = frequencies.Length;
        Span<long> weights = stackalloc long[572]; Span<int> parents = stackalloc int[572], heap = stackalloc int[286];
        parents.Fill(-1); var count = 0;
        for (var i = 0; i < symbols; i++)
        {
            if (frequencies[i] < 0) return false;
            if (frequencies[i] != 0) { weights[i] = frequencies[i]; Push(heap, ref count, i, weights); }
        }
        if (count == 0) { lengths[0] = 1; return true; }
        var next = symbols;
        while (count > 1)
        {
            var a = Pop(heap, ref count, weights); var b = Pop(heap, ref count, weights);
            weights[next] = weights[a] + weights[b]; parents[a] = parents[b] = next;
            Push(heap, ref count, next++, weights);
        }
        for (var i = 0; i < symbols; i++)
        {
            if (frequencies[i] == 0) continue;
            var depth = 0; for (var node = i; parents[node] >= 0; node = parents[node]) depth++;
            if (depth > 15) return false; // Original stream is always available.
            lengths[i] = (byte)Math.Max(1, depth);
        }
        return true;
    }

    private static bool Before(int a, int b, ReadOnlySpan<long> weights)
        => weights[a] < weights[b] || (weights[a] == weights[b] && a < b);
    private static void Push(Span<int> heap, ref int count, int node, ReadOnlySpan<long> weights)
    {
        var p = count++;
        while (p > 0 && Before(node, heap[(p - 1) / 2], weights)) { heap[p] = heap[(p - 1) / 2]; p = (p - 1) / 2; }
        heap[p] = node;
    }
    private static int Pop(Span<int> heap, ref int count, ReadOnlySpan<long> weights)
    {
        var result = heap[0]; var last = heap[--count]; var p = 0;
        while (p * 2 + 1 < count)
        {
            var child = p * 2 + 1;
            if (child + 1 < count && Before(heap[child + 1], heap[child], weights)) child++;
            if (!Before(heap[child], last, weights)) break;
            heap[p] = heap[child]; p = child;
        }
        if (count != 0) heap[p] = last;
        return result;
    }
    private static void Codes(ReadOnlySpan<byte> lengths, Span<ushort> codes)
    {
        Span<int> counts = stackalloc int[16], next = stackalloc int[16]; counts.Clear(); next.Clear();
        foreach (var length in lengths) if (length != 0) counts[length]++;
        var code = 0;
        for (var i = 1; i <= 15; i++) { code = (code + counts[i - 1]) << 1; next[i] = code; }
        for (var i = 0; i < lengths.Length; i++) codes[i] = lengths[i] == 0 ? (ushort)0 : (ushort)Reverse(next[lengths[i]]++, lengths[i]);
    }
    private static ushort[] BuildFixed()
    {
        var table = new ushort[512]; Span<byte> lengths = stackalloc byte[288]; Span<ushort> codes = stackalloc ushort[288];
        lengths[..144].Fill(8); lengths[144..256].Fill(9); lengths[256..280].Fill(7); lengths[280..].Fill(8); Codes(lengths, codes);
        for (var symbol = 0; symbol < 288; symbol++)
            for (var suffix = 0; suffix < 1 << (9 - lengths[symbol]); suffix++)
                table[codes[symbol] | suffix << lengths[symbol]] = (ushort)(symbol << 4 | lengths[symbol]);
        return table;
    }
    private static int Reverse(int value, int count)
    {
        var bits = (uint)value;
        bits = (bits & 0x5555u) << 1 | (bits >> 1 & 0x5555u);
        bits = (bits & 0x3333u) << 2 | (bits >> 2 & 0x3333u);
        bits = (bits & 0x0F0Fu) << 4 | (bits >> 4 & 0x0F0Fu);
        bits = (bits & 255u) << 8 | bits >> 8;
        return (int)(bits >> (16 - count));
    }
    private static void Length(int symbol, out int value, out int width)
    {
        if (symbol < 265) { value = symbol - 254; width = 0; }
        else if (symbol == 285) { value = 258; width = 0; }
        else { width = 1 + (symbol - 265) / 4; value = ((4 + (symbol - 265) % 4) << width) + 3; }
    }
    private static void Distance(int symbol, out int value, out int width)
    {
        width = symbol < 4 ? 0 : symbol / 2 - 1;
        value = symbol < 4 ? symbol + 1 : ((2 + symbol % 2) << width) + 1;
    }
    private ref struct Reader
    {
        private readonly ReadOnlySpan<byte> data;
        private uint bits; private int available, position;
        internal Reader(ReadOnlySpan<byte> data) { this.data = data; bits = 0; available = position = 0; }
        internal int Length => data.Length;
        internal int ConsumedBytes => position - available / 8;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Read(int count, out int value)
        {
            while (available < count && position < data.Length) { bits |= (uint)data[position++] << available; available += 8; }
            value = (int)(bits & ((1u << count) - 1));
            if (available < count) return false;
            bits >>= count; available -= count; return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Symbol(out int symbol)
        {
            while (available < 9 && position < data.Length) { bits |= (uint)data[position++] << available; available += 8; }
            var packed = Fixed[bits & 511]; var count = packed & 15; symbol = packed >> 4;
            if (available < count) return false;
            bits >>= count; available -= count; return true;
        }
    }
    private ref struct Writer
    {
        private readonly Span<byte> data;
        private uint bits; private int available;
        internal int Count { get; private set; }
        internal Writer(Span<byte> data) { this.data = data; bits = 0; available = 0; Count = 0; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Put(int value, int count)
        {
            bits |= (uint)value << available; available += count;
            while (available >= 8) { data[Count++] = (byte)bits; bits >>= 8; available -= 8; }
        }
        internal void Finish() { if (available != 0) data[Count++] = (byte)bits; }
    }
}
