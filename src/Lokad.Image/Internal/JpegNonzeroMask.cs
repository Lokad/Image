using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static class JpegNonzeroMask
{
    // T.81 diagonal geometry, generated independently. Compiled format data;
    // no managed table, cache, allocation or per-block admission is introduced.
    private static ReadOnlySpan<short> Scan => [
        0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5,
        12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63];

    internal static ulong Ac(ReadOnlySpan<short> coefficients)
    {
        var low = Vector512.Create(coefficients); var high = Vector512.Create(coefficients[32..]);
        var first = Avx512BW.PermuteVar32x16x2(low, Vector512.Create(Scan), high);
        var last = Avx512BW.PermuteVar32x16x2(low, Vector512.Create(Scan[32..]), high);
        var a = Vector512.ExtractMostSignificantBits(Vector512.Equals(first, Vector512<short>.Zero));
        var b = Vector512.ExtractMostSignificantBits(Vector512.Equals(last, Vector512<short>.Zero));
        return ((~a & uint.MaxValue) | ((~b & uint.MaxValue) << 32)) & ~1UL;
    }
}
