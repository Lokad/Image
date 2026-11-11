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

    // One bit per actual dequantized word in natural order, including DC.
    // Prepare this once for the inverse's DC, row and two-row decisions.
    internal static ulong Natural(ReadOnlySpan<short> coefficients)
    {
        if (Avx512BW.IsSupported)
        {
            var low = Vector512.Equals(Vector512.Create(coefficients), Vector512<short>.Zero);
            var high = Vector512.Equals(Vector512.Create(coefficients[32..]), Vector512<short>.Zero);
            return (~Vector512.ExtractMostSignificantBits(low) & uint.MaxValue)
                | ((~Vector512.ExtractMostSignificantBits(high) & uint.MaxValue) << 32);
        }
        ulong mask = 0;
        for (var i = 0; i < 4; i++)
        {
            var zero = Avx2.CompareEqual(Vector256.Create(coefficients[(i * 16)..]), Vector256<short>.Zero);
            var bits = ~(uint)Avx2.MoveMask(zero.AsByte());
            uint words;
            if (Bmi2.IsSupported) words = Bmi2.ParallelBitExtract(bits, 0x55555555u);
            else
            {
                // Each word comparison duplicates its bit in two byte lanes.
                // Keep even lanes, then compact adjacent groups of 2/4/8/16.
                words = bits & 0x55555555u;
                words = (words | (words >> 1)) & 0x33333333u;
                words = (words | (words >> 2)) & 0x0f0f0f0fu;
                words = (words | (words >> 4)) & 0x00ff00ffu;
                words = (words | (words >> 8)) & 0x0000ffffu;
            }
            mask |= (ulong)words << (i * 16);
        }
        return mask;
    }

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
