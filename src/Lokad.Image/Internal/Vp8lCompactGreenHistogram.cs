using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// RFC 9649 section 3.5.1, specialized for A/R/B zero ALPH preparation.
// The existing analysis owner supplies cleared, non-overlapping green banks.
internal static class Vp8lCompactGreenHistogram
{
    internal static bool IsSupported => Ssse3.IsSupported && Sse41.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Add(Span<int> bins, int mode, int residual)
        => bins[mode * 256 + (residual & 255)]++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddFour(Span<int> bins, int mode, Vector128<int> residual)
    {
        residual &= Vector128.Create(255);
        bins = bins.Slice(mode * 256, 256);
        bins[residual.GetElement(0)]++; bins[residual.GetElement(1)]++;
        bins[residual.GetElement(2)]++; bins[residual.GetElement(3)]++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> Average(Vector128<int> a, Vector128<int> b) => (a + b) >> 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> Green(ReadOnlySpan<uint> pixels, int at)
        => (Vector128.Create(pixels.Slice(at, 4)) >> 8).AsInt32();

    private static void InteriorFour(Span<int> bins, Vector128<int> actual,
        Vector128<int> left, Vector128<int> top, Vector128<int> corner, Vector128<int> right)
    {
        var leftCorner = Average(left, corner); var leftTop = Average(left, top); var topRight = Average(top, right);
        AddFour(bins, 0, actual); AddFour(bins, 1, actual - left);
        AddFour(bins, 2, actual - top); AddFour(bins, 3, actual - right); AddFour(bins, 4, actual - corner);
        AddFour(bins, 5, actual - Average(Average(left, right), top));
        AddFour(bins, 6, actual - leftCorner); AddFour(bins, 7, actual - leftTop);
        AddFour(bins, 8, actual - Average(corner, top)); AddFour(bins, 9, actual - topRight);
        AddFour(bins, 10, actual - Average(leftCorner, topRight));
        var chooseLeft = Sse2.CompareGreaterThan(Ssse3.Abs(left - corner).AsInt32(), Ssse3.Abs(top - corner).AsInt32());
        var selected = (chooseLeft & left) | (~chooseLeft & top);
        AddFour(bins, 11, actual - selected);
        var zero = Vector128<int>.Zero; var maximum = Vector128.Create(255);
        AddFour(bins, 12, actual - Sse41.Min(Sse41.Max(left + top - corner, zero), maximum));
        var delta = leftTop - corner;
        // Signed half-differences truncate toward zero, unlike arithmetic shift.
        var half = (delta + (delta.AsUInt32() >> 31).AsInt32()) >> 1;
        AddFour(bins, 13, actual - Sse41.Min(Sse41.Max(leftTop + half, zero), maximum));
    }

    private static void Interior(Span<int> bins, int actual, int left, int top, int corner, int right)
    {
        var leftCorner = (left + corner) / 2; var leftTop = (left + top) / 2; var topRight = (top + right) / 2;
        Add(bins, 0, actual); Add(bins, 1, actual - left); Add(bins, 2, actual - top);
        Add(bins, 3, actual - right); Add(bins, 4, actual - corner);
        Add(bins, 5, actual - ((left + right) / 2 + top) / 2); Add(bins, 6, actual - leftCorner);
        Add(bins, 7, actual - leftTop); Add(bins, 8, actual - (corner + top) / 2); Add(bins, 9, actual - topRight);
        Add(bins, 10, actual - (leftCorner + topRight) / 2);
        Add(bins, 11, actual - (Math.Abs(top - corner) < Math.Abs(left - corner) ? left : top));
        Add(bins, 12, actual - Math.Clamp(left + top - corner, 0, 255));
        Add(bins, 13, actual - Math.Clamp(leftTop + (leftTop - corner) / 2, 0, 255));
    }

    internal static void Build(ReadOnlySpan<uint> pixels, int width, int height,
        int x0, int y0, int bits, Span<int> bins, OperationContext context)
    {
        var endX = Math.Min(x0 + (1 << bits), width); var endY = Math.Min(y0 + (1 << bits), height);
        for (var y = y0; y < endY; y++)
        {
            var x = x0;
            while (x < endX)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var at = y * width + x;
                if (y != 0 && x != 0 && x + 4 <= Math.Min(endX, width - 1))
                {
                    InteriorFour(bins, Green(pixels, at), Green(pixels, at - 1), Green(pixels, at - width),
                        Green(pixels, at - width - 1), Green(pixels, at - width + 1));
                    x += 4; continue;
                }
                var actual = (int)(pixels[at] >> 8);
                if (y == 0 || x == 0)
                {
                    var prediction = y == 0 ? x == 0 ? 0 : (int)(pixels[at - 1] >> 8) : (int)(pixels[at - width] >> 8);
                    for (var mode = 0; mode < 14; mode++) Add(bins, mode, actual - prediction);
                }
                else Interior(bins, actual, (int)(pixels[at - 1] >> 8), (int)(pixels[at - width] >> 8),
                    (int)(pixels[at - width - 1] >> 8), (int)(pixels[x == width - 1 ? y * width : at - width + 1] >> 8));
                x++;
            }
        }
    }
}
