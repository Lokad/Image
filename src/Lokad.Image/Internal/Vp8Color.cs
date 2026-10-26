using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Centered byte chroma and explicitly normalized limited-range BT.601.</summary>
internal static class Vp8Color
{
    // Derive coefficients from BT.601 Kr=299/1000, Kb=114/1000 and the
    // studio luma/chroma ranges219/224. Preserve the Q16-to-Q13 rounding.
    private const int Luma = ((65536 * 255 / 219) + 4) >> 3;
    private static readonly int Red = Inverse(1402, 1000, false);
    private static readonly int Blue = Inverse(1772, 1000, false);
    private static readonly int GreenU = Inverse(2L * 114 * 886, 1000L * 587, true);
    private static readonly int GreenV = Inverse(2L * 299 * 701, 1000L * 587, true);
    private static readonly int Yr = Forward(299, 219), Yg = Forward(587, 219), Yb = Forward(114, 219);
    private static readonly int Ur = -Forward(169, 224), Ug = -Forward(331, 224), Ub = Forward(500, 224);
    private static readonly int Vr = Forward(500, 224), Vg = -Forward(419, 224), Vb = -Forward(81, 224);

    private static int Inverse(long numerator, long denominator, bool negative)
    {
        var q16 = (numerator * 255 * 65536 + denominator * 112) / (denominator * 224);
        return (int)(((negative ? -q16 : q16) + 4) >> 3);
    }

    private static int Forward(int weight, int range)
        => (int)((weight * (long)range * 32768 + 127500) / 255000);

    public static ImageBuffer Render(Vp8Planes planes, byte[]? alpha, OperationContext context)
    {
        var result = ImageBuffer.Allocate(planes.Width, planes.Height, 8, context);
        try
        {
            for (var y = 0; y < planes.Height; y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var row = result.GetRow8(y);
                var first = Avx2.IsSupported && Sse41.IsSupported && planes.Width >= 9
                    ? WriteVectorRow(planes, alpha, y, row) : 0;
                for (var x = first; x < planes.Width; x++)
                {
                    var u = Chroma(planes.U, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
                    var v = Chroma(planes.V, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
                    WritePixel(planes.Y[y * planes.Stride + x], u, v, row.Slice(x * 4, 3));
                    row[x * 4 + 3] = alpha is null ? (byte)255 : alpha[y * planes.Width + x];
                }
            }
            context.Release(planes.MemoryBytes + (alpha is null ? 0 : alpha.Length + 32L));
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static int WriteVectorRow(Vp8Planes planes, byte[]? alpha, int y, Span<byte> output)
    {
        // Each group starts on an odd pixel. Its four adjacent odd/even pairs
        // share five chroma columns and the same two source rows.
        var width = planes.Width; var chromaWidth = (width + 1) / 2;
        var stride = planes.Stride / 2; var top = (y - 1) / 2;
        var bottom = Math.Min(top + 1, (planes.Height + 1) / 2 - 1);
        var dy = y == 0 ? 0 : (y & 1) == 0 ? 3 : 1;
        var ut = planes.U.AsSpan(top * stride, chromaWidth); var ub = planes.U.AsSpan(bottom * stride, chromaWidth);
        var vt = planes.V.AsSpan(top * stride, chromaWidth); var vb = planes.V.AsSpan(bottom * stride, chromaWidth);
        var luma = planes.Y.AsSpan(y * planes.Stride, width);
        var alphaRow = alpha is null ? ReadOnlySpan<byte>.Empty : alpha.AsSpan(y * width, width);
        WritePixel(luma[0], Chroma(planes.U, stride, chromaWidth, (planes.Height + 1) / 2, 0, y) - 128,
            Chroma(planes.V, stride, chromaWidth, (planes.Height + 1) / 2, 0, y) - 128, output[..3]);
        output[3] = alpha is null ? (byte)255 : alphaRow[0];
        var x = 1;
        for (; x <= width - 8; x += 8)
        {
            var u = Chroma8(ut, ub, (x - 1) / 2, dy); var v = Chroma8(vt, vb, (x - 1) / 2, dy);
            var a = alpha is null ? Vector256.Create(255)
                : Avx2.ConvertToVector256Int32(Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(alphaRow.Slice(x, 8))).AsByte());
            WritePixels8(luma.Slice(x, 8), u, v, a, output.Slice(x * 4, 32));
        }
        return x;
    }

    private static Vector128<int> Chroma4(ReadOnlySpan<byte> row, int x)
        => Sse41.ConvertToVector128Int32(Vector128.CreateScalar(BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(x, 4))).AsByte());

    private static Vector256<int> Chroma8(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom, int x, int dy)
    {
        var a = Chroma4(top, x) * (4 - dy) + Chroma4(bottom, x) * dy;
        var b = Chroma4(top, x + 1) * (4 - dy) + Chroma4(bottom, x + 1) * dy;
        var odd = (a * 3 + b + Vector128.Create(8)) >> 4;
        var even = (a + b * 3 + Vector128.Create(8)) >> 4;
        return Vector256.Create(Sse2.UnpackLow(odd, even), Sse2.UnpackHigh(odd, even)) - Vector256.Create(128);
    }

    private static Vector256<int> Clip16(Vector256<int> value)
        => Vector256.Min(Vector256.Max(value, Vector256<int>.Zero), Vector256.Create(65535));

    private static Vector256<int> Sample8(Vector256<int> value)
        => Vector256.Min(Vector256.Max((value + Vector256.Create(1 << 19)) >> 20, Vector256<int>.Zero), Vector256.Create(255));

    private static void WritePixels8(ReadOnlySpan<byte> luma, Vector256<int> u, Vector256<int> v,
        Vector256<int> alpha, Span<byte> output)
    {
        var y = (Avx2.ConvertToVector256Int32(Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(luma)).AsByte()) - Vector256.Create(16)) * Luma;
        var red = Clip16((y + v * Red + Vector256.Create(16)) >> 5);
        var green = Clip16((y + u * GreenU + v * GreenV + Vector256.Create(16)) >> 5);
        var blue = Clip16((y + u * Blue + Vector256.Create(16)) >> 5);
        // The complete saturated RGB16 cube bounds every product, partial sum
        // and bias to signed32: Y<=1,978,454,547, U/V<=2,016,937,928.
        // Thus low-word SIMD multiplication preserves the scalar Int64 result.
        var studioY = (red * Yr + green * Yg + blue * Yb + Vector256.Create(8193 << 14)) >> 15;
        var studioU = (red * Ur + green * Ug + blue * Ub + Vector256.Create(65537 << 14)) >> 15;
        var studioV = (red * Vr + green * Vg + blue * Vb + Vector256.Create(65537 << 14)) >> 15;
        y = ((studioY >> 1) - Vector256.Create(2048)) * Luma;
        u = (studioU >> 1) - Vector256.Create(16384); v = (studioV >> 1) - Vector256.Create(16384);
        red = Sample8(y + v * Red); green = Sample8(y + u * GreenU + v * GreenV); blue = Sample8(y + u * Blue);
        (red | green << 8 | blue << 16 | alpha << 24).AsByte().CopyTo(output);
    }

    private static void WritePixel(byte luma, int u, int v, Span<byte> output)
    {
        // The approved saturated16-bit RGB normalization is followed by a Q15
        // studio-YUV round trip before reduction to bytes. Keeping those
        // intermediate precision steps avoids a different half-sample policy.
        // All intermediates are scalars; no converted frame or row is stored.
        var y = (luma - 16) * Luma;
        var red = Math.Clamp((y + v * Red + 16) >> 5, 0, 65535);
        var green = Math.Clamp((y + u * GreenU + v * GreenV + 16) >> 5, 0, 65535);
        var blue = Math.Clamp((y + u * Blue + 16) >> 5, 0, 65535);
        var studioY = (Yr * (long)red + Yg * (long)green + Yb * (long)blue + (8193L << 14)) >> 15;
        var studioU = (Ur * (long)red + Ug * (long)green + Ub * (long)blue + (65537L << 14)) >> 15;
        var studioV = (Vr * (long)red + Vg * (long)green + Vb * (long)blue + (65537L << 14)) >> 15;
        // Truncate16-bit samples to15bits before the final Q13 conversion.
        y = (int)((studioY >> 1) - 2048) * Luma;
        u = (int)(studioU >> 1) - 16384;
        v = (int)(studioV >> 1) - 16384;
        output[0] = Sample(y + v * Red);
        output[1] = Sample(y + u * GreenU + v * GreenV);
        output[2] = Sample(y + u * Blue);
    }

    private static byte Sample(int value) => (byte)Math.Clamp((value + (1 << 19)) >> 20, 0, 255);

    private static int Chroma(byte[] plane, int stride, int width, int height, int x, int y)
    {
        // Render supplies nonnegative coordinates within the doubled chroma
        // extent. Centered phases are 1/4 on odd pixels and 3/4 on even pixels;
        // the first pixel clamps to phase zero. Integer division maps -1/2 to
        // zero here, while duplicated last samples provide the other edge.
        var left = (x - 1) / 2; var top = (y - 1) / 2;
        var right = Math.Min(left + 1, width - 1); var bottom = Math.Min(top + 1, height - 1);
        var dx = x == 0 ? 0 : (x & 1) == 0 ? 3 : 1;
        var dy = y == 0 ? 0 : (y & 1) == 0 ? 3 : 1;
        var first = plane[top * stride + left] * (4 - dx) + plane[top * stride + right] * dx;
        var second = plane[bottom * stride + left] * (4 - dx) + plane[bottom * stride + right] * dx;
        // The weighted sum is at most 255*16. Round the exact /16 expansion
        // before the normalized integer color conversion, with half-up rounding.
        return (first * (4 - dy) + second * dy + 8) >> 4;
    }

}
