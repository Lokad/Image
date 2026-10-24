// Independent even/odd integer factorization of the pinned grayscale DCT.
// Round cosine rotations to Q13 before combining them; preserve both fixed-point
// stage boundaries. No upstream source/table is incorporated. ITU-T T.81 A.3.3
// defines the transform; the rounded linear map is independently qualified.
namespace Lokad.Image.Internal;

internal static class JpegGrayReferenceTransform
{
    private static int Fixed(double value) => (int)Math.Round(value * 8192, MidpointRounding.AwayFromZero);
    private static readonly double Root = Math.Sqrt(2), C1 = Math.Cos(Math.PI / 16), C3 = Math.Cos(3 * Math.PI / 16),
        C5 = Math.Cos(5 * Math.PI / 16), C7 = Math.Cos(7 * Math.PI / 16);
    private static readonly int Rotation = Fixed(Root * Math.Cos(3 * Math.PI / 8)),
        Near = Fixed(2 * Math.Cos(3 * Math.PI / 8)), Far = Fixed(2 * Math.Cos(Math.PI / 8));
    private static readonly int OddBase = Fixed(Root * C3), Odd03 = Fixed(Root * (C7 - C3)),
        Odd12 = Fixed(Root * (-C1 - C3)), Odd02 = Fixed(Root * (-C3 - C5)), Odd13 = Fixed(Root * (C5 - C3));
    private static readonly int Extra0 = Fixed(Root * (C1 + C3 - C5 - C7)), Extra1 = Fixed(Root * (C1 + C3 + C5 - C7)),
        Extra2 = Fixed(Root * (C1 + C3 - C5 + C7)), Extra3 = Fixed(Root * (-C1 + C3 + C5 - C7));

    private static void TransformLine(int x0, int x1, int x2, int x3, int x4, int x5, int x6, int x7,
        Span<int> output, int stride, int bias, int shift)
    {
        var a0 = x0 + x7; var a1 = x1 + x6; var a2 = x2 + x5; var a3 = x3 + x4;
        var t0 = a0 + a3; var t1 = a1 + a2; var t2 = a1 - a2; var t3 = a0 - a3;
        var rotation = Rotation * (t2 + t3);
        var d0 = x3 - x4; var d1 = x2 - x5; var d2 = x1 - x6; var d3 = x0 - x7;
        var common = OddBase * (d0 + d1 + d2 + d3);
        var p = Odd03 * (d0 + d3); var q = Odd12 * (d1 + d2);
        var r = Odd02 * (d0 + d2); var s = Odd13 * (d1 + d3);
        // Each complete integer sum is identical to the Q13 kernel expansion.
        // Bias/shift occur only after that sum, including negative half ties.
        // Every byte input bounds row magnitudes by4097; the conservative
        // column intermediate plus bias stays below1,129,411,792 < Int32.MaxValue.
        output[0] = (((t0 + t1) << 13) + bias) >> shift;
        output[stride] = (common + p + s + Extra0 * d3 + bias) >> shift;
        output[stride * 2] = (rotation + Near * t3 + bias) >> shift;
        output[stride * 3] = (common + q + r + Extra1 * d2 + bias) >> shift;
        output[stride * 4] = (((t0 - t1) << 13) + bias) >> shift;
        output[stride * 5] = (common + q + s + Extra2 * d1 + bias) >> shift;
        output[stride * 6] = (rotation - Far * t2 + bias) >> shift;
        output[stride * 7] = (common + p + r + Extra3 * d0 + bias) >> shift;
    }

    public static void Forward(ReadOnlySpan<byte> samples, Span<short> transformed)
    {
        if (samples.Length != 64 || transformed.Length != 64) throw new ArgumentException("Complete8x8 block required.");
        // Same256-byte snapshot inside the existing fixed operation admission.
        // Gather all eight column inputs before replacing that column in place;
        // all source bytes are consumed before any possibly overlapping output.
        Span<int> rows = stackalloc int[64];
        for (var y = 0; y < 8; y++)
        {
            var a = samples.Slice(y * 8, 8);
            TransformLine(a[0] - 128, a[1] - 128, a[2] - 128, a[3] - 128,
                a[4] - 128, a[5] - 128, a[6] - 128, a[7] - 128, rows[(y * 8)..], 1, 1024, 11);
        }
        for (var u = 0; u < 8; u++)
            TransformLine(rows[u], rows[8 + u], rows[16 + u], rows[24 + u], rows[32 + u], rows[40 + u], rows[48 + u], rows[56 + u],
                rows[u..], 8, 16384, 15);
        for (var p = 0; p < 64; p++) transformed[p] = (short)rows[p];
    }

    public static ushort[] Quantization(int quality)
    {
        if (quality < 1 || quality > 100) throw new ArgumentOutOfRangeException(nameof(quality));
        int scale = quality < 50 ? 5000 / quality : 200 - 2 * quality;
        var result = new ushort[64];
        for (int v = 0; v < 8; v++)
            for (int u = 0; u < 8; u++)
                result[v * 8 + u] = (ushort)Math.Clamp(((1 + 2 * (u + v) + u * v) * scale + 50) / 100, 1, 255);
        return result;
    }

}
