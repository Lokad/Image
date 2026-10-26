namespace Lokad.Image.Internal;

// Independent matrix evaluation of T.81 A.3.3 at the pinned reference's
// fixed-point boundaries, used by every JPEG inverse block.
// The kernel is generated from the cosine definition, without upstream tables.
internal static class JpegInverseTransform
{
    private static readonly int[] Kernel = BuildKernel();

    private static int[] BuildKernel()
    {
        var result = new int[64];
        for (var x = 0; x < 8; x++)
        for (var frequency = 0; frequency < 8; frequency++)
        {
            var value = frequency == 0 ? 1 : Math.Sqrt(2) * Math.Cos((2 * x + 1) * frequency * Math.PI / 16);
            // The pinned 8-bit policy represents the unit rotation one Q14
            // step below unity, including the DC and quarter-turn columns.
            result[x * 8 + frequency] = frequency is 0 or 4
                ? Math.Sign(value) * ((1 << 14) - 1)
                : (int)Math.Round(value * (1 << 14), MidpointRounding.AwayFromZero);
        }
        return result;
    }

    internal static void Inverse(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels, OperationContext context)
    {
        if (coefficients.Length != 64 || quantization.Length != 64 || pixels.Length != 64)
            throw new ArgumentException("JPEG transforms operate on complete 8x8 blocks.");
        context.Cancellation.ThrowIfCancellationRequested();
        Span<short> input = stackalloc short[64];
        Span<short> rows = stackalloc short[64];
        for (var i = 0; i < 64; i++)
            input[i] = unchecked((short)((long)coefficients[i] * quantization[i]));
        // Level shift belongs to the dequantized DC input. Narrow AC modulo
        // 16 bits, but saturate DC after adding the eight-bit sample midpoint.
        input[0] = (short)Math.Clamp((long)coefficients[0] * quantization[0] + 1024, short.MinValue, short.MaxValue);
        for (var y = 0; y < 8; y++)
        {
            var row = input.Slice(y * 8, 8);
            if (!row[1..].ContainsAnyExcept((short)0))
            {
                rows.Slice(y * 8, 8).Fill(unchecked((short)(row[0] * 8)));
                continue;
            }
            for (var x = 0; x < 8; x++)
            {
                var sum = 1 << 10;
                for (var u = 0; u < 8; u++)
                    sum = unchecked(sum + row[u] * Kernel[x * 8 + u]);
                rows[y * 8 + x] = (short)Math.Clamp(sum >> 11, short.MinValue, short.MaxValue);
            }
        }
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            // The reference SIMD path adds the column bias in signed words
            // before multiplication. Widening that addition changes extremes.
            var biased = unchecked((short)(rows[x] + 32));
            var sum = biased * Kernel[y * 8];
            for (var v = 1; v < 8; v++)
                sum = unchecked(sum + rows[v * 8 + x] * Kernel[y * 8 + v]);
            pixels[y * 8 + x] = (byte)Math.Clamp(sum >> 20, 0, 255);
        }
    }
}
