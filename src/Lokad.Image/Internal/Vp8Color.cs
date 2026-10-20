using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Explicit centered chroma expansion and limited-range BT.601 conversion.</summary>
internal static class Vp8Color
{
    internal static int SourceError(Vp8Planes planes, int x, int y, int red, int green, int blue)
    {
        var luma = (planes.Y[y * planes.Stride + x] - 16) * (255.0 / 219);
        var u = Chroma(planes.U, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
        var v = Chroma(planes.V, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
        var r = red - Sample(luma + 1.596026785714286 * v);
        var g = green - Sample(luma - 0.391762290094914 * u - 0.812967647237771 * v);
        var b = blue - Sample(luma + 2.017232142857143 * u);
        return r * r + g * g + b * b;
    }

    public static ImageBuffer Render(Vp8Planes planes, byte[]? alpha, OperationContext context)
    {
        var result = ImageBuffer.Allocate(planes.Width, planes.Height, 8, context);
        try
        {
            var blocks = Avx512F.IsSupported && Avx2.IsSupported && Ssse3.IsSupported && BitConverter.IsLittleEndian;
            var chromaWidth = (planes.Width + 1) / 2;
            for (var y = 0; y < planes.Height; y++)
            {
                context.Work(planes.Width * 64L);
                var row = result.GetRow8(y);
                for (var x = 0; x < planes.Width; x++)
                {
                    // Interior blocks start at even x. Eight bounded chroma
                    // reads cover six selected columns, without reading padding.
                    if (blocks && x >= 2 && (x & 1) == 0 && x + 8 <= planes.Width
                        && (x - 1) / 2 + 8 <= chromaWidth)
                    {
                        Render8(planes, alpha, row.Slice(x * 4, 32), x, y);
                        x += 7;
                        continue;
                    }
                    var luma = (planes.Y[y * planes.Stride + x] - 16) * (255.0 / 219);
                    var u = Chroma(planes.U, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
                    var v = Chroma(planes.V, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
                    row[x * 4] = Sample(luma + 1.596026785714286 * v);
                    row[x * 4 + 1] = Sample(luma - 0.391762290094914 * u - 0.812967647237771 * v);
                    row[x * 4 + 2] = Sample(luma + 2.017232142857143 * u);
                    row[x * 4 + 3] = alpha is null ? (byte)255 : alpha[y * planes.Width + x];
                }
            }
            context.Release(planes.MemoryBytes + (alpha is null ? 0 : alpha.Length + 32L));
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void Render8(Vp8Planes planes, byte[]? alpha, Span<byte> output, int x, int y)
    {
        var luma = Avx512F.Multiply(Avx512F.Subtract(
            Avx512F.ConvertToVector512Double(Avx2.ConvertToVector256Int32(Bytes8(planes.Y.AsSpan(y * planes.Stride + x, 8)))),
            Vector512.Create(16.0)), Vector512.Create(255.0 / 219));
        var top = (y - 1) / 2; var bottom = Math.Min(top + 1, (planes.Height + 1) / 2 - 1);
        var dy = y == 0 ? 0 : (y & 1) == 0 ? 3 : 1;
        var left = (x - 1) / 2; var stride = planes.Stride / 2;
        var u = Avx512F.Subtract(Avx512F.ConvertToVector512Double(Chroma8(planes.U, stride, left, top, bottom, dy)), Vector512.Create(128.0));
        var v = Avx512F.Subtract(Avx512F.ConvertToVector512Double(Chroma8(planes.V, stride, left, top, bottom, dy)), Vector512.Create(128.0));
        // Separate double operations retain the scalar BT.601 expression order;
        // fused multiply-add would change rounding near a half-sample boundary.
        var red = Avx512F.Add(luma, Avx512F.Multiply(v, Vector512.Create(1.596026785714286)));
        var green = Avx512F.Subtract(Avx512F.Subtract(luma, Avx512F.Multiply(u, Vector512.Create(0.391762290094914))),
            Avx512F.Multiply(v, Vector512.Create(0.812967647237771)));
        var blue = Avx512F.Add(luma, Avx512F.Multiply(u, Vector512.Create(2.017232142857143)));
        var opacity = alpha is null ? Vector256.Create(255u)
            : Avx2.ConvertToVector256Int32(Bytes8(alpha.AsSpan(y * planes.Width + x, 8))).AsUInt32();
        var rgba = Samples8(red) | (Samples8(green) << 8) | (Samples8(blue) << 16) | (opacity << 24);
        rgba.AsByte().CopyTo(output);
    }

    private static Vector128<byte> Bytes8(ReadOnlySpan<byte> bytes)
        => Vector128.Create(MemoryMarshal.Read<ulong>(bytes)).AsByte();

    private static Vector256<int> Chroma8(byte[] plane, int stride, int left, int top, int bottom, int dy)
    {
        var vertical = Vector128.WidenLower(Bytes8(plane.AsSpan(top * stride + left, 8))) * Vector128.Create((ushort)(4 - dy))
            + Vector128.WidenLower(Bytes8(plane.AsSpan(bottom * stride + left, 8))) * Vector128.Create((ushort)dy);
        // Centered phases alternate 3/4,1/4 across the eight even-start pixels.
        // These indices follow their coordinate geometry, not a codec table.
        var first = Vector128.Shuffle(vertical, Vector128.Create((ushort)0, 1, 1, 2, 2, 3, 3, 4));
        var second = Vector128.Shuffle(vertical, Vector128.Create((ushort)1, 2, 2, 3, 3, 4, 4, 5));
        var expanded = (first * Vector128.Create((ushort)1, 3, 1, 3, 1, 3, 1, 3)
            + second * Vector128.Create((ushort)3, 1, 3, 1, 3, 1, 3, 1) + Vector128.Create((ushort)8)) >> 4;
        return Avx2.ConvertToVector256Int32(expanded.AsInt16());
    }

    private static Vector256<uint> Samples8(Vector512<double> values)
    {
        var rounded = Vector512.Floor(Avx512F.Add(values, Vector512.Create(0.5)));
        var clipped = Avx512F.Min(Avx512F.Max(rounded, Vector512<double>.Zero), Vector512.Create(255.0));
        return Avx512F.ConvertToVector256Int32WithTruncation(clipped).AsUInt32();
    }

    private static double Chroma(byte[] plane, int stride, int width, int height, int x, int y)
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
        // before the unchanged color matrix, with the original half-up rule.
        return (first * (4 - dy) + second * dy + 8) >> 4;
    }

    private static byte Sample(double value) => (byte)Math.Clamp((int)Math.Floor(value + 0.5), 0, 255);
}
