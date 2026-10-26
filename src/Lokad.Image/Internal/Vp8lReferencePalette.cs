namespace Lokad.Image.Internal;

internal enum Vp8lPaletteOrder { Sorted, MinDelta, ModifiedZeng }

// Palette samples outlive packing, under the same borrowed operation context.
// The working image remains owned by the outer encoder, including unused capacity.
internal sealed class Vp8lPalettePlan : IDisposable
{
    private readonly OperationContext context;
    private uint[]? samples;
    internal int ColorCount { get; }
    internal int EncodedColorCount { get; private set; }
    internal int PackingBits { get; }
    internal int CodedWidth { get; }
    internal int PackedPixelCount { get; }
    internal long CapacityBytes => (samples ?? throw new ObjectDisposedException(nameof(Vp8lPalettePlan))).Length * 4L;
    internal ReadOnlySpan<uint> Colors => (samples ?? throw new ObjectDisposedException(nameof(Vp8lPalettePlan))).AsSpan(0, ColorCount);
    internal ReadOnlySpan<uint> EncodedDeltas => (samples ?? throw new ObjectDisposedException(nameof(Vp8lPalettePlan))).AsSpan(ColorCount, EncodedColorCount);
    internal Span<uint> WritableColors => (samples ?? throw new ObjectDisposedException(nameof(Vp8lPalettePlan))).AsSpan(0, ColorCount);
    internal Span<uint> WritableDeltas => (samples ?? throw new ObjectDisposedException(nameof(Vp8lPalettePlan))).AsSpan(ColorCount, ColorCount);

    internal Vp8lPalettePlan(OperationContext context, int count, int width, int height)
    {
        this.context = context; ColorCount = EncodedColorCount = count;
        PackingBits = count <= 2 ? 3 : count <= 4 ? 2 : count <= 16 ? 1 : 0;
        CodedWidth = (width + (1 << PackingBits) - 1) >> PackingBits;
        PackedPixelCount = CodedWidth * height;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(count * 8L);
        try { context.Cancellation.ThrowIfCancellationRequested(); samples = new uint[count * 2]; }
        catch { context.Release(count * 8L); throw; }
    }

    internal void OmitImplicitZero()
    {
        EncodedColorCount = ColorCount > 17 && Colors[^1] == 0 ? ColorCount - 1 : ColorCount;
    }

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref samples, null);
        if (owned is not null) context.Release(owned.Length * 4L);
    }
}

// Independent palette selection and packing; color deltas, index bit order and
// implicit palette padding follow RFC 9649 section 3.5.4. No input is retained.
internal static class Vp8lReferencePalette
{
    private const int Slots = 512;
    private const long ScratchBytes = Slots * 6L + 128;

    private static uint Difference(uint first, uint previous)
    {
        uint result = 0;
        for (var shift = 0; shift < 32; shift += 8)
            result |= (unchecked((first >> shift) - (previous >> shift)) & 255) << shift;
        return result;
    }

    private static int Distance(uint first, uint previous)
    {
        var delta = Difference(first, previous); var score = 0;
        for (var shift = 0; shift < 32; shift += 8)
        {
            var value = (int)(delta >> shift & 255);
            score += (shift == 24 ? 1 : 9) * Math.Min(value, 256 - value);
        }
        return score;
    }

    private static int Find(uint value, ReadOnlySpan<uint> keys, ReadOnlySpan<ushort> indices, OperationContext context)
    {
        var slot = (int)(unchecked(value * 0x9e3779b9u) >> 23);
        context.Cancellation.ThrowIfCancellationRequested();
        while (indices[slot] != 0 && keys[slot] != value)
        { context.Cancellation.ThrowIfCancellationRequested(); slot = (slot + 1) & (Slots - 1); }
        return slot;
    }

    private static void Order(Span<uint> colors, ReadOnlySpan<uint> working, int width, Vp8lPaletteOrder order, OperationContext context)
    {
        // Numeric order is independent of collection-table positions. At most
        // 256 elements bound this sort and its cancellation interval.
        context.Cancellation.ThrowIfCancellationRequested(); colors.Sort();
        if (order == Vp8lPaletteOrder.ModifiedZeng)
        { Vp8lPaletteAdjacency.Reorder(colors, working, width, context); return; }
        var minimizeDeltas = order == Vp8lPaletteOrder.MinDelta;
        var positive = 0; var negative = 0; uint previous = 0;
        foreach (var color in colors)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var delta = Difference(color, previous); previous = color;
            for (var channel = 0; channel < 3; channel++)
            {
                var value = delta >> (channel * 8) & 255;
                if (value == 0) continue;
                if (value < 128) positive |= 1 << channel; else negative |= 1 << channel;
            }
        }
        if (minimizeDeltas && (positive & negative) != 0)
        {
            var active = colors.Length;
            if (active > 17 && colors[0] == 0)
            { active--; (colors[0], colors[active]) = (colors[active], colors[0]); }
            previous = 0;
            for (var at = 0; at < active; at++)
            {
                var winner = at; var best = int.MaxValue;
                for (var trial = at; trial < active; trial++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var cost = Distance(colors[trial], previous);
                    if (cost < best) { best = cost; winner = trial; }
                }
                (colors[at], colors[winner]) = (colors[winner], colors[at]); previous = colors[at];
            }
        }
        if (!minimizeDeltas && colors.Length > 17 && colors[0] == 0)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            colors[1..].CopyTo(colors); colors[^1] = 0;
        }
    }

    // working is untransformed normalized ARGB in library-owned storage. Packing
    // may partially modify it on failure; the outer encoder then discards it.
    internal static Vp8lPalettePlan SelectAndPack(Span<uint> working, int width,
        int colorCount, Vp8lPaletteOrder order, OperationContext context)
    {
        if (width is < 1 or > 16384 || working.Length == 0 || working.Length % width != 0
            || working.Length / width > 16384) throw new ArgumentException("Expected complete VP8L image rows.");
        if (colorCount is < 1 or > 256) throw new ArgumentOutOfRangeException(nameof(colorCount));
        if ((uint)order > (uint)Vp8lPaletteOrder.ModifiedZeng) throw new ArgumentOutOfRangeException(nameof(order));
        var height = working.Length / width;
        var result = new Vp8lPalettePlan(context, colorCount, width, height);
        long scratch = 0;
        try
        {
            context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(ScratchBytes); scratch = ScratchBytes;
            context.Cancellation.ThrowIfCancellationRequested();
            var keys = new uint[Slots]; var indices = new ushort[Slots];
            var colors = result.WritableColors; var found = 0;
            foreach (var pixel in working)
            {
                var slot = Find(pixel, keys, indices, context);
                if (indices[slot] != 0) continue;
                if (found == colorCount) throw new ArgumentException("Palette color count differs from analysis.", nameof(colorCount));
                keys[slot] = pixel; indices[slot] = (ushort)(found + 1); colors[found++] = pixel;
            }
            if (found != colorCount) throw new ArgumentException("Palette color count differs from analysis.", nameof(colorCount));
            Order(colors, working, width, order, context);
            var deltas = result.WritableDeltas; uint previous = 0;
            for (var i = 0; i < colorCount; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); deltas[i] = Difference(colors[i], previous); previous = colors[i];
            }
            result.OmitImplicitZero();
            context.Cancellation.ThrowIfCancellationRequested(); indices.AsSpan().Clear();
            for (var i = 0; i < colorCount; i++)
            {
                var slot = Find(colors[i], keys, indices, context);
                keys[slot] = colors[i]; indices[slot] = (ushort)(i + 1);
            }
            for (var y = 0; y < height; y++)
            for (var block = 0; block < result.CodedWidth; block++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); uint value = 0;
                for (var at = 0; at < (1 << result.PackingBits) && (block << result.PackingBits) + at < width; at++)
                {
                    var source = y * width + (block << result.PackingBits) + at;
                    var slot = Find(working[source], keys, indices, context);
                    if (indices[slot] == 0) throw new InvalidOperationException("Palette source changed during packing.");
                    value |= (uint)(indices[slot] - 1) << (at * (8 >> result.PackingBits));
                }
                // Read the complete group before overwriting its destination.
                // Source groups advance at least as fast as compact destinations.
                working[y * result.CodedWidth + block] = 0xff000000u | value << 8;
            }
            return result;
        }
        catch { result.Dispose(); throw; }
        finally { if (scratch != 0) context.Release(scratch); }
    }
}
