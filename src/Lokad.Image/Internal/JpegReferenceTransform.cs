// Independent integer factorization of the eight-point DCT. Tangent rotations
// and cosine row weights are generated mathematically; no implementation tables
// are imported. Integer scale, high-product truncation, odd corrections and final
// rounding intentionally match the pinned forward stage.
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static class JpegReferenceTransform
{
    private static readonly int Tangent1 = Fixed(Math.Tan(Math.PI / 16), 16);
    private static readonly int Tangent2 = Fixed(Math.Tan(Math.PI / 8), 16);
    private static readonly int Tangent3 = Fixed(Math.Tan(3 * Math.PI / 16), 16) - (1 << 16);
    private static readonly int HalfCosine = Fixed(Math.Cos(Math.PI / 4), 15);
    private static readonly short[] RowWeights = GenerateWeights();

    private static int Fixed(double value, int bits) => (int)Math.Round(value * (1 << bits), MidpointRounding.AwayFromZero);
    private static int HighProduct(int value, int multiplier) => value * multiplier >> 16;

    private static short[] GenerateWeights()
    {
        var result = new short[512];
        for (int v = 0; v < 8; v++)
        {
            int angle = v == 0 || v == 4 ? 4 : Math.Min(v, 8 - v);
            double scale = Math.Cos(angle * Math.PI / 16);
            for (int u = 0; u < 8; u++)
                for (int x = 0; x < 8; x++)
                {
                    double cosine = u == 0 ? Math.Cos(Math.PI / 4) : Math.Cos((2 * x + 1) * u * Math.PI / 16);
                    result[v * 64 + u * 8 + x] = checked((short)Fixed(cosine * scale, 15));
                }
        }
        return result;
    }

    public static void Forward(ReadOnlySpan<byte> samples, Span<short> output)
    {
        if (samples.Length != 64 || output.Length != 64) throw new ArgumentException("Complete 8x8 block required.");
        if (Sse2.IsSupported && BitConverter.IsLittleEndian)
        {
            ForwardVectors(samples, output);
            return;
        }
        Span<int> columns = stackalloc int[64];
        for (int x = 0; x < 8; x++)
        {
            int a = (samples[x] + samples[56 + x]) << 3;
            int b = (samples[8 + x] + samples[48 + x]) << 3;
            int c = (samples[16 + x] + samples[40 + x]) << 3;
            int d = (samples[24 + x] + samples[32 + x]) << 3;
            int outside = a + d, inside = b + c;
            int outsideDifference = a - d, insideDifference = b - c;
            columns[x] = outside + inside;
            columns[32 + x] = outside - inside;
            columns[16 + x] = (outsideDifference + HighProduct(insideDifference, Tangent2)) | 1;
            columns[48 + x] = (HighProduct(outsideDifference, Tangent2) - insideDifference) | 1;

            int first = (samples[x] - samples[56 + x]) << 3;
            int middle1 = (samples[8 + x] - samples[48 + x]) << 4;
            int middle2 = (samples[16 + x] - samples[40 + x]) << 4;
            int last = (samples[24 + x] - samples[32 + x]) << 3;
            int sumRotation = HighProduct(middle1 + middle2, HalfCosine) | 1;
            int differenceRotation = HighProduct(middle1 - middle2, HalfCosine);
            int low = first + sumRotation, high = first - sumRotation;
            int lowRotation = last + differenceRotation, highRotation = last - differenceRotation;
            columns[8 + x] = (low + HighProduct(lowRotation, Tangent1)) | 1;
            columns[56 + x] = HighProduct(low, Tangent1) - lowRotation;
            columns[24 + x] = high - highRotation - HighProduct(highRotation, Tangent3);
            columns[40 + x] = highRotation + high + HighProduct(high, Tangent3);
        }
        for (int v = 0; v < 8; v++)
            for (int u = 0; u < 8; u++)
            {
                long sum = 1 << 16;
                for (int x = 0; x < 8; x++) sum += (long)columns[v * 8 + x] * RowWeights[v * 64 + u * 8 + x];
                output[v * 8 + u] = (short)Math.Clamp(sum >> 17, short.MinValue, short.MaxValue);
            }
    }

    private static Vector128<short> Samples8(ReadOnlySpan<byte> samples)
        => Sse2.UnpackLow(Vector128.CreateScalar(MemoryMarshal.Read<ulong>(samples)).AsByte(), Vector128<byte>.Zero).AsInt16();

    private static Vector128<short> HighProduct(Vector128<short> value, int multiplier)
        => Sse2.MultiplyHigh(value, Vector128.Create((short)multiplier));

    private static short RoundRow(Vector128<int> products)
    {
        // Each signed pair fits32bits. Widen before the row reduction so the
        // scalar expression's exact sum and rounding remain explicit.
        var sum = (long)products.GetElement(0) + products.GetElement(1)
            + products.GetElement(2) + products.GetElement(3) + (1 << 16);
        return (short)Math.Clamp(sum >> 17, short.MinValue, short.MaxValue);
    }

    private static void ForwardVectors(ReadOnlySpan<byte> samples, Span<short> output)
    {
        // Eight byte-valued columns run the same signed butterfly together.
        // All column intermediates fit signed16bits (the largest even sum is
        // 8*8*255=16320); signed high products retain negative truncation.
        var s0 = Samples8(samples); var s1 = Samples8(samples[8..]);
        var s2 = Samples8(samples[16..]); var s3 = Samples8(samples[24..]);
        var s4 = Samples8(samples[32..]); var s5 = Samples8(samples[40..]);
        var s6 = Samples8(samples[48..]); var s7 = Samples8(samples[56..]);
        var a = Sse2.ShiftLeftLogical(s0 + s7, 3); var b = Sse2.ShiftLeftLogical(s1 + s6, 3);
        var c = Sse2.ShiftLeftLogical(s2 + s5, 3); var d = Sse2.ShiftLeftLogical(s3 + s4, 3);
        var outside = a + d; var inside = b + c;
        var outsideDifference = a - d; var insideDifference = b - c;
        var one = Vector128.Create((short)1);
        Span<Vector128<short>> rows = stackalloc Vector128<short>[8];
        rows[0] = outside + inside; rows[4] = outside - inside;
        rows[2] = (outsideDifference + HighProduct(insideDifference, Tangent2)) | one;
        rows[6] = (HighProduct(outsideDifference, Tangent2) - insideDifference) | one;
        var first = Sse2.ShiftLeftLogical(s0 - s7, 3);
        var middle1 = Sse2.ShiftLeftLogical(s1 - s6, 4);
        var middle2 = Sse2.ShiftLeftLogical(s2 - s5, 4);
        var last = Sse2.ShiftLeftLogical(s3 - s4, 3);
        var sumRotation = HighProduct(middle1 + middle2, HalfCosine) | one;
        var differenceRotation = HighProduct(middle1 - middle2, HalfCosine);
        var low = first + sumRotation; var high = first - sumRotation;
        var lowRotation = last + differenceRotation; var highRotation = last - differenceRotation;
        rows[1] = (low + HighProduct(lowRotation, Tangent1)) | one;
        rows[7] = HighProduct(low, Tangent1) - lowRotation;
        rows[3] = high - highRotation - HighProduct(highRotation, Tangent3);
        rows[5] = highRotation + high + HighProduct(high, Tangent3);
        for (var v = 0; v < 8; v++)
        {
            var row = rows[v];
            if (Avx2.IsSupported)
            {
                var twice = Vector256.Create(row, row);
                var weights = RowWeights.AsSpan(v * 64, 64);
                var p01 = Avx2.MultiplyAddAdjacent(twice, MemoryMarshal.Read<Vector256<short>>(MemoryMarshal.AsBytes(weights[..16])));
                var p23 = Avx2.MultiplyAddAdjacent(twice, MemoryMarshal.Read<Vector256<short>>(MemoryMarshal.AsBytes(weights.Slice(16, 16))));
                var p45 = Avx2.MultiplyAddAdjacent(twice, MemoryMarshal.Read<Vector256<short>>(MemoryMarshal.AsBytes(weights.Slice(32, 16))));
                var p67 = Avx2.MultiplyAddAdjacent(twice, MemoryMarshal.Read<Vector256<short>>(MemoryMarshal.AsBytes(weights.Slice(48, 16))));
                // Byte-domain column bounds keep every absolute weighted row,
                // including the bias, within 2,139,160,576. All partial Int32
                // sums are exact; scalar widening/extraction is unnecessary.
                var sums = Avx2.HorizontalAdd(Avx2.HorizontalAdd(p01, p23), Avx2.HorizontalAdd(p45, p67));
                var rounded = Avx2.ShiftRightArithmetic(sums + Vector256.Create(1 << 16), 17);
                var packed = Sse2.PackSignedSaturate(rounded.GetLower(), rounded.GetUpper());
                // Horizontal reduction leaves even frequencies before odd ones.
                Sse2.UnpackLow(packed, Sse2.ShiftRightLogical128BitLane(packed.AsByte(), 8).AsInt16()).CopyTo(output[(v * 8)..]);
            }
            else
                for (var u = 0; u < 8; u++)
                {
                    var weights = MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(RowWeights.AsSpan(v * 64 + u * 8, 8)));
                    output[v * 8 + u] = RoundRow(Sse2.MultiplyAddAdjacent(row, weights));
                }
        }
    }

    internal static void PrepareQuantization(ReadOnlySpan<ushort> quantization, Span<ushort> plan)
    {
        if (quantization.Length != 64 || plan.Length != 128 || quantization[0] != 8)
            throw new ArgumentException("Complete blocks with the pinned DC precision required.");
        plan.Clear();
        for (int k = 1; k < 64; k++)
        {
            if (quantization[k] is 0 or > 255) throw new ArgumentException("Pinned 8-bit quantization required.");
            int reciprocal = 8192 / quantization[k];
            plan[k] = checked((ushort)reciprocal);
            plan[64 + k] = checked((ushort)((24576 + reciprocal / 2) / reciprocal));
        }
    }

    public static void Quantize(ReadOnlySpan<short> transformed, ReadOnlySpan<ushort> plan, Span<short> output)
    {
        if (transformed.Length != 64 || plan.Length != 128 || output.Length != 64)
            throw new ArgumentException("Complete blocks and prepared quantization required.");
        var dc = (short)(((transformed[0] + 32) >> 6) - 128);
        // Abs(-32768) and the largest rounding bias fit unsigned16bits. The
        // unsigned high product is the exact scalar >>16, with sign restored
        // afterward. The prepared zero DC lane is replaced independently.
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            for (var k = 0; k < 64; k += 16)
            {
                var values = MemoryMarshal.Read<Vector256<short>>(MemoryMarshal.AsBytes(transformed[k..]));
                var sign = Avx2.ShiftRightArithmetic(values, 15);
                var magnitude = ((values ^ sign) - sign).AsUInt16();
                var bias = MemoryMarshal.Read<Vector256<ushort>>(MemoryMarshal.AsBytes(plan[(64 + k)..]));
                var reciprocal = MemoryMarshal.Read<Vector256<ushort>>(MemoryMarshal.AsBytes(plan[k..]));
                var quantized = Avx2.MultiplyHigh(magnitude + bias, reciprocal).AsInt16();
                ((quantized ^ sign) - sign).CopyTo(output[k..]);
            }
            output[0] = dc; return;
        }
        if (Sse2.IsSupported && BitConverter.IsLittleEndian)
        {
            for (var k = 0; k < 64; k += 8)
            {
                var values = MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(transformed[k..]));
                var sign = Sse2.ShiftRightArithmetic(values, 15);
                var magnitude = ((values ^ sign) - sign).AsUInt16();
                var bias = MemoryMarshal.Read<Vector128<ushort>>(MemoryMarshal.AsBytes(plan[(64 + k)..]));
                var reciprocal = MemoryMarshal.Read<Vector128<ushort>>(MemoryMarshal.AsBytes(plan[k..]));
                var quantized = Sse2.MultiplyHigh(magnitude + bias, reciprocal).AsInt16();
                ((quantized ^ sign) - sign).CopyTo(output[k..]);
            }
            output[0] = dc; return;
        }
        output[0] = dc;
        for (int k = 1; k < 64; k++)
        {
            int value = transformed[k];
            int magnitude = ((Math.Abs(value) + plan[64 + k]) * plan[k]) >> 16;
            output[k] = (short)(value < 0 ? -magnitude : magnitude);
        }
    }

}
