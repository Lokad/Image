using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegTransform
{
    // Only the sparse materializer calls this, after its Work(4096). No new
    // storage: 8KiB original module data and four independent integer vectors.
    internal static bool TryInverseSparseInteger(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels, ulong active)
    {
        if (!Avx512F.IsSupported || !Avx512F.VL.IsSupported || BitOperations.PopCount(active) > 8) return false;
        long mass = 0, acMass = 0; var remaining = active;
        while (remaining != 0)
        {
            var index = BitOperations.TrailingZeroCount(remaining); remaining &= remaining - 1;
            var term = Math.Abs((long)coefficients[index] * quantization[index]);
            mass += term; if (index != 0) acMass += term;
            if (mass > 65536) return false;
        }
        // Q=2^16, |W|<2^14, S<=2^16: every signed product/prefix, the128Q
        // offset and half-unit addition fit Int32. Non-DC basis quantization
        // error is <=S_AC/2 Q units. Frozen DC quantization plus the original
        // ordered binary64 evaluation error is <1 Q unit throughout this domain.
        var error = ((int)acMass + 1) / 2 + 1;
        var weights = JpegSparseBasis.Weights;
        Vector512<int> s0 = Vector512.Create(128 << 16), s1 = s0, s2 = s0, s3 = s0;
        remaining = active;
        while (remaining != 0)
        {
            var index = BitOperations.TrailingZeroCount(remaining); remaining &= remaining - 1;
            var sample = Vector512.Create((int)((long)coefficients[index] * quantization[index])); var at = index * 64;
            s0 = Avx512F.Add(s0, Avx512F.MultiplyLow(sample, Avx512F.ConvertToVector512Int32(Vector256.Create(weights.Slice(at, 16)))));
            s1 = Avx512F.Add(s1, Avx512F.MultiplyLow(sample, Avx512F.ConvertToVector512Int32(Vector256.Create(weights.Slice(at + 16, 16)))));
            s2 = Avx512F.Add(s2, Avx512F.MultiplyLow(sample, Avx512F.ConvertToVector512Int32(Vector256.Create(weights.Slice(at + 32, 16)))));
            s3 = Avx512F.Add(s3, Avx512F.MultiplyLow(sample, Avx512F.ConvertToVector512Int32(Vector256.Create(weights.Slice(at + 48, 16)))));
        }
        // Strictly certify the entire block before any write. Half ties and
        // uncertain intervals keep the original kernel and away-from-zero rule.
        var certain = CertifySparseInteger(s0, error, out var p0) & CertifySparseInteger(s1, error, out var p1)
            & CertifySparseInteger(s2, error, out var p2) & CertifySparseInteger(s3, error, out var p3);
        if (!certain) return false;
        p0.CopyTo(pixels[..16]); p1.CopyTo(pixels.Slice(16, 16)); p2.CopyTo(pixels.Slice(32, 16)); p3.CopyTo(pixels.Slice(48, 16));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool CertifySparseInteger(Vector512<int> samples, int error, out Vector128<byte> pixels)
    {
        var clipped = Avx512F.Min(Avx512F.Max(samples, Vector512<int>.Zero), Vector512.Create(255 << 16));
        var fraction = Avx512F.And(clipped, Vector512.Create(65535));
        var distance = Avx512F.Abs(Avx512F.Subtract(fraction, Vector512.Create(32768))).AsInt32();
        var certain = Vector512.ExtractMostSignificantBits(Avx512F.CompareGreaterThan(distance, Vector512.Create(error))) == 65535;
        var rounded = Avx512F.ShiftRightLogical(Avx512F.Add(clipped, Vector512.Create(32768)).AsUInt32(), 16).AsInt32();
        pixels = Avx512F.ConvertToVector128Byte(rounded);
        return certain;
    }
}
