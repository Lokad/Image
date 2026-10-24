namespace Lokad.Image.Internal;

// Private encoder metrics. All buffers/weights are borrowed; the enclosing
// operation owns their admission and block work/cancellation. There are no
// per-block heap allocations, caches or scopes.
internal static class Vp8Distortion
{
    // Independent equivalent importance model: sample a quarter-plane Gaussian
    // with variance3, normalize to256, then distribute integer remainders.
    // This reproduces the pinned encoder profile without importing its table;
    // it does not assert how that profile was originally chosen.
    internal static void TextureWeights(Span<ushort> destination)
    {
        if (destination.Length < 16) throw new ArgumentException("Texture importance requires16weights.");
        Span<double> fractions = stackalloc double[16]; double total = 0;
        for (var p = 0; p < 16; p++)
        {
            var x = p & 3; var y = p >> 2;
            fractions[p] = Math.Exp(-(x * x + y * y) / 6d); total += fractions[p];
        }
        var remaining = 256;
        for (var p = 0; p < 16; p++)
        {
            var scaled = 256 * fractions[p] / total; var floor = (int)Math.Floor(scaled);
            destination[p] = (ushort)floor; remaining -= floor; fractions[p] = scaled - floor;
        }
        while (remaining-- > 0)
        {
            var best = 0;
            for (var p = 1; p < 16; p++) if (fractions[p] > fractions[best]) best = p;
            destination[best]++; fractions[best] = -1;
        }
    }

    private static void Check(ReadOnlySpan<byte> source, int stride, int width, int height)
    {
        if (width is < 1 or > 16 || height is < 1 or > 16 || stride < width
            || (long)(height - 1) * stride + width > source.Length)
            throw new ArgumentException("Distortion requires a complete pixel rectangle up to16x16.");
    }

    private static void CheckWeights(ReadOnlySpan<ushort> weights)
    {
        if (weights.Length < 16) throw new ArgumentException("Texture importance requires16weights.");
        var sum = 0; for (var p = 0; p < 16; p++) sum += weights[p];
        if (sum != 256) throw new ArgumentException("Texture importance must sum to256.");
    }

    internal static int Squared(ReadOnlySpan<byte> a, int aStride, ReadOnlySpan<byte> b, int bStride, int width, int height)
    {
        Check(a, aStride, width, height); Check(b, bStride, width, height);
        var sum = 0;
        for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
        {
            var difference = a[y * aStride + x] - b[y * bStride + x]; sum += difference * difference;
        }
        return sum;
    }

    // Factor the sequency Hadamard basis from reversed two-bit Gray-code
    // parity. Unlike forwardY2, this has no half-scale or rounding.
    private static int Energy(ReadOnlySpan<byte> pixels, int stride, ReadOnlySpan<ushort> weights)
    {
        Span<int> rows = stackalloc int[16];
        for (var y = 0; y < 4; y++)
        {
            var at = y * stride;
            var a = pixels[at] + pixels[at + 1]; var b = pixels[at + 2] + pixels[at + 3];
            var c = pixels[at] - pixels[at + 1]; var d = pixels[at + 2] - pixels[at + 3];
            rows[y * 4] = a + b; rows[y * 4 + 1] = a - b;
            rows[y * 4 + 2] = c - d; rows[y * 4 + 3] = c + d;
        }
        var energy = 0;
        for (var u = 0; u < 4; u++)
        {
            var a = rows[u] + rows[4 + u]; var b = rows[8 + u] + rows[12 + u];
            var c = rows[u] - rows[4 + u]; var d = rows[8 + u] - rows[12 + u];
            energy += weights[u] * Math.Abs(a + b) + weights[4 + u] * Math.Abs(a - b)
                + weights[8 + u] * Math.Abs(c - d) + weights[12 + u] * Math.Abs(c + d);
        }
        return energy;
    }

    private static int Texture(ReadOnlySpan<byte> a, int aStride, ReadOnlySpan<byte> b, int bStride, ReadOnlySpan<ushort> weights)
        => Math.Abs(Energy(a, aStride, weights) - Energy(b, bStride, weights)) >> 5;

    internal static int Texture4(ReadOnlySpan<byte> a, int aStride, ReadOnlySpan<byte> b, int bStride, ReadOnlySpan<ushort> weights)
    {
        Check(a, aStride, 4, 4); Check(b, bStride, 4, 4); CheckWeights(weights);
        return Texture(a, aStride, b, bStride, weights);
    }

    internal static int Texture16(ReadOnlySpan<byte> a, int aStride, ReadOnlySpan<byte> b, int bStride, ReadOnlySpan<ushort> weights)
    {
        Check(a, aStride, 16, 16); Check(b, bStride, 16, 16); CheckWeights(weights);
        var total = 0;
        for (var y = 0; y < 16; y += 4) for (var x = 0; x < 16; x += 4)
            total += Texture(a[(y * aStride + x)..], aStride, b[(y * bStride + x)..], bStride, weights);
        return total;
    }
}
