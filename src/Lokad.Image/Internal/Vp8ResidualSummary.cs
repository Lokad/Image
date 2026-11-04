using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Lokad.Image.Internal;

// RFC6386 residual termination, shared by statistics and final partition coding.
// Callers supply sixteen borrowed levels and first=0/1. No retained state.
internal static class Vp8ResidualSummary
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Last(ReadOnlySpan<short> levels, int first, bool validate)
    {
        if (Vector256.IsHardwareAccelerated)
        {
            var samples = Vector256.Create(levels);
            if (validate && (Vector256.GreaterThanAny(samples, Vector256.Create((short)2047))
                || Vector256.LessThanAny(samples, Vector256.Create((short)-2047))))
                throw new ArgumentException("VP8 residual level exceeds 2,047.");
            var mask = ~Vector256.ExtractMostSignificantBits(Vector256.Equals(samples, Vector256<short>.Zero)) & 0xffffu;
            mask &= uint.MaxValue << first;
            return 31 - BitOperations.LeadingZeroCount(mask);
        }
        var last = -1;
        for (var p = first; p < 16; p++)
        {
            if (validate && levels[p] is < -2047 or > 2047)
                throw new ArgumentException("VP8 residual level exceeds 2,047.");
            if (levels[p] != 0) last = p;
        }
        return last;
    }
}
