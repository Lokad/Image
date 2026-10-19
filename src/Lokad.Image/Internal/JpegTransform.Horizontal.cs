using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegTransform
{
    // With no vertical AC term, every frozen vertical basis factor is C(0).
    // Evaluate the original ordered sum once; every output row is bit-identical.
    // The caller has validated spans and charged the original full block work.
    private static void InverseHorizontalRow(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels, ulong active)
    {
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported)
        {
            var sum = Vector512<double>.Zero; var remaining = active;
            var vertical = Vector512.Create(Cosines[0]);
            while (remaining != 0)
            {
                var u = BitOperations.TrailingZeroCount(remaining); remaining &= remaining - 1;
                var sample = Vector512.Create((double)coefficients[u] * quantization[u]);
                var horizontal = Avx512F.Multiply(sample, Vector512.Create(InverseCosines.AsSpan(u * 8, 8)));
                sum = Avx512F.Add(sum, Avx512F.Multiply(horizontal, vertical));
            }
            RoundSamples(Avx512F.Add(Avx512F.Multiply(sum, Vector512.Create(.25)), Vector512.Create(128d)), pixels[..8]);
        }
        else
        {
            for (var x = 0; x < 8; x++)
            {
                double sum = 0; var remaining = active;
                while (remaining != 0)
                {
                    var u = BitOperations.TrailingZeroCount(remaining); remaining &= remaining - 1;
                    sum += (double)coefficients[u] * quantization[u] * Cosines[x * 8 + u] * Cosines[0];
                }
                pixels[x] = (byte)Math.Clamp(Math.Round(sum * .25 + 128, MidpointRounding.AwayFromZero), 0, 255);
            }
        }
        for (var y = 1; y < 8; y++) pixels[..8].CopyTo(pixels.Slice(y * 8, 8));
    }
}
