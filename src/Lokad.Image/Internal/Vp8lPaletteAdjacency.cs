namespace Lokad.Image.Internal;

// Independent adjacency-based palette ordering (modified Zeng). The strategy is
// discussed in Pinho/Neves, "A Survey on Palette Reordering Methods for Improving
// the Compression of Color-Indexed Images". Four-neighbor frequencies choose
// colors; weighted distance to the current sequence chooses which end receives
// the next color. Earlier ties, including swap-removal order, are encoder policy.
internal static class Vp8lPaletteAdjacency
{
    internal static void Reorder(Span<uint> colors, ReadOnlySpan<uint> pixels, int width, OperationContext context)
    {
        var count = colors.Length;
        if (count < 2) return;
        var scratch = count * count * 4L + width * 4L + count * 16L;
        context.Reserve(scratch);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var adjacency = new uint[count * count]; var row = new int[width];
            Span<uint> sorted = stackalloc uint[count]; colors.CopyTo(sorted);
            Span<int> sequence = stackalloc int[count], remaining = stackalloc int[count];
            Span<uint> affinity = stackalloc uint[count];
            var cachedIndex = -1; uint cachedColor = 0;
            for (var y = 0; y < pixels.Length / width; y++)
            {
                var left = -1;
                for (var x = 0; x < width; x++)
                {
                    context.Cancellation.ThrowIfCancellationRequested(); var color = pixels[y * width + x];
                    if (cachedIndex < 0 || color != cachedColor)
                    {
                        var lo = 0; var hi = count - 1;
                        while (lo <= hi)
                        {
                            context.Cancellation.ThrowIfCancellationRequested(); var mid = (lo + hi) / 2;
                            if (sorted[mid] == color) { cachedIndex = mid; break; }
                            if (sorted[mid] < color) lo = mid + 1; else hi = mid - 1;
                        }
                        if (lo > hi) throw new InvalidOperationException("Palette source changed during ordering.");
                        cachedColor = color;
                    }
                    var index = cachedIndex;
                    if (left >= 0 && left != index) { adjacency[index * count + left]++; adjacency[left * count + index]++; }
                    if (y != 0 && row[x] != index) { adjacency[index * count + row[x]]++; adjacency[row[x] * count + index]++; }
                    row[x] = left = index;
                }
            }

            var seed = 0; uint bestTotal = 0;
            for (var i = 0; i < count; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); uint total = 0;
                for (var j = 0; j < count; j++) total += adjacency[i * count + j];
                if (total > bestTotal) { bestTotal = total; seed = i; }
            }
            var partner = 0; uint strongest = 0;
            for (var i = 0; i < count; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var edge = adjacency[seed * count + i];
                if (edge > strongest) { strongest = edge; partner = i; }
            }
            sequence[0] = seed; sequence[1] = partner;
            var active = 0;
            for (var i = 0; i < count; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); if (i == seed || i == partner) continue;
                remaining[active] = i; affinity[active++] = adjacency[i * count + seed] + adjacency[i * count + partner];
            }
            var start = 0; var length = 2;
            while (active != 0)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var winner = 0;
                for (var i = 1; i < active; i++) if (affinity[i] > affinity[winner]) winner = i;
                var next = remaining[winner]; long placement = 0;
                for (var i = 0; i < length; i++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    placement += (length - 1 - 2 * i) * (long)adjacency[next * count + sequence[(start + i) % count]];
                }
                // The pinned policy uses a signed32-bit weighted-distance sum.
                if (unchecked((int)placement) > 0) { start = (start + count - 1) % count; sequence[start] = next; }
                else sequence[(start + length) % count] = next;
                length++; active--;
                remaining[winner] = remaining[active];
                affinity[winner] = affinity[active];
                for (var i = 0; i < active; i++)
                { context.Cancellation.ThrowIfCancellationRequested(); affinity[i] += adjacency[next * count + remaining[i]]; }
            }
            for (var i = 0; i < count; i++) { context.Cancellation.ThrowIfCancellationRequested(); colors[i] = sorted[sequence[(start + i) % count]]; }
        }
        finally { context.Release(scratch); }
    }
}
