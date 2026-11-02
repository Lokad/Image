namespace Lokad.Image.Internal;

internal readonly struct Vp8lHistogramLayout
{
    internal int CacheBits { get; }
    internal int CacheSize => CacheBits == 0 ? 0 : 1 << CacheBits;
    internal int GreenSize => 280 + CacheSize;
    // Only clustering scratch may collapse globally constant literal banks.
    // Zero/default retains the full logical alphabets used by published plans.
    internal int ConstantBanks { get; }
    internal int RedSize => (ConstantBanks & 2) == 0 ? 256 : 1;
    internal int BlueSize => (ConstantBanks & 4) == 0 ? 256 : 1;
    internal int AlphaSize => (ConstantBanks & 8) == 0 ? 256 : 1;
    internal int Red => GreenSize;
    internal int Blue => Red + RedSize;
    internal int Alpha => Blue + BlueSize;
    internal int Distance => Alpha + AlphaSize;
    internal int Total => Distance + 40;

    internal Vp8lHistogramLayout(int cacheBits, int constantBanks = 0)
    {
        if (cacheBits is < 0 or > 10) throw new ArgumentOutOfRangeException(nameof(cacheBits));
        if ((constantBanks & ~14) != 0) throw new ArgumentOutOfRangeException(nameof(constantBanks));
        CacheBits = cacheBits; ConstantBanks = constantBanks;
    }

    internal void Fill(ReadOnlySpan<Vp8lToken> tokens, int width, bool plane, Span<int> counts, OperationContext context)
    {
        var red = Red; var blue = Blue; var alpha = Alpha; var distance = Distance;
        var redMask = (uint)(RedSize - 1); var blueMask = (uint)(BlueSize - 1); var alphaMask = (uint)(AlphaSize - 1);
        foreach (var token in tokens)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (token.Code > 0)
            {
                counts[256 + Vp8lSymbols.Prefix(token.Code).Code]++;
                counts[distance + Vp8lSymbols.Prefix(plane ? Vp8lSymbols.Plane((int)token.Value, width) : (int)token.Value).Code]++;
            }
            else if (token.Code == 0)
            {
                var pixel = token.Value;
                counts[(int)(pixel >> 8 & 255)]++; counts[red + (int)(pixel >> 16 & redMask)]++;
                counts[blue + (int)(pixel & blueMask)]++; counts[alpha + (int)(pixel >> 24 & alphaMask)]++;
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
    {
        if (ConstantBanks != 0) throw new InvalidOperationException("Clustering scratch is not a logical population.");
        return Vp8lReferenceEntropy.Population(counts[..GreenSize], context)
            + Vp8lReferenceEntropy.Population(counts.Slice(Red, 256), context)
            + Vp8lReferenceEntropy.Population(counts.Slice(Blue, 256), context)
            + Vp8lReferenceEntropy.Population(counts.Slice(Alpha, 256), context)
            + Vp8lReferenceEntropy.Population(counts.Slice(Distance, 40), context);
    }

    // Expand at the ownership boundary, never exposing physical scratch banks
    // to Huffman construction or the retained histogram plan.
    internal void AddToFull(Span<int> target, ReadOnlySpan<int> source, uint literal, OperationContext context)
    {
        if (ConstantBanks == 0) { Vp8lReferenceClustering.AddPopulation(target, source, context); return; }
        var full = new Vp8lHistogramLayout(CacheBits);
        Vp8lReferenceClustering.AddPopulation(target[..GreenSize], source[..GreenSize], context);
        for (var bank = 1; bank < 4; bank++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var start = bank == 1 ? Red : bank == 2 ? Blue : Alpha;
            var output = bank == 1 ? full.Red : bank == 2 ? full.Blue : full.Alpha;
            if ((ConstantBanks & (1 << bank)) == 0)
                Vp8lReferenceClustering.AddPopulation(target.Slice(output, 256), source.Slice(start, 256), context);
            else
            {
                var symbol = bank == 1 ? (int)(literal >> 16 & 255) : bank == 2 ? (int)(literal & 255) : (int)(literal >> 24);
                target[output + symbol] = unchecked(target[output + symbol] + source[start]);
            }
        }
        Vp8lReferenceClustering.AddPopulation(target.Slice(full.Distance, 40), source.Slice(Distance, 40), context);
    }
}
