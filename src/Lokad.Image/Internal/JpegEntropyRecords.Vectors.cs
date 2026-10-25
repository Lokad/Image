using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal sealed partial class JpegEntropyRecords
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Prepare16(ReadOnlySpan<short> values, Span<int> destination)
    {
        var signed = Avx512F.ConvertToVector512Int32(Vector256.Create(values));
        var magnitude = Avx512F.Abs(signed).AsInt32();
        var bits = Avx512F.Subtract(Vector512.Create(32), Avx512CD.LeadingZeroCount(magnitude));
        var negative = Avx512F.CompareGreaterThan(Vector512<int>.Zero, signed);
        var adjustment = Avx512F.Subtract(Avx512F.ShiftLeftLogicalVariable(Vector512.Create(1), bits.AsUInt32()), Vector512.Create(1));
        var payload = Avx512F.Add(signed, Avx512F.And(adjustment, negative));
        var record = Avx512F.Or(Avx512F.And(payload, Vector512.Create(65535)),
            Avx512F.Or(Avx512F.ShiftLeftLogical(bits, 16), Avx512F.ShiftLeftLogical(bits, 27)));
        record.CopyTo(destination);
    }

    // The caller has already selected AVX512F/CD and a complete independent
    // block. The256B stack result fits the existing8192B JPEG fixed allowance.
    private static void PrepareRecordValues(ReadOnlySpan<short> coefficients, Span<int> destination)
    {
        for (var at = 0; at < 64; at += 16) Prepare16(coefficients.Slice(at, 16), destination.Slice(at, 16));
    }
}
