namespace Lokad.Image.Internal;

internal static class BilinearHorizontalPairs
{
    // The bounded reduction has at most six taps in each ten-slot record.
    // Reuse dead slot nine for a prepared four-tap/+2 source-window start.
    internal static void Prepare(ReadOnlySpan<int> counts, Span<int> offsets)
    {
        for (var x = 0; x + 1 < counts.Length; x += 2)
        {
            var at = x * 10;
            offsets[at + 9] = -1;
            if (counts[x] != 4 || counts[x + 1] != 4) continue;
            var first = offsets[at];
            var consecutive = first >= 0;
            for (var t = 0; t < 4; t++)
                consecutive &= offsets[at + t] == first + t && offsets[at + 10 + t] == first + 2 + t;
            if (consecutive) offsets[at + 9] = first;
        }
    }
}
