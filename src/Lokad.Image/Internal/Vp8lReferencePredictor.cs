namespace Lokad.Image.Internal;

// Only the selected map escapes analysis. Its operation charge remains live
// until disposal; this owner must be disposed before its operation context.
internal sealed class Vp8lPredictionPlan : IDisposable
{
    private readonly OperationContext context;
    private byte[]? modes;
    internal ReadOnlySpan<byte> Modes => modes ?? throw new ObjectDisposedException(nameof(Vp8lPredictionPlan));
    internal Span<byte> Writable => modes ?? throw new ObjectDisposedException(nameof(Vp8lPredictionPlan));
    internal int SelectedBits { get; }
    internal int StoredBits { get; }
    internal long EntropyCost { get; }

    internal Vp8lPredictionPlan(OperationContext context, int length, int selectedBits, int storedBits, long entropyCost)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        this.context = context; SelectedBits = selectedBits; StoredBits = storedBits; EntropyCost = entropyCost;
        context.Work(0); context.Reserve(length);
        try { context.Work(0); modes = new byte[length]; }
        catch { context.Release(length); throw; }
    }

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref modes, null);
        if (owned is not null) context.Release(owned.Length);
    }
}

// Independently implemented selection for the reference encoder being integrated.
// Prediction and modulo channel arithmetic follow RFC 9649 section 3.5.1.
internal static class Vp8lReferencePredictor
{
    private const int HistogramSize = 14 * 1024;
    private const int LevelSize = HistogramSize + 1024 + 14;
    private static int Ceil(int value, int bits) => (value + (1 << bits) - 1) >> bits;

    private sealed class Analysis : IDisposable
    {
        private readonly OperationContext context;
        private readonly int width, height, minBits, maxBits, levels;
        private readonly long reserved;
        private readonly bool greenOnly;
        private int[]? data;
        private byte[]? maps;
        private readonly int[] offsets;
        private readonly int[] columns;

        internal Analysis(int width, int height, int minBits, int maxBits, OperationContext context, bool greenOnly)
        {
            this.context = context; this.width = width; this.height = height; this.greenOnly = greenOnly;
            this.minBits = minBits; this.maxBits = maxBits; levels = maxBits - minBits + 1;
            var mapLength = 0;
            for (var bits = minBits; bits <= maxBits; bits++) mapLength += Ceil(width, bits) * Ceil(height, bits);
            // Flat reusable level histograms, accumulated counts, mode counts and
            // candidate maps; the coarse allowance covers the small owner arrays.
            reserved = levels * LevelSize * 4L + mapLength + 512;
            context.Work(0); context.Reserve(reserved);
            try
            {
                context.Work(0);
                data = new int[levels * LevelSize]; maps = new byte[mapLength];
                offsets = new int[levels]; columns = new int[levels];
                var offset = 0;
                for (var level = 0; level < levels; level++)
                {
                    var bits = minBits + level;
                    offsets[level] = offset; columns[level] = Ceil(width, bits);
                    offset += columns[level] * Ceil(height, bits);
                }
            }
            catch { context.Release(reserved); throw; }
        }

        private Span<int> Histogram(int level) => data.AsSpan(level * LevelSize, HistogramSize);
        private Span<int> History(int level) => data.AsSpan(level * LevelSize + HistogramSize, 1024);
        private Span<int> Frequency(int level) => data.AsSpan(level * LevelSize + HistogramSize + 1024, 14);
        private Span<byte> Map(int level) => maps.AsSpan(offsets[level], columns[level] * Ceil(height, minBits + level));

        private void Visit(ReadOnlySpan<uint> pixels, int level, int x0, int y0)
        {
            var bits = minBits + level; var histogram = Histogram(level);
            for (var at = 0; at < histogram.Length; at += 4096)
            {
                var count = Math.Min(4096, histogram.Length - at);
                context.Work(count); histogram.Slice(at, count).Clear();
            }
            if (level == 0 && greenOnly)
                Vp8lGreenPredictorHistogram.Build(pixels, width, height, x0, y0, bits, histogram, context);
            else if (level == 0)
            {
                for (var y = y0; y < Math.Min(y0 + (1 << bits), height); y++)
                for (var x = x0; x < Math.Min(x0 + (1 << bits), width); x++)
                {
                    context.Work(56);
                    var at = y * width + x; var actual = pixels[at];
                    for (var mode = 0; mode < 14; mode++)
                    {
                        var prediction = y == 0 ? x == 0 ? 0xff000000u : pixels[at - 1]
                            : x == 0 ? pixels[at - width]
                            : WebpLosslessDecoder.Predict(mode, pixels[at - 1], pixels[at - width], pixels[at - width - 1],
                                pixels[x == width - 1 ? y * width : at - width + 1]);
                        for (var plane = 0; plane < 4; plane++)
                        {
                            var shift = (3 - plane) * 8;
                            histogram[mode * 1024 + plane * 256 + (int)(unchecked((actual >> shift) - (prediction >> shift)) & 255)]++;
                        }
                    }
                }
            }
            else
            {
                // Each square's children finish in quadrant order before its
                // parent. One histogram per level is reused, with separate history.
                var half = 1 << (bits - 1);
                for (var child = 0; child < 4; child++)
                {
                    var childX = x0 + (child & 1) * half; var childY = y0 + (child >> 1) * half;
                    if (childX >= width || childY >= height) continue;
                    Visit(pixels, level - 1, childX, childY);
                    var lower = Histogram(level - 1);
                    for (var at = 0; at < histogram.Length; at += 4096)
                    {
                        var count = Math.Min(4096, histogram.Length - at); context.Work(count);
                        for (var i = at; i < at + count; i++) histogram[i] += lower[i];
                    }
                }
            }
            var tx = x0 >> bits; var ty = y0 >> bits; var tile = ty * columns[level] + tx;
            var modes = Map(level); var history = History(level); var chosen = 0; var best = long.MaxValue;
            for (var mode = 0; mode < 14; mode++)
            {
                var cost = Vp8lReferenceCost.Prediction(histogram.Slice(mode * 1024, 1024), history, context);
                if (tx != 0 && modes[tile - 1] == mode) cost -= 15 * Vp8lReferenceCost.Scale;
                if (ty != 0 && modes[tile - columns[level]] == mode) cost -= 15 * Vp8lReferenceCost.Scale;
                if (cost < best) { best = cost; chosen = mode; }
            }
            modes[tile] = (byte)chosen; Frequency(level)[chosen]++;
            context.Work(1024);
            for (var i = 0; i < 1024; i++) history[i] += histogram[chosen * 1024 + i];
        }

        internal Vp8lPredictionPlan Select(ReadOnlySpan<uint> pixels)
        {
            for (var y = 0; y < height; y += 1 << maxBits)
            for (var x = 0; x < width; x += 1 << maxBits) Visit(pixels, levels - 1, x, y);
            var winner = 0; var best = long.MaxValue;
            for (var level = 0; level < levels; level++)
            {
                var cost = Vp8lReferenceCost.Shannon(Frequency(level), context);
                for (var plane = 0; plane < 4; plane++)
                    cost += Vp8lReferenceCost.Shannon(History(level).Slice(plane * 256, 256), context);
                if (cost < best) { best = cost; winner = level; }
            }
            var selected = minBits + winner; var stored = selected;
            var original = Map(winner); var oldColumns = columns[winner]; var oldRows = Ceil(height, selected);
            for (var bits = selected + 1; bits <= 9; bits++)
            {
                var step = 1 << (bits - selected); var same = true;
                for (var y = 0; same && y < oldRows; y++)
                {
                    context.Work(oldColumns);
                    for (var x = 0; x < oldColumns; x++)
                        if (original[y * oldColumns + x] != original[(y / step * step) * oldColumns + x / step * step])
                        { same = false; break; }
                }
                if (!same) break;
                stored = bits;
            }
            var stride = 1 << (stored - selected); var newColumns = Ceil(width, stored); var newRows = Ceil(height, stored);
            var result = new Vp8lPredictionPlan(context, newColumns * newRows, selected, stored, best);
            try
            {
                var destination = result.Writable;
                for (var y = 0; y < newRows; y++)
                {
                    context.Work(newColumns);
                    for (var x = 0; x < newColumns; x++)
                        destination[y * newColumns + x] = original[y * stride * oldColumns + x * stride];
                }
                return result;
            }
            catch { result.Dispose(); throw; }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref data, null) is null) return;
            maps = null; context.Release(reserved);
        }
    }

    internal static Vp8lPredictionPlan SelectAndApply(Span<uint> working, int width,
        int minBits, int maxBits, bool lowEffort, OperationContext context, bool greenOnly = false)
    {
        if (width is < 1 or > 16384 || working.Length == 0 || working.Length % width != 0
            || working.Length / width > 16384) throw new ArgumentException("Expected complete VP8L image rows.");
        if (minBits < 2 || minBits > maxBits || maxBits > 9) throw new ArgumentOutOfRangeException(nameof(minBits));
        var height = working.Length / width;
        Vp8lPredictionPlan? result = null;
        try
        {
            if (lowEffort)
            {
                // Forced mode eleven retains configured sampling, including a
                // uniform map. It does not enter adaptive analysis/coarsening.
                result = new(context, Ceil(width, maxBits) * Ceil(height, maxBits), maxBits, maxBits, 0);
                var modes = result.Writable;
                for (var at = 0; at < modes.Length; at += 4096)
                {
                    var count = Math.Min(4096, modes.Length - at); context.Work(count); modes.Slice(at, count).Fill(11);
                }
            }
            else
            {
                using var analysis = new Analysis(width, height, minBits, maxBits, context, greenOnly);
                result = analysis.Select(working);
            }
            // Candidate maps and analysis counts are released before row scratch.
            Apply(working, width, result, context);
            return result;
        }
        catch { result?.Dispose(); throw; }
    }

    private static void Apply(Span<uint> working, int width, Vp8lPredictionPlan plan, OperationContext context)
    {
        var scratch = width * 8L;
        context.Work(0); context.Reserve(scratch);
        try
        {
            context.Work(0);
            var rows = new uint[width * 2];
            var previous = rows.AsSpan(0, width); var current = rows.AsSpan(width);
            var modes = plan.Modes; var columns = Ceil(width, plan.StoredBits);
            for (var y = 0; y < working.Length / width; y++)
            {
                context.Work(width); working.Slice(y * width, width).CopyTo(current);
                for (var x = 0; x < width; x++)
                {
                    context.Work(4);
                    var prediction = y == 0 ? x == 0 ? 0xff000000u : current[x - 1]
                        : x == 0 ? previous[x]
                        : WebpLosslessDecoder.Predict(modes[(y >> plan.StoredBits) * columns + (x >> plan.StoredBits)],
                            current[x - 1], previous[x], previous[x - 1], x == width - 1 ? current[0] : previous[x + 1]);
                    uint residual = 0;
                    for (var shift = 0; shift < 32; shift += 8)
                        residual |= (unchecked((current[x] >> shift) - (prediction >> shift)) & 255) << shift;
                    working[y * width + x] = residual;
                }
                var temporary = previous; previous = current; current = temporary;
            }
        }
        finally { context.Release(scratch); }
    }
}
