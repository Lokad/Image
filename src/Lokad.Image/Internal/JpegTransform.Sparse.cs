using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegTransform
{
    // Original T.81 evaluation order, omitting only exactly zero terms. Eight
    // active frequencies bound the direct walk to one eighth of a dense block.
    // Only the private owned-CMYK materializer selects this path.
    internal static bool TryInverseSparse(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels, OperationContext context)
    {
        if (coefficients.Length != 64 || quantization.Length != 64 || pixels.Length != 64)
            throw new ArgumentException("JPEG transforms operate on complete 8x8 blocks.");
        ulong active = 0;
        if (Avx512F.IsSupported)
        {
            for (var i = 0; i < 64; i += 16)
            {
                var zero = Vector512.ExtractMostSignificantBits(Avx512F.CompareEqual(Vector512.Create(coefficients.Slice(i, 16)), Vector512<int>.Zero));
                active |= (ulong)(~zero & 65535u) << i;
            }
        }
        else
        {
            var count = 0;
            for (var i = 0; i < 64; i++)
                if (coefficients[i] != 0) { if (++count > 8) return false; active |= 1UL << i; }
        }
        if (BitOperations.PopCount(active) > 8) return false;
        context.Work(4096);
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported)
        {
            Vector512<double> sum0 = Vector512<double>.Zero, sum1 = Vector512<double>.Zero,
                sum2 = Vector512<double>.Zero, sum3 = Vector512<double>.Zero,
                sum4 = Vector512<double>.Zero, sum5 = Vector512<double>.Zero,
                sum6 = Vector512<double>.Zero, sum7 = Vector512<double>.Zero;
            var remaining = active;
            while (remaining != 0)
            {
                var index = BitOperations.TrailingZeroCount(remaining); remaining &= remaining - 1;
                var v = index / 8; var u = index % 8;
                var sample = Vector512.Create((double)coefficients[index] * quantization[index]);
                var horizontal = Avx512F.Multiply(sample, Vector512.Create(InverseCosines.AsSpan(u * 8, 8)));
                sum0 = Avx512F.Add(sum0, Avx512F.Multiply(horizontal, Vector512.Create(Cosines[v])));
                sum1 = Avx512F.Add(sum1, Avx512F.Multiply(horizontal, Vector512.Create(Cosines[8 + v])));
                sum2 = Avx512F.Add(sum2, Avx512F.Multiply(horizontal, Vector512.Create(Cosines[16 + v])));
                sum3 = Avx512F.Add(sum3, Avx512F.Multiply(horizontal, Vector512.Create(Cosines[24 + v])));
                sum4 = Avx512F.Add(sum4, Avx512F.Multiply(horizontal, Vector512.Create(Cosines[32 + v])));
                sum5 = Avx512F.Add(sum5, Avx512F.Multiply(horizontal, Vector512.Create(Cosines[40 + v])));
                sum6 = Avx512F.Add(sum6, Avx512F.Multiply(horizontal, Vector512.Create(Cosines[48 + v])));
                sum7 = Avx512F.Add(sum7, Avx512F.Multiply(horizontal, Vector512.Create(Cosines[56 + v])));
            }
            var scale = Vector512.Create(.25); var offset = Vector512.Create(128d);
            RoundSamples(Avx512F.Add(Avx512F.Multiply(sum0, scale), offset), pixels[..8]);
            RoundSamples(Avx512F.Add(Avx512F.Multiply(sum1, scale), offset), pixels.Slice(8, 8));
            RoundSamples(Avx512F.Add(Avx512F.Multiply(sum2, scale), offset), pixels.Slice(16, 8));
            RoundSamples(Avx512F.Add(Avx512F.Multiply(sum3, scale), offset), pixels.Slice(24, 8));
            RoundSamples(Avx512F.Add(Avx512F.Multiply(sum4, scale), offset), pixels.Slice(32, 8));
            RoundSamples(Avx512F.Add(Avx512F.Multiply(sum5, scale), offset), pixels.Slice(40, 8));
            RoundSamples(Avx512F.Add(Avx512F.Multiply(sum6, scale), offset), pixels.Slice(48, 8));
            RoundSamples(Avx512F.Add(Avx512F.Multiply(sum7, scale), offset), pixels.Slice(56, 8)); return true;
        }
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            double sum = 0; var remaining = active;
            while (remaining != 0)
            {
                var index = BitOperations.TrailingZeroCount(remaining); remaining &= remaining - 1;
                sum += (double)coefficients[index] * quantization[index] * Cosines[x * 8 + index % 8] * Cosines[y * 8 + index / 8];
            }
            pixels[y * 8 + x] = (byte)Math.Clamp(Math.Round(sum * .25 + 128, MidpointRounding.AwayFromZero), 0, 255);
        }
        return true;
    }
}
