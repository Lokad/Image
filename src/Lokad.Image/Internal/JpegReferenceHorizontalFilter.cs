using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// The existing area plans supply every tap and weight. Prepare their selectors
// once per MCU, then filter eight interleaved Cb/Cr pairs together. All vectors
// borrow the admitted MCU stack workspace; no frame, cache or owner is added.
internal readonly struct JpegReferenceHorizontalFilter
{
    private readonly Vector512<short> indices0, indices1, indices2;
    private readonly Vector512<int> weights0, weights1, weights2;

    internal JpegReferenceHorizontalFilter(ReadOnlySpan<JpegReferenceArea> plans, int firstX, bool pair)
    {
        Span<short> indices = stackalloc short[32]; indices.Clear();
        Span<int> weights = stackalloc int[16];
        Prepare(plans, firstX, pair, 0, indices, weights);
        indices0 = Vector512.Create(indices); weights0 = Vector512.Create(weights);
        Prepare(plans, firstX, pair, 1, indices, weights);
        indices1 = Vector512.Create(indices); weights1 = Vector512.Create(weights);
        Prepare(plans, firstX, pair, 2, indices, weights);
        indices2 = Vector512.Create(indices); weights2 = Vector512.Create(weights);
    }

    private static void Prepare(ReadOnlySpan<JpegReferenceArea> plans, int firstX, bool pair, int tap,
        Span<short> indices, Span<int> weights)
    {
        for (var x = 0; x < 8; x++)
        {
            var present = tap < plans[x].Count;
            var source = present ? plans[x].Source(tap) * (pair ? 2 : 1) - firstX : 0;
            indices[x * 2] = checked((short)(source * 2));
            indices[x * 2 + 1] = checked((short)(source * 2 + 1));
            weights[x * 2] = weights[x * 2 + 1] = present ? plans[x].Weight(tap) : 0;
        }
    }

    // Called only for initialized Q6 rows of 16..18 packed pixel pairs, on
    // little-endian AVX-512BW hardware. Selectors never address a missing tail.
    internal void Apply(ReadOnlySpan<int> row, Span<short> output)
    {
        var source = MemoryMarshal.Read<Vector512<short>>(MemoryMarshal.AsBytes(row));
        var tail = Vector512.CreateScalar(row.Length > 16 ? row[16] : 0)
            .WithElement(1, row.Length > 17 ? row[17] : 0).AsInt16();
        var a = Avx512F.ConvertToVector512Int32(Avx512BW.PermuteVar32x16x2(source, indices0, tail).GetLower());
        var b = Avx512F.ConvertToVector512Int32(Avx512BW.PermuteVar32x16x2(source, indices1, tail).GetLower());
        var c = Avx512F.ConvertToVector512Int32(Avx512BW.PermuteVar32x16x2(source, indices2, tail).GetLower());
        // Positive Q6 samples and Q14 weights sum to at most 15360*16384.
        // Keep the scalar Q13 truncation, Q7 range conversion and upper clips.
        var sum = Avx512F.MultiplyLow(a, weights0) + Avx512F.MultiplyLow(b, weights1) + Avx512F.MultiplyLow(c, weights2);
        var limited = Avx512F.Min(Vector512.Create(32767), Avx512F.ShiftRightArithmetic(sum, 13));
        const int scale = (255 * 16384 + 223) / 224;
        const int offset = (255 << 21) - ((16 + 224) << 7) * scale + 8192;
        var full = Avx512F.Min(Vector512.Create(32767),
            Avx512F.ShiftRightArithmetic(Avx512F.MultiplyLow(limited, Vector512.Create(scale)) + Vector512.Create(offset), 14));
        // Signed Q7 includes the -1 guard sample; it must not clamp to a byte.
        Avx512F.ConvertToVector256Int16(full).CopyTo(output);
    }
}
