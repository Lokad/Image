namespace Lokad.Image.Internal;

/// <summary>Independent bounded canonical prefix encoder for RFC 9649 section 3.7.</summary>
internal sealed class WebpPrefixCode
{
    private readonly byte[] lengths;
    private readonly ushort[] codes;
    private readonly int single;
    internal int MemoryBytes { get; }
    internal int Alphabet => lengths.Length;
    internal int Length(int symbol) => lengths[symbol];
    internal int Code(int symbol) => codes[symbol];

    internal WebpPrefixCode(ReadOnlySpan<int> frequencies, OperationContext context, int maximumLength = 15)
    {
        if (frequencies.Length is < 1 or > 2328 || maximumLength is < 1 or > 15)
            throw new ArgumentOutOfRangeException(nameof(frequencies));
        var used = 0; var first = 0;
        for (var symbol = 0; symbol < frequencies.Length; symbol++)
        {
            context.Work(1);
            if (frequencies[symbol] < 0) throw new ArgumentOutOfRangeException(nameof(frequencies));
            if (frequencies[symbol] != 0) { used++; first = symbol; }
        }
        if (used > 1 << maximumLength) throw new ArgumentOutOfRangeException(nameof(maximumLength));
        if (used == 0) { used = 1; first = 0; }
        single = used == 1 ? first : -1;
        MemoryBytes = checked(frequencies.Length * 3 + 128);
        context.Reserve(MemoryBytes);
        context.Work(0);
        lengths = new byte[frequencies.Length]; codes = new ushort[frequencies.Length];
        if (single >= 0) { lengths[single] = 1; return; }

        var nodes = frequencies.Length * 2 - 1;
        var scratchBytes = checked(nodes * 12 + frequencies.Length * 4 + 256);
        context.Reserve(scratchBytes);
        try
        {
            context.Work(0);
            var weights = new long[nodes]; var parents = new int[nodes]; var heap = new int[frequencies.Length];
            long floor = 0;
            while (true)
            {
                context.Work(nodes + (long)frequencies.Length);
                parents.AsSpan().Fill(-1); lengths.AsSpan().Clear();
                var size = 0;
                for (var symbol = 0; symbol < frequencies.Length; symbol++)
                {
                    if (frequencies[symbol] == 0) continue;
                    weights[symbol] = Math.Max(frequencies[symbol], floor); heap[size++] = symbol;
                }
                for (var i = size / 2 - 1; i >= 0; i--) Down(heap, weights, size, i, context);
                var next = frequencies.Length;
                while (size > 1)
                {
                    var a = Pop(heap, weights, ref size, context); var b = Pop(heap, weights, ref size, context);
                    context.Work(1); weights[next] = weights[a] + weights[b]; parents[a] = next; parents[b] = next;
                    Push(heap, weights, ref size, next++, context);
                }
                var maximum = 0;
                for (var symbol = 0; symbol < frequencies.Length; symbol++)
                {
                    if (frequencies[symbol] == 0) continue;
                    var depth = 0; var node = symbol;
                    while (parents[node] >= 0) { context.Work(1); depth++; node = parents[node]; }
                    // A very skewed unrestricted tree can exceed byte depth.
                    // Keep its actual maximum before choosing the bounded retry.
                    maximum = Math.Max(maximum, depth);
                    if (depth <= maximumLength) lengths[symbol] = (byte)depth;
                }
                if (maximum <= maximumLength) break;
                // Raising only positive weights eventually produces a balanced
                // complete tree. This is bounded numerical construction, never
                // a retry following memory refusal or allocation failure.
                floor = floor == 0 ? 1 : checked(floor * 2);
            }
        }
        finally { context.Release(scratchBytes); }

        Span<int> histogram = stackalloc int[16]; Span<int> nextCode = stackalloc int[16];
        histogram.Clear(); nextCode.Clear();
        foreach (var length in lengths) if (length != 0) histogram[length]++;
        var code = 0;
        for (var length = 1; length < 16; length++)
        { code = (code + histogram[length - 1]) << 1; nextCode[length] = code; }
        for (var symbol = 0; symbol < lengths.Length; symbol++)
        {
            var count = lengths[symbol]; if (count == 0) continue;
            context.Work(count); codes[symbol] = Reverse(nextCode[count]++, count);
        }
    }

    internal void Write(WebpBitWriter writer, int symbol)
    {
        if ((uint)symbol >= (uint)lengths.Length || lengths[symbol] == 0)
            throw new ArgumentOutOfRangeException(nameof(symbol));
        writer.Write(single >= 0 ? 0 : codes[symbol], single >= 0 ? 0 : lengths[symbol]);
    }

    internal void Header(WebpBitWriter writer, OperationContext context)
    {
        // Simple trees store only byte-sized symbol values. Larger green/cache
        // symbols require the normal code-length representation even alone.
        var first = -1; var second = -1; var leaves = 0;
        for (var symbol = 0; symbol < lengths.Length; symbol++)
        {
            context.Work(1);
            if (lengths[symbol] == 0) continue;
            if (leaves == 0) first = symbol; else if (leaves == 1) second = symbol;
            leaves++;
        }
        if (leaves <= 2 && Math.Max(first, second) < 256)
        {
            writer.Write(1, 1); writer.Write(leaves - 1, 1); writer.Write(first > 1 ? 1 : 0, 1);
            writer.Write(first, first > 1 ? 8 : 1);
            if (leaves == 2) writer.Write(second, 8);
            return;
        }

        var tokenBytes = checked(lengths.Length * 4 + 32); context.Reserve(tokenBytes);
        try
        {
            context.Work(0);
            var tokens = new int[lengths.Length]; var count = 0; var position = 0;
            Span<int> frequencies = stackalloc int[19]; frequencies.Clear();
            context.Work(lengths.Length * 2L);
            while (position < lengths.Length)
            {
                var value = lengths[position]; var run = 1;
                while (position + run < lengths.Length && lengths[position + run] == value) run++;
                position += run;
                if (value == 0)
                {
                    while (run >= 11)
                    { var n = Math.Min(run, 138); Token(tokens, frequencies, ref count, 18, n - 11, 7); run -= n; }
                    if (run >= 3)
                    { var n = Math.Min(run, 10); Token(tokens, frequencies, ref count, 17, n - 3, 3); run -= n; }
                    while (run-- > 0) Token(tokens, frequencies, ref count, 0, 0, 0);
                }
                else
                {
                    Token(tokens, frequencies, ref count, value, 0, 0); run--;
                    while (run >= 3)
                    { var n = Math.Min(run, 6); Token(tokens, frequencies, ref count, 16, n - 3, 2); run -= n; }
                    while (run-- > 0) Token(tokens, frequencies, ref count, value, 0, 0);
                }
            }
            var tree = new WebpPrefixCode(frequencies, context, 7);
            try
            {
                // Normative order, RFC9649 section3.7.2.1.2; not an upstream table.
                ReadOnlySpan<byte> order = [17, 18, 0, 1, 2, 3, 4, 5, 16, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
                var codeCount = 4;
                for (var i = 0; i < order.Length; i++) if (tree.lengths[order[i]] != 0) codeCount = Math.Max(codeCount, i + 1);
                writer.Write(0, 1); writer.Write(codeCount - 4, 4);
                for (var i = 0; i < codeCount; i++) writer.Write(tree.lengths[order[i]], 3);
                writer.Write(0, 1); // Full alphabet; zero runs compact its unused tail.
                for (var i = 0; i < count; i++)
                {
                    var token = tokens[i]; tree.Write(writer, token & 31);
                    writer.Write(token >> 8, token >> 5 & 7);
                }
            }
            finally { context.Release(tree.MemoryBytes); }
        }
        finally { context.Release(tokenBytes); }
    }

    private static void Token(Span<int> tokens, Span<int> frequencies, ref int count, int symbol, int extra, int bits)
    { tokens[count++] = symbol | bits << 5 | extra << 8; frequencies[symbol]++; }

    private static ushort Reverse(int value, int count)
    {
        var result = 0;
        for (var i = 0; i < count; i++) { result = result * 2 + (value & 1); value >>= 1; }
        return (ushort)result;
    }

    private static bool Less(int a, int b, long[] weights, OperationContext context)
    { context.Work(1); return weights[a] < weights[b] || weights[a] == weights[b] && a < b; }

    private static void Down(int[] heap, long[] weights, int size, int index, OperationContext context)
    {
        var node = heap[index];
        while (index * 2 + 1 < size)
        {
            var child = index * 2 + 1;
            if (child + 1 < size && Less(heap[child + 1], heap[child], weights, context)) child++;
            if (!Less(heap[child], node, weights, context)) break;
            heap[index] = heap[child]; index = child;
        }
        heap[index] = node;
    }

    private static int Pop(int[] heap, long[] weights, ref int size, OperationContext context)
    {
        var node = heap[0]; heap[0] = heap[--size];
        if (size != 0) Down(heap, weights, size, 0, context);
        return node;
    }

    private static void Push(int[] heap, long[] weights, ref int size, int node, OperationContext context)
    {
        var index = size++;
        while (index > 0)
        {
            var parent = (index - 1) / 2;
            if (!Less(node, heap[parent], weights, context)) break;
            heap[index] = heap[parent]; index = parent;
        }
        heap[index] = node;
    }
}
