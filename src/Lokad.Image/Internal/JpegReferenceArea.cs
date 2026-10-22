namespace Lokad.Image.Internal;

// Independently generated pixel-cell area weights for at most 2:1 chroma
// reduction. Q16 phases, ordered error-carry quantization and edge folding
// preserve the pinned conversion stages. All storage is bounded safe stack.
internal readonly struct JpegReferenceArea
{
    internal int Count { get; }
    private readonly int first, second, third, a, b, c;
    private JpegReferenceArea(int count, ReadOnlySpan<int> positions, ReadOnlySpan<int> weights)
    {
        Count = count; first = positions[0]; a = weights[0];
        second = count > 1 ? positions[1] : 0; b = count > 1 ? weights[1] : 0;
        third = count > 2 ? positions[2] : 0; c = count > 2 ? weights[2] : 0;
    }

    internal int Source(int index) => index == 0 ? first : index == 1 ? second : third;
    internal int Weight(int index) => index == 0 ? a : index == 1 ? b : c;

    internal static JpegReferenceArea Create(int sourceLength, int targetLength, int index, int bits)
    {
        Span<int> positions = stackalloc int[3], weights = stackalloc int[3];
        if (sourceLength == targetLength)
        {
            positions[0] = index; weights[0] = 1 << bits;
            return new JpegReferenceArea(1, positions, weights);
        }
        var cells = Math.Clamp(1 + (sourceLength + targetLength - 1) / targetLength,
            1, Math.Max(1, sourceLength - 2));
        var step = ((long)sourceLength * 65536 + targetLength / 2) / targetLength;
        var center = step - 65536 + 2 * index * step;
        var firstCell = (int)((center - (cells - 2) * 65536) / 131072);
        const long areaUnit = 1L << 46;
        var precision = sourceLength == 2 * targetLength ? 128 : 256;
        Span<long> areas = stackalloc long[3]; var count = 0;
        for (var j = firstCell; j < firstCell + cells; j++)
        {
            var distance = Math.Abs(j * 131072L - center) * 8192 * targetLength / sourceLength;
            var area = Math.Clamp((1L << 45) - (distance - (1L << 29)) * step, 0, areaUnit);
            if (count == 0 && area <= areaUnit / 500) continue;
            var source = Math.Clamp(j, 0, sourceLength - 1);
            if (count != 0 && positions[count - 1] == source) areas[count - 1] += area * precision;
            else { positions[count] = source; areas[count++] = area * precision; }
        }
        long sum = 0; for (var i = 0; i < count; i++) sum += areas[i];
        var denominator = (sum + (1L << (bits - 1))) >> bits;
        long error = 0;
        for (var i = 0; i < count; i++)
        {
            var value = areas[i] + error;
            weights[i] = (int)((value + denominator / 2) / denominator);
            error = value - weights[i] * denominator;
        }
        return new JpegReferenceArea(count, positions, weights);
    }
}
