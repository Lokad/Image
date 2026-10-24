namespace Lokad.Image.Internal;

// The meaningful map can shrink in place, but its complete allocated capacity
// stays charged until disposal. This owner must not outlive its operation.
internal sealed class Vp8lColorTransformPlan : IDisposable
{
    private readonly OperationContext context;
    private uint[]? coefficients;
    private int count;
    internal int StoredBits { get; private set; }
    internal long CapacityBytes => (coefficients ?? throw new ObjectDisposedException(nameof(Vp8lColorTransformPlan))).Length * 4L;
    internal ReadOnlySpan<uint> Coefficients => (coefficients ?? throw new ObjectDisposedException(nameof(Vp8lColorTransformPlan))).AsSpan(0, count);
    internal Span<uint> Writable => coefficients ?? throw new ObjectDisposedException(nameof(Vp8lColorTransformPlan));

    internal Vp8lColorTransformPlan(OperationContext context, int length, int bits)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        this.context = context; count = length; StoredBits = bits;
        context.Work(0); context.Reserve(length * 4L);
        try { context.Work(0); coefficients = new uint[length]; }
        catch { context.Release(length * 4L); throw; }
    }

    internal void Compact(int width, int height)
    {
        var map = Writable;
        var initial = StoredBits; var stored = initial;
        var columns = (width + (1 << initial) - 1) >> initial;
        var rows = (height + (1 << initial) - 1) >> initial;
        for (var bits = initial + 1; bits <= 9; bits++)
        {
            var stride = 1 << (bits - initial); var same = true;
            for (var y = 0; same && y < rows; y++)
            {
                context.Work(columns);
                for (var x = 0; x < columns; x++)
                    if (map[y * columns + x] != map[y / stride * stride * columns + x / stride * stride])
                    { same = false; break; }
            }
            if (!same) break;
            stored = bits;
        }
        var step = 1 << (stored - initial);
        var newColumns = (width + (1 << stored) - 1) >> stored;
        var newRows = (height + (1 << stored) - 1) >> stored;
        for (var y = 0; y < newRows; y++)
        {
            context.Work(newColumns);
            // Source anchors advance at least as fast as destination indices;
            // writing the compact prefix cannot overwrite a later source anchor.
            for (var x = 0; x < newColumns; x++) map[y * newColumns + x] = map[y * step * columns + x * step];
        }
        count = newColumns * newRows; StoredBits = stored;
    }

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref coefficients, null);
        if (owned is not null) context.Release(owned.Length * 4L);
    }
}

// Independent reference-encoder transform selection. Channel arithmetic follows
// RFC 9649 sections 3.5.2 and 3.5.3; searches use mathematically derived costs.
internal static class Vp8lReferenceColorTransform
{
    internal static void SubtractGreen(Span<uint> working, OperationContext context)
    {
        for (var at = 0; at < working.Length; at += 4096)
        {
            var count = Math.Min(4096, working.Length - at); context.Work(count * 2L);
            var chunk = working.Slice(at, count);
            for (var i = 0; i < chunk.Length; i++)
            {
                var pixel = chunk[i]; var green = pixel >> 8 & 255;
                chunk[i] = pixel & 0xff00ff00u
                    | (unchecked((pixel >> 16) - green) & 255) << 16
                    | (unchecked(pixel - green) & 255);
            }
        }
    }

    private static uint Apply(uint pixel, int greenToRed, int greenToBlue, int redToBlue)
    {
        var green = (int)unchecked((sbyte)(pixel >> 8)); var red = (int)unchecked((sbyte)(pixel >> 16));
        var newRed = ((int)(pixel >> 16 & 255) - (unchecked((sbyte)greenToRed) * green >> 5)) & 255;
        var newBlue = ((int)(pixel & 255) - (unchecked((sbyte)greenToBlue) * green >> 5)
            - (unchecked((sbyte)redToBlue) * red >> 5)) & 255;
        return pixel & 0xff00ff00u | (uint)newRed << 16 | (uint)newBlue;
    }

    private sealed class Search : IDisposable
    {
        private const long ScratchBytes = 3 * 256 * 4L + 2 * 256 * 8L + 128;
        private readonly OperationContext context;
        private int[]? counts;
        private long[]? products;
        private long redBaseCost, blueBaseCost;
        private Span<long> RedProducts => products.AsSpan(0, 256);
        private Span<long> BlueProducts => products.AsSpan(256, 256);
        private Span<int> Histogram => counts.AsSpan(0, 256);
        private Span<int> RedHistory => counts.AsSpan(256, 256);
        private Span<int> BlueHistory => counts.AsSpan(512, 256);

        internal Search(OperationContext context)
        {
            this.context = context;
            context.Work(0); context.Reserve(ScratchBytes);
            try { context.Work(0); counts = new int[3 * 256]; products = new long[2 * 256]; }
            catch { context.Release(ScratchBytes); throw; }
        }

        private long Cost(bool red)
        {
            context.Work(256);
            return Vp8lColorCost.Evaluate(Histogram, red ? RedHistory : BlueHistory,
                red ? RedProducts : BlueProducts, red ? redBaseCost : blueBaseCost);
        }

        private long Score(ReadOnlySpan<uint> working, int width, int x0, int y0, int x1, int y1,
            uint previousX, uint previousY, bool red, int gr, int gb, int rb)
        {
            context.Work(256); var histogram = Histogram; histogram.Clear();
            // This tile remains unchanged during the complete coefficient search.
            // Strided borrowing needs neither a tile copy nor a second image.
            for (var y = y0; y < y1; y++)
            {
                var row = working.Slice(y * width + x0, x1 - x0); context.Work(row.Length * 4L);
                foreach (var pixel in row)
                {
                    var transformed = Apply(pixel, gr, gb, rb);
                    histogram[(int)(red ? transformed >> 16 & 255 : transformed & 255)]++;
                }
            }
            var cost = Cost(red);
            const long preference = 3 * Vp8lReferenceCost.Scale;
            if (red)
            {
                if (unchecked((byte)gr) == (byte)previousX) cost -= preference;
                if (unchecked((byte)gr) == (byte)previousY) cost -= preference;
                if (gr == 0) cost -= preference;
            }
            else
            {
                if (unchecked((byte)gb) == (byte)(previousX >> 8)) cost -= preference;
                if (unchecked((byte)gb) == (byte)(previousY >> 8)) cost -= preference;
                if (unchecked((byte)rb) == (byte)(previousX >> 16)) cost -= preference;
                if (unchecked((byte)rb) == (byte)(previousY >> 16)) cost -= preference;
                if (gb == 0) cost -= preference;
                if (rb == 0) cost -= preference;
            }
            return cost;
        }

        internal void SelectAndApply(Span<uint> working, int width, int height, int bits, int quality, Span<uint> map)
        {
            var columns = (width + (1 << bits) - 1) >> bits;
            var rows = (height + (1 << bits) - 1) >> bits;
            uint previousX = 0, previousY = 0;
            for (var ty = 0; ty < rows; ty++)
            for (var tx = 0; tx < columns; tx++)
            {
                var x0 = tx << bits; var y0 = ty << bits;
                var x1 = Math.Min(width, x0 + (1 << bits)); var y1 = Math.Min(height, y0 + (1 << bits));
                var tile = ty * columns + tx;
                if (ty != 0) previousY = map[tile - columns];
                // Both histories and tile population remain fixed through all
                // coefficient trials. Charge and prepare each history once.
                var population = (x1 - x0) * (y1 - y0);
                context.Work(256); redBaseCost = Vp8lColorCost.Prepare(RedHistory, population, RedProducts);
                context.Work(256); blueBaseCost = Vp8lColorCost.Prepare(BlueHistory, population, BlueProducts);
                var bestGr = 0;
                var bestRedCost = Score(working, width, x0, y0, x1, y1, previousX, previousY, true, 0, 0, 0);
                for (var level = 0; level < 4 + ((7 * quality) >> 8); level++)
                {
                    var step = 32 >> level;
                    for (var sign = -1; sign <= 1; sign += 2)
                    {
                        var candidate = bestGr + sign * step;
                        var cost = Score(working, width, x0, y0, x1, y1, previousX, previousY, true, candidate, 0, 0);
                        if (cost < bestRedCost) { bestRedCost = cost; bestGr = candidate; }
                    }
                }
                var bestGb = 0; var bestRb = 0;
                var bestBlueCost = Score(working, width, x0, y0, x1, y1, previousX, previousY, false, 0, 0, 0);
                var iterations = quality < 25 ? 1 : quality > 50 ? 7 : 4;
                for (var level = 0; level < iterations; level++)
                {
                    var step = 16 >> Math.Clamp(level - 1, 0, 3);
                    for (var axis = 0; axis < 8; axis++)
                    {
                        var dx = axis < 2 ? 0 : axis < 4 ? axis == 2 ? -1 : 1 : axis < 6 ? -1 : 1;
                        var dy = axis < 2 ? axis == 0 ? -1 : 1 : axis < 4 ? 0 : axis % 2 == 0 ? -1 : 1;
                        var gb = bestGb + step * dx; var rb = bestRb + step * dy;
                        var cost = Score(working, width, x0, y0, x1, y1, previousX, previousY, false, 0, gb, rb);
                        if (cost < bestBlueCost) { bestBlueCost = cost; bestGb = gb; bestRb = rb; }
                    }
                    if (step == 2 && bestGb == 0 && bestRb == 0) break;
                }
                // previousX deliberately persists across the row boundary.
                previousX = 0xff000000u | (uint)unchecked((byte)bestRb) << 16
                    | (uint)unchecked((byte)bestGb) << 8 | unchecked((byte)bestGr);
                map[tile] = previousX;
                for (var y = y0; y < y1; y++)
                {
                    context.Work((x1 - x0) * 4L);
                    for (var x = x0; x < x1; x++) working[y * width + x] = Apply(working[y * width + x], bestGr, bestGb, bestRb);
                }
                var redHistory = RedHistory; var blueHistory = BlueHistory;
                for (var y = y0; y < y1; y++)
                {
                    context.Work((x1 - x0) * 2L);
                    for (var x = x0; x < x1; x++)
                    {
                        var at = y * width + x; var pixel = working[at];
                        if (at >= 2 && pixel == working[at - 1] && pixel == working[at - 2]) continue;
                        if (at >= width + 2 && pixel == working[at - width]
                            && working[at - 1] == working[at - width - 1] && working[at - 2] == working[at - width - 2]) continue;
                        redHistory[(int)(pixel >> 16 & 255)]++; blueHistory[(int)(pixel & 255)]++;
                    }
                }
            }
        }

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref counts, null);
            Interlocked.Exchange(ref products, null);
            if (owned is not null) context.Release(ScratchBytes);
        }
    }

    internal static Vp8lColorTransformPlan SelectAndApply(Span<uint> working, int width,
        int bits, int quality, OperationContext context)
    {
        if (width is < 1 or > 16384 || working.Length == 0 || working.Length % width != 0
            || working.Length / width > 16384) throw new ArgumentException("Expected complete VP8L image rows.");
        if (bits is < 2 or > 9) throw new ArgumentOutOfRangeException(nameof(bits));
        if (quality is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(quality));
        var height = working.Length / width;
        var result = new Vp8lColorTransformPlan(context,
            ((width + (1 << bits) - 1) >> bits) * ((height + (1 << bits) - 1) >> bits), bits);
        try
        {
            using (var search = new Search(context)) search.SelectAndApply(working, width, height, bits, quality, result.Writable);
            result.Compact(width, height);
            return result;
        }
        catch { result.Dispose(); throw; }
    }
}
