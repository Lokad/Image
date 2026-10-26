namespace Lokad.Image.Internal;

internal readonly struct Vp8lHistogramLayout
{
    internal int CacheBits { get; }
    internal int CacheSize => CacheBits == 0 ? 0 : 1 << CacheBits;
    internal int GreenSize => 280 + CacheSize;
    internal int Red => GreenSize;
    internal int Blue => Red + 256;
    internal int Alpha => Blue + 256;
    internal int Distance => Alpha + 256;
    internal int Total => Distance + 40;

    internal Vp8lHistogramLayout(int cacheBits)
    {
        if (cacheBits is < 0 or > 10) throw new ArgumentOutOfRangeException(nameof(cacheBits));
        CacheBits = cacheBits;
    }

    internal void Fill(ReadOnlySpan<Vp8lToken> tokens, int width, bool plane, Span<int> counts, OperationContext context)
    {
        foreach (var token in tokens)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (token.Code > 0)
            {
                counts[256 + Vp8lSymbols.Prefix(token.Code).Code]++;
                counts[Distance + Vp8lSymbols.Prefix(plane ? Vp8lSymbols.Plane((int)token.Value, width) : (int)token.Value).Code]++;
            }
            else if (token.Code == 0)
            {
                var pixel = token.Value;
                counts[(int)(pixel >> 8 & 255)]++; counts[Red + (int)(pixel >> 16 & 255)]++;
                counts[Blue + (int)(pixel & 255)]++; counts[Alpha + (int)(pixel >> 24)]++;
            }
            else
            {
                var index = ~token.Code;
                if ((uint)index >= (uint)CacheSize) throw new InvalidOperationException("Token cache index exceeds its alphabet.");
                counts[280 + index]++;
            }
        }
    }

    internal long Population(ReadOnlySpan<int> counts, OperationContext context)
        => Vp8lReferenceEntropy.Population(counts[..GreenSize], context)
            + Vp8lReferenceEntropy.Population(counts.Slice(Red, 256), context)
            + Vp8lReferenceEntropy.Population(counts.Slice(Blue, 256), context)
            + Vp8lReferenceEntropy.Population(counts.Slice(Alpha, 256), context)
            + Vp8lReferenceEntropy.Population(counts.Slice(Distance, 40), context);
}
