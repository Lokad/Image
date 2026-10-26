namespace Lokad.Image.Internal;

internal static partial class Vp8lReferenceCache
{
    internal static int Key(uint pixel, int bits) => (int)(unchecked(pixel * 0x1e35a7bdu) >> (32 - bits));

    internal static int BestBits(ReadOnlySpan<uint> pixels, ReadOnlySpan<Vp8lToken> seed, int maximumBits, OperationContext context)
    {
        var maximum = new Vp8lHistogramLayout(maximumBits);
        var reserved = (maximum.Total + maximum.CacheSize) * 4L + 128;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(reserved);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var cache = new uint[maximum.CacheSize]; var counts = new int[maximum.Total];
            var best = 0; var bestCost = long.MaxValue;
            for (var bits = 0; bits <= maximumBits; bits++)
            {
                var layout = new Vp8lHistogramLayout(bits);
                context.Cancellation.ThrowIfCancellationRequested();
                cache.AsSpan(0, layout.CacheSize).Clear(); counts.AsSpan(0, layout.Total).Clear();
                var position = 0;
                foreach (var token in seed)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    if (token.Code > 0)
                    {
                        counts[256 + Vp8lSymbols.Prefix(token.Code).Code]++;
                        // Distance counts intentionally do not enter cache choice.
                        if (bits != 0)
                        {
                            for (var i = position; i < position + token.Length; i++)
                            { context.Cancellation.ThrowIfCancellationRequested(); var pixel = pixels[i]; cache[Key(pixel, bits)] = pixel; }
                        }
                    }
                    else
                    {
                        var pixel = pixels[position]; var key = bits == 0 ? 0 : Key(pixel, bits);
                        if (bits != 0 && cache[key] == pixel) counts[280 + key]++;
                        else
                        {
                            counts[(int)(pixel >> 8 & 255)]++; counts[layout.Red + (int)(pixel >> 16 & 255)]++;
                            counts[layout.Blue + (int)(pixel & 255)]++; counts[layout.Alpha + (int)(pixel >> 24)]++;
                            if (bits != 0) cache[key] = pixel;
                        }
                    }
                    position += token.Length;
                }
                if (position != pixels.Length) throw new ArgumentException("Token seed does not cover its source.", nameof(seed));
                var cost = layout.Population(counts.AsSpan(0, layout.Total), context);
                if (cost < bestCost) { bestCost = cost; best = bits; }
            }
            return best;
        }
        finally { context.Release(reserved); }
    }

    // Convert only owned token storage; source pixels remain borrowed. Copies
    // update every cache entry they pass, including overlapping back references.
    internal static void Apply(ReadOnlySpan<uint> pixels, Vp8lTokenBuffer buffer, int bits, OperationContext context)
    {
        var layout = new Vp8lHistogramLayout(bits);
        context.Cancellation.ThrowIfCancellationRequested();
        if (bits == 0)
        {
            if (buffer.CacheBits != 0) throw new InvalidOperationException("Cannot remove an already applied token cache.");
            return;
        }
        var reserved = layout.CacheSize * 4L + 64;
        context.Reserve(reserved);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested(); var cache = new uint[layout.CacheSize]; var tokens = buffer.Writable; var position = 0;
            for (var i = 0; i < tokens.Length; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var token = tokens[i];
                if (token.Code > 0)
                {
                    for (var at = position; at < position + token.Length; at++)
                    { context.Cancellation.ThrowIfCancellationRequested(); var pixel = pixels[at]; cache[Key(pixel, bits)] = pixel; }
                }
                else
                {
                    var pixel = pixels[position]; var key = Key(pixel, bits);
                    tokens[i] = cache[key] == pixel ? Vp8lToken.Cached(key) : Vp8lToken.Literal(pixel); cache[key] = pixel;
                }
                position += token.Length;
            }
            if (position != pixels.Length) throw new ArgumentException("Token buffer does not cover its source.", nameof(pixels));
            buffer.CacheBits = bits;
        }
        finally { context.Release(reserved); }
    }
}
