using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegTransform
{
    // For v=0 the frozen vertical factor is identical in every row. The
    // qualified Q16 basis/certificate therefore needs one two-row vector only.
    // The sparse caller has already admitted its original4096 work units.
    internal static bool TryInverseHorizontalInteger(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels, ulong active)
    {
        if (!Avx512F.IsSupported || !Avx512F.VL.IsSupported || (active & ~255UL) != 0) return false;
        long mass = 0, acMass = 0; var remaining = active;
        while (remaining != 0)
        {
            var index = BitOperations.TrailingZeroCount(remaining); remaining &= remaining - 1;
            var term = Math.Abs((long)coefficients[index] * quantization[index]);
            mass += term; if (index != 0) acMass += term;
            if (mass > 65536) return false;
        }
        var weights = JpegSparseBasis.Weights; var sum = Vector512.Create(128 << 16);
        remaining = active;
        while (remaining != 0)
        {
            var index = BitOperations.TrailingZeroCount(remaining); remaining &= remaining - 1;
            var sample = Vector512.Create((int)((long)coefficients[index] * quantization[index]));
            var basis = Avx512F.ConvertToVector512Int32(Vector256.Create(weights.Slice(index * 64, 16)));
            sum = Avx512F.Add(sum, Avx512F.MultiplyLow(sample, basis));
        }
        // Same signed-prefix and rounding-error domain as the qualified2D
        // kernel. Unknown/half bins return before any write to the old row path.
        if (!CertifySparseInteger(sum, ((int)acMass + 1) / 2 + 1, out var twoRows)) return false;
        var fourRows = Vector256.Create(twoRows, twoRows);
        Vector512.Create(fourRows, fourRows).CopyTo(pixels);
        return true;
    }
}
