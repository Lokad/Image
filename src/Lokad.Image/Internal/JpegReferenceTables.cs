// Independently implemented table generation for the pinned JPEG encoder profile.
// Normative format data: ITU-T H.262 (02/2000), 6.3.11, default intra matrix.
// https://www.itu.int/rec/dologin_pub.asp?id=T-REC-H.262-200002-S!!PDF-E&lang=s&type=items
// No upstream implementation tables or source are incorporated.
namespace Lokad.Image.Internal;

internal static class JpegReferenceTables
{
    public static ushort[] Quantization(int qscale)
    {
        if (qscale < 1 || qscale > 31) throw new ArgumentOutOfRangeException(nameof(qscale));
        ReadOnlySpan<byte> normative = new byte[]
        {
            8,16,19,22,26,27,29,34,
            16,16,22,24,27,29,34,37,
            19,22,26,27,29,34,34,38,
            22,22,26,27,29,34,37,40,
            22,26,27,29,32,35,40,48,
            26,27,29,32,35,40,48,58,
            26,27,29,34,38,46,56,69,
            27,29,35,38,46,56,69,83
        };
        var result = new ushort[64]; result[0] = 8;
        for (int i = 1; i < 64; i++) result[i] = (ushort)Math.Min(255, normative[i] * qscale / 8);
        return result;
    }

    private readonly record struct Symbol(int Value, long Weight);
    private sealed record Package(long Weight, int Leaf, Package? Left, Package? Right);

    internal const int HuffmanWork = 131072;

    internal static JpegEncodingTable Huffman(int[] histogram, OperationContext context)
    {
        if (histogram.Length != 256) throw new ArgumentException("Byte alphabet required.");
        var active = 0;
        foreach (var count in histogram)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(histogram));
            if (count != 0) active++;
        }
        // One coarse owner covers all retained graph/list backing capacity,
        // including overlapping old/new list arrays. The returned table's owner
        // was established by the caller before entering this temporary scope.
        var scratch = 65536L + (active + 1) * 2048L;
        context.Reserve(scratch);
        try { context.Work(HuffmanWork); return Build(histogram, context); }
        finally { context.Release(scratch); }
    }

    private static JpegEncodingTable Build(int[] histogram, OperationContext context)
    {
        if (histogram.Length != 256) throw new ArgumentException("Byte alphabet required.");
        var symbols = new List<Symbol>();
        for (int i = 0; i < 256; i++)
        {
            if (histogram[i] < 0) throw new ArgumentOutOfRangeException(nameof(histogram));
            if (histogram[i] != 0) symbols.Add(new Symbol(i, histogram[i]));
        }
        if (symbols.Count == 0) throw new ArgumentException("Nonempty histogram required.");
        symbols.Add(new Symbol(256, 0)); // Reserve the all-ones path with a zero-weight leaf.
        Symbol[] ordered = symbols.ToArray(); Order(ordered, 0, ordered.Length - 1, context);
        var leaves = new List<Package>();
        foreach (Symbol symbol in ordered) leaves.Add(new Package(symbol.Weight, symbol.Value, null, null));
        var previous = new List<Package>();
        // Bounded binary coin collection. Equal-weight packages precede fresh
        // leaves. The final level contains packages only; its first N-1 items
        // determine the code-length multiset, excluding the reserved leaf.
        for (int level = 0; level <= 16; level++)
        {
            context.Work(0);
            var packages = new List<Package>();
            for (int i = 0; i + 1 < previous.Count; i += 2)
                packages.Add(new Package(previous[i].Weight + previous[i + 1].Weight, -1, previous[i], previous[i + 1]));
            var next = new List<Package>(); int leaf = level == 16 ? leaves.Count : 0, pair = 0;
            while (leaf < leaves.Count || pair < packages.Count)
                next.Add(leaf < leaves.Count && (pair == packages.Count || leaves[leaf].Weight < packages[pair].Weight)
                    ? leaves[leaf++] : packages[pair++]);
            previous = next;
        }
        var lengths = new int[257];
        var pending = new Stack<Package>();
        for (int i = 0; i < ordered.Length - 1; i++) pending.Push(previous[i]);
        while (pending.Count != 0)
        {
            context.Work(0);
            Package item = pending.Pop();
            if (item.Leaf >= 0) lengths[item.Leaf]++;
            else { pending.Push(item.Left!); pending.Push(item.Right!); }
        }
        var counts = new byte[16]; var values = new byte[ordered.Length - 1];
        var codes = new ushort[256]; var widths = new byte[256];
        for (int i = 0; i < 256; i++)
            if (lengths[i] != 0) counts[lengths[i] - 1]++;
        for (int i = 0; i < values.Length; i++) values[i] = (byte)ordered[ordered.Length - 1 - i].Value;
        var code = 0; var position = 0;
        for (var width = 1; width <= 16; width++)
        {
            for (var i = 0; i < counts[width - 1]; i++)
            {
                if (position >= values.Length || code >= (1 << width) - 1)
                    throw new InvalidOperationException("Invalid reference Huffman tree.");
                var value = values[position++]; codes[value] = (ushort)code++; widths[value] = (byte)width;
            }
            code <<= 1;
        }
        if (position != values.Length) throw new InvalidOperationException("Reference Huffman symbols differ.");
        return new JpegEncodingTable(counts, values, codes, widths);
    }

    // Frequency ties must retain the pinned median-of-three partition behavior.
    // A stable ordinal tie breaker would produce different DHT symbol bytes.
    // Recursion and index-based storage are independently written here; no
    // pointer-sort implementation or lookup permutation is imported.
    private static void Order(Symbol[] data, int first, int last, OperationContext context)
    {
        context.Work(0);
        if (first >= last) return;
        if (last == first + 1)
        {
            if (data[first].Weight > data[last].Weight) Swap(data, first, last);
            return;
        }
        int middle = (first + last) / 2; bool checkSorted = false;
        if (data[first].Weight > data[last].Weight)
            Swap(data, first, data[last].Weight > data[middle].Weight ? middle : last);
        else if (data[first].Weight > data[middle].Weight) Swap(data, first, middle);
        else checkSorted = true;
        if (data[middle].Weight > data[last].Weight) { Swap(data, middle, last); checkSorted = false; }
        if (last == first + 2) return;
        Swap(data, middle, last - 1);
        long pivot = data[last - 1].Weight; int low = first + 1, high = last - 2;
        while (low <= high)
        {
            while (low <= high && data[low].Weight < pivot) low++;
            while (low <= high && data[high].Weight > pivot) high--;
            if (low <= high) { Swap(data, low, high); low++; high--; }
        }
        Swap(data, low, last - 1);
        if (checkSorted && (middle == low || middle == low - 1))
        {
            bool sorted = true;
            for (int i = first; i < last; i++)
                if (data[i].Weight > data[i + 1].Weight) { sorted = false; break; }
            if (sorted) return;
        }
        Order(data, first, high, context); Order(data, low + 1, last, context);
    }

    private static void Swap(Symbol[] data, int a, int b) => (data[a], data[b]) = (data[b], data[a]);

}
