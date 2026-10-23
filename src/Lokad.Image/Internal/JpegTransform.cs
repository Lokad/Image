using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static class JpegTransform
{
    // T.81 diagonal scan order, generated from its geometry.
    public static readonly int[] Zigzag = BuildZigzag();

    private static int[] BuildZigzag()
    {
        var result = new int[64]; var index = 0;
        for (var diagonal = 0; diagonal <= 14; diagonal++)
        {
            var first = Math.Max(0, diagonal - 7); var last = Math.Min(7, diagonal);
            for (var position = first; position <= last; position++)
            {
                var row = (diagonal & 1) == 0 ? last - (position - first) : position;
                var column = diagonal - row;
                result[index++] = row * 8 + column;
            }
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void RoundSamples(Vector512<double> samples, Span<byte> pixels)
    {
        if (pixels.Length != 8) throw new ArgumentException("JPEG sample rows have eight pixels.", nameof(pixels));
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported)
        {
            // Transform sums are finite. Clipping before rounding is equivalent
            // at the byte endpoints. Within [0,255], floor and an exact
            // half-fraction comparison implement away-from-zero ties without
            // adding 0.5 (which can move a value just below a tie onto it).
            var clipped = Avx512F.Min(Avx512F.Max(samples, Vector512<double>.Zero), Vector512.Create(255.0));
            var integral = Avx512F.RoundScale(clipped, 0x09); // floor, suppress precision exception
            var aboveHalf = Avx512F.CompareGreaterThanOrEqual(Avx512F.Subtract(clipped, integral), Vector512.Create(0.5));
            var increment = Avx512F.And(aboveHalf.AsInt64(), Vector512.Create(1.0).AsInt64()).AsDouble();
            var rounded = Avx512F.Add(integral, increment);
            Avx512F.VL.ConvertToVector128Byte(Avx512F.ConvertToVector256Int32WithTruncation(rounded)).GetLower().CopyTo(pixels);
            return;
        }
        for (var x = 0; x < 8; x++)
            pixels[x] = (byte)Math.Clamp(Math.Round(samples.GetElement(x), MidpointRounding.AwayFromZero), 0, 255);
    }

}
