using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static class JpegTransform
{
    // Natural positions for JPEG's diagonal scan, generated from the geometry
    // defined in T.81 rather than copied from an implementation's lookup table.
    public static readonly int[] Zigzag = BuildZigzag();
    // Original binary64 evaluation of T.81 A.3.3: C(u) cos((2x+1)u*pi/16),
    // C(0)=1/sqrt(2), C(u>0)=1. These round-trip values pin the first owned
    // implementation's numerical policy. Platform Math.Cos implementations can
    // differ by one ULP and change quantization or sample rounding at half ties.
    // This is a mathematically generated basis, not an upstream codec table.
    // Row-major storage avoids rectangular-array addressing in the inner loops.
    private static readonly double[] Cosines =
    [
        0.7071067811865475, 0.9807852804032304, 0.9238795325112867, 0.8314696123025452, 0.7071067811865476, 0.5555702330196023, 0.38268343236508984, 0.19509032201612833,
        0.7071067811865475, 0.8314696123025452, 0.38268343236508984, -0.1950903220161282, -0.7071067811865475, -0.9807852804032304, -0.9238795325112868, -0.5555702330196022,
        0.7071067811865475, 0.5555702330196023, -0.3826834323650897, -0.9807852804032304, -0.7071067811865477, 0.1950903220161283, 0.9238795325112865, 0.8314696123025456,
        0.7071067811865475, 0.19509032201612833, -0.9238795325112867, -0.5555702330196022, 0.7071067811865474, 0.8314696123025456, -0.3826834323650899, -0.9807852804032307,
        0.7071067811865475, -0.1950903220161282, -0.9238795325112868, 0.5555702330196018, 0.7071067811865477, -0.8314696123025451, -0.38268343236509056, 0.9807852804032305,
        0.7071067811865475, -0.555570233019602, -0.38268343236509034, 0.9807852804032304, -0.7071067811865467, -0.19509032201612803, 0.9238795325112867, -0.831469612302545,
        0.7071067811865475, -0.8314696123025453, 0.38268343236509, 0.19509032201612878, -0.7071067811865471, 0.9807852804032307, -0.9238795325112864, 0.5555702330196015,
        0.7071067811865475, -0.9807852804032304, 0.9238795325112865, -0.8314696123025451, 0.7071067811865466, -0.5555702330196015, 0.38268343236508956, -0.19509032201612858
    ];
    // The same owned, frozen basis in frequency-major order for independent
    // output-pixel SIMD lanes. This is only a transpose, with no new constants.
    private static readonly double[] InverseCosines = TransposeCosines();

    private static double[] TransposeCosines()
    {
        var result = new double[64];
        for (var u = 0; u < 8; u++)
        for (var x = 0; x < 8; x++) result[u * 8 + x] = Cosines[x * 8 + u];
        return result;
    }

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

    public static void Inverse(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels, OperationContext context)
    {
        if (coefficients.Length != 64 || quantization.Length != 64 || pixels.Length != 64)
            throw new ArgumentException("JPEG transforms operate on complete 8x8 blocks.");
        context.Work(4096);
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported
            && TryInverseSeparable(coefficients, quantization, pixels)) return;
        // T.81 A.3.3 output pixels are independent. Every lane keeps the
        // scalar v/u accumulation order and separate multiplications; no FMA,
        // reassociation or reduced logical work. Final round/clamp preserves
        // the original away-from-zero sample policy.
        // The only additional storage is the 512-byte static basis transpose.
        if (Avx512F.IsSupported)
        {
            // Share the exact horizontal product across output rows. Each
            // row still accumulates terms in the original v/u order; only
            // independent rows are interleaved, with no reassociation/FMA.
            Vector512<double> sum0 = Vector512<double>.Zero, sum1 = Vector512<double>.Zero,
                sum2 = Vector512<double>.Zero, sum3 = Vector512<double>.Zero,
                sum4 = Vector512<double>.Zero, sum5 = Vector512<double>.Zero,
                sum6 = Vector512<double>.Zero, sum7 = Vector512<double>.Zero;
            for (var v = 0; v < 8; v++)
            {
                var vertical0 = Vector512.Create(Cosines[v]); var vertical1 = Vector512.Create(Cosines[8 + v]);
                var vertical2 = Vector512.Create(Cosines[16 + v]); var vertical3 = Vector512.Create(Cosines[24 + v]);
                var vertical4 = Vector512.Create(Cosines[32 + v]); var vertical5 = Vector512.Create(Cosines[40 + v]);
                var vertical6 = Vector512.Create(Cosines[48 + v]); var vertical7 = Vector512.Create(Cosines[56 + v]);
                for (var u = 0; u < 8; u++)
                {
                    var index = v * 8 + u;
                    var sample = Vector512.Create((double)coefficients[index] * quantization[index]);
                    var horizontal = Avx512F.Multiply(sample, Vector512.Create(InverseCosines.AsSpan(u * 8, 8)));
                    sum0 = Avx512F.Add(sum0, Avx512F.Multiply(horizontal, vertical0));
                    sum1 = Avx512F.Add(sum1, Avx512F.Multiply(horizontal, vertical1));
                    sum2 = Avx512F.Add(sum2, Avx512F.Multiply(horizontal, vertical2));
                    sum3 = Avx512F.Add(sum3, Avx512F.Multiply(horizontal, vertical3));
                    sum4 = Avx512F.Add(sum4, Avx512F.Multiply(horizontal, vertical4));
                    sum5 = Avx512F.Add(sum5, Avx512F.Multiply(horizontal, vertical5));
                    sum6 = Avx512F.Add(sum6, Avx512F.Multiply(horizontal, vertical6));
                    sum7 = Avx512F.Add(sum7, Avx512F.Multiply(horizontal, vertical7));
                }
            }
            if (Avx512F.VL.IsSupported)
            {
                // Multiplication by the exact power-of-two 0.25 gives the
                // same binary64 result as division by four, including underflow.
                var scale = Vector512.Create(0.25); var offset = Vector512.Create(128.0);
                RoundSamples(Avx512F.Add(Avx512F.Multiply(sum0, scale), offset), pixels[..8]);
                RoundSamples(Avx512F.Add(Avx512F.Multiply(sum1, scale), offset), pixels.Slice(8, 8));
                RoundSamples(Avx512F.Add(Avx512F.Multiply(sum2, scale), offset), pixels.Slice(16, 8));
                RoundSamples(Avx512F.Add(Avx512F.Multiply(sum3, scale), offset), pixels.Slice(24, 8));
                RoundSamples(Avx512F.Add(Avx512F.Multiply(sum4, scale), offset), pixels.Slice(32, 8));
                RoundSamples(Avx512F.Add(Avx512F.Multiply(sum5, scale), offset), pixels.Slice(40, 8));
                RoundSamples(Avx512F.Add(Avx512F.Multiply(sum6, scale), offset), pixels.Slice(48, 8));
                RoundSamples(Avx512F.Add(Avx512F.Multiply(sum7, scale), offset), pixels.Slice(56, 8));
                return;
            }
            for (var x = 0; x < 8; x++)
            {
                pixels[x] = (byte)Math.Clamp(Math.Round(sum0.GetElement(x) / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
                pixels[8 + x] = (byte)Math.Clamp(Math.Round(sum1.GetElement(x) / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
                pixels[16 + x] = (byte)Math.Clamp(Math.Round(sum2.GetElement(x) / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
                pixels[24 + x] = (byte)Math.Clamp(Math.Round(sum3.GetElement(x) / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
                pixels[32 + x] = (byte)Math.Clamp(Math.Round(sum4.GetElement(x) / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
                pixels[40 + x] = (byte)Math.Clamp(Math.Round(sum5.GetElement(x) / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
                pixels[48 + x] = (byte)Math.Clamp(Math.Round(sum6.GetElement(x) / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
                pixels[56 + x] = (byte)Math.Clamp(Math.Round(sum7.GetElement(x) / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
            }
            return;
        }
        if (Avx.IsSupported)
        {
            for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x += 4)
            {
                var sum = Vector256<double>.Zero;
                for (var v = 0; v < 8; v++)
                {
                    var vertical = Vector256.Create(Cosines[y * 8 + v]);
                    for (var u = 0; u < 8; u++)
                    {
                        var index = v * 8 + u;
                        var sample = Vector256.Create((double)coefficients[index] * quantization[index]);
                        var horizontal = Vector256.Create(InverseCosines.AsSpan(u * 8 + x, 4));
                        sum = Avx.Add(sum, Avx.Multiply(Avx.Multiply(sample, horizontal), vertical));
                    }
                }
                for (var lane = 0; lane < 4; lane++)
                    pixels[y * 8 + x + lane] = (byte)Math.Clamp(Math.Round(sum.GetElement(lane) / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
            }
            return;
        }
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            double sum = 0;
            for (var v = 0; v < 8; v++)
            {
                var vertical = Cosines[y * 8 + v];
                for (var u = 0; u < 8; u++)
                    sum += (double)coefficients[v * 8 + u] * quantization[v * 8 + u] * Cosines[x * 8 + u] * vertical;
            }
            pixels[y * 8 + x] = (byte)Math.Clamp(Math.Round(sum / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
        }
    }

    private static bool TryInverseSeparable(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels)
    {
        // Give the JIT fixed-size views before either bounded pass. The public
        // primitive already validated these lengths; no checks or storage are
        // removed. Hoist basis access out of the arithmetic loops as well.
        coefficients = coefficients[..64]; quantization = quantization[..64]; pixels = pixels[..64];
        var horizontalBasis = InverseCosines.AsSpan(0, 64);
        var verticalBasis = Cosines.AsSpan(0, 64);
        Vector512<double> row0 = Vector512<double>.Zero, row1 = Vector512<double>.Zero,
            row2 = Vector512<double>.Zero, row3 = Vector512<double>.Zero,
            row4 = Vector512<double>.Zero, row5 = Vector512<double>.Zero,
            row6 = Vector512<double>.Zero, row7 = Vector512<double>.Zero;
        // Reuse each exact dequantized sample for the certificate. Independent
        // sums avoid a separate 64-term serial reduction. All partial sums and
        // the total are integers below 2^53, so regrouping changes no bit of E.
        // See docs/jpeg-inverse-rounding.md for the bound against direct output.
        double magnitude0 = 0, magnitude1 = 0, magnitude2 = 0, magnitude3 = 0;
        for (var u = 0; u < 8; u++)
        {
            var horizontal = Vector512.Create(horizontalBasis.Slice(u * 8, 8));
            var sample0 = (double)coefficients[u] * quantization[u]; magnitude0 += Math.Abs(sample0);
            row0 = Avx512F.Add(row0, Avx512F.Multiply(Vector512.Create(sample0), horizontal));
            var sample1 = (double)coefficients[8 + u] * quantization[8 + u]; magnitude1 += Math.Abs(sample1);
            row1 = Avx512F.Add(row1, Avx512F.Multiply(Vector512.Create(sample1), horizontal));
            var sample2 = (double)coefficients[16 + u] * quantization[16 + u]; magnitude2 += Math.Abs(sample2);
            row2 = Avx512F.Add(row2, Avx512F.Multiply(Vector512.Create(sample2), horizontal));
            var sample3 = (double)coefficients[24 + u] * quantization[24 + u]; magnitude3 += Math.Abs(sample3);
            row3 = Avx512F.Add(row3, Avx512F.Multiply(Vector512.Create(sample3), horizontal));
            var sample4 = (double)coefficients[32 + u] * quantization[32 + u]; magnitude0 += Math.Abs(sample4);
            row4 = Avx512F.Add(row4, Avx512F.Multiply(Vector512.Create(sample4), horizontal));
            var sample5 = (double)coefficients[40 + u] * quantization[40 + u]; magnitude1 += Math.Abs(sample5);
            row5 = Avx512F.Add(row5, Avx512F.Multiply(Vector512.Create(sample5), horizontal));
            var sample6 = (double)coefficients[48 + u] * quantization[48 + u]; magnitude2 += Math.Abs(sample6);
            row6 = Avx512F.Add(row6, Avx512F.Multiply(Vector512.Create(sample6), horizontal));
            var sample7 = (double)coefficients[56 + u] * quantization[56 + u]; magnitude3 += Math.Abs(sample7);
            row7 = Avx512F.Add(row7, Avx512F.Multiply(Vector512.Create(sample7), horizontal));
        }
        var magnitude = (magnitude0 + magnitude1) + (magnitude2 + magnitude3);
        var error = (magnitude + 128) * 5.684341886080802e-14; // 2^-44, exact power-of-two scaling
        if (error >= 0.5) return false; // no destination byte was written
        for (var y = 0; y < 8; y++)
        {
            var basis = verticalBasis.Slice(y * 8, 8);
            var sum = Avx512F.Multiply(row0, Vector512.Create(basis[0]));
            sum = Avx512F.Add(sum, Avx512F.Multiply(row1, Vector512.Create(basis[1])));
            sum = Avx512F.Add(sum, Avx512F.Multiply(row2, Vector512.Create(basis[2])));
            sum = Avx512F.Add(sum, Avx512F.Multiply(row3, Vector512.Create(basis[3])));
            sum = Avx512F.Add(sum, Avx512F.Multiply(row4, Vector512.Create(basis[4])));
            sum = Avx512F.Add(sum, Avx512F.Multiply(row5, Vector512.Create(basis[5])));
            sum = Avx512F.Add(sum, Avx512F.Multiply(row6, Vector512.Create(basis[6])));
            sum = Avx512F.Add(sum, Avx512F.Multiply(row7, Vector512.Create(basis[7])));
            var samples = Avx512F.Add(Avx512F.Multiply(sum, Vector512.Create(0.25)), Vector512.Create(128.0));
            var clipped = Avx512F.Min(Avx512F.Max(samples, Vector512<double>.Zero), Vector512.Create(255.0));
            var integral = Avx512F.RoundScale(clipped, 0x09);
            var fraction = Avx512F.Subtract(clipped, integral);
            var distance = Avx512F.And(Avx512F.Subtract(fraction, Vector512.Create(0.5)).AsInt64(),
                Vector512.Create(long.MaxValue)).AsDouble();
            // Any uncertain lane causes this row to use the original direct
            // v/u evaluation. No rounded separable byte escapes the certificate.
            if (Vector512.ExtractMostSignificantBits(Avx512F.CompareLessThanOrEqual(distance, Vector512.Create(error))) != 0)
            {
                var direct = Vector512<double>.Zero;
                for (var v = 0; v < 8; v++)
                {
                    var vertical = Vector512.Create(basis[v]);
                    for (var u = 0; u < 8; u++)
                    {
                        var index = v * 8 + u;
                        var sample = Vector512.Create((double)coefficients[index] * quantization[index]);
                        var horizontal = Avx512F.Multiply(sample, Vector512.Create(horizontalBasis.Slice(u * 8, 8)));
                        direct = Avx512F.Add(direct, Avx512F.Multiply(horizontal, vertical));
                    }
                }
                RoundSamples(Avx512F.Add(Avx512F.Multiply(direct, Vector512.Create(0.25)), Vector512.Create(128.0)), pixels.Slice(y * 8, 8));
                continue;
            }
            var aboveHalf = Avx512F.CompareGreaterThanOrEqual(fraction, Vector512.Create(0.5));
            var increment = Avx512F.And(aboveHalf.AsInt64(), Vector512.Create(1.0).AsInt64()).AsDouble();
            var rounded = Avx512F.Add(integral, increment);
            Avx512F.VL.ConvertToVector128Byte(Avx512F.ConvertToVector256Int32WithTruncation(rounded))
                .GetLower().CopyTo(pixels.Slice(y * 8, 8));
        }
        return true;
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

    internal static bool TryForwardQuantized(ReadOnlySpan<byte> pixels, ReadOnlySpan<ushort> quantization, Span<int> result, OperationContext context)
    {
        if (pixels.Length != 64 || quantization.Length != 64 || result.Length != 64)
            throw new ArgumentException("JPEG quantized transforms operate on complete 8x8 blocks.");
        if (!Avx512F.IsSupported || !Avx2.IsSupported || quantization.Contains((ushort)0)) return false;
        context.Work(4096);
        // Stored basis values are exact inputs to the certificate. Row/column
        // regrouping may change coefficient bits, so only a certified quantized
        // integer is accepted. See docs/jpeg-forward-quantization.md.
        Span<double> horizontal = stackalloc double[64];
        var basis = Cosines.AsSpan(0, 64);
        pixels = pixels[..64]; quantization = quantization[..64]; result = result[..64];
        // Interleave independent rows, retaining each row's original x order.
        // A single accumulator at a time serializes the vector additions even
        // though the separable evaluation needs fewer arithmetic operations.
        Vector512<double> row0 = Vector512<double>.Zero, row1 = Vector512<double>.Zero,
            row2 = Vector512<double>.Zero, row3 = Vector512<double>.Zero,
            row4 = Vector512<double>.Zero, row5 = Vector512<double>.Zero,
            row6 = Vector512<double>.Zero, row7 = Vector512<double>.Zero;
        for (var x = 0; x < 8; x++)
        {
            var weights = Vector512.Create(basis.Slice(x * 8, 8));
            row0 = Avx512F.Add(row0, Avx512F.Multiply(Vector512.Create((double)(pixels[x] - 128)), weights));
            row1 = Avx512F.Add(row1, Avx512F.Multiply(Vector512.Create((double)(pixels[8 + x] - 128)), weights));
            row2 = Avx512F.Add(row2, Avx512F.Multiply(Vector512.Create((double)(pixels[16 + x] - 128)), weights));
            row3 = Avx512F.Add(row3, Avx512F.Multiply(Vector512.Create((double)(pixels[24 + x] - 128)), weights));
            row4 = Avx512F.Add(row4, Avx512F.Multiply(Vector512.Create((double)(pixels[32 + x] - 128)), weights));
            row5 = Avx512F.Add(row5, Avx512F.Multiply(Vector512.Create((double)(pixels[40 + x] - 128)), weights));
            row6 = Avx512F.Add(row6, Avx512F.Multiply(Vector512.Create((double)(pixels[48 + x] - 128)), weights));
            row7 = Avx512F.Add(row7, Avx512F.Multiply(Vector512.Create((double)(pixels[56 + x] - 128)), weights));
        }
        row0.CopyTo(horizontal[..8]); row1.CopyTo(horizontal.Slice(8, 8));
        row2.CopyTo(horizontal.Slice(16, 8)); row3.CopyTo(horizontal.Slice(24, 8));
        row4.CopyTo(horizontal.Slice(32, 8)); row5.CopyTo(horizontal.Slice(40, 8));
        row6.CopyTo(horizontal.Slice(48, 8)); row7.CopyTo(horizontal.Slice(56, 8));
        // The vertical frequencies are independent as well. Reuse this same
        // scratch after every horizontal row has been consumed; no additional
        // storage and no change to a frequency's y accumulation order.
        Vector512<double> sum0 = Vector512<double>.Zero, sum1 = Vector512<double>.Zero,
            sum2 = Vector512<double>.Zero, sum3 = Vector512<double>.Zero,
            sum4 = Vector512<double>.Zero, sum5 = Vector512<double>.Zero,
            sum6 = Vector512<double>.Zero, sum7 = Vector512<double>.Zero;
        for (var y = 0; y < 8; y++)
        {
            var samples = Vector512.Create(horizontal.Slice(y * 8, 8)); var offset = y * 8;
            sum0 = Avx512F.Add(sum0, Avx512F.Multiply(samples, Vector512.Create(basis[offset])));
            sum1 = Avx512F.Add(sum1, Avx512F.Multiply(samples, Vector512.Create(basis[offset + 1])));
            sum2 = Avx512F.Add(sum2, Avx512F.Multiply(samples, Vector512.Create(basis[offset + 2])));
            sum3 = Avx512F.Add(sum3, Avx512F.Multiply(samples, Vector512.Create(basis[offset + 3])));
            sum4 = Avx512F.Add(sum4, Avx512F.Multiply(samples, Vector512.Create(basis[offset + 4])));
            sum5 = Avx512F.Add(sum5, Avx512F.Multiply(samples, Vector512.Create(basis[offset + 5])));
            sum6 = Avx512F.Add(sum6, Avx512F.Multiply(samples, Vector512.Create(basis[offset + 6])));
            sum7 = Avx512F.Add(sum7, Avx512F.Multiply(samples, Vector512.Create(basis[offset + 7])));
        }
        var scale = Vector512.Create(0.25);
        Avx512F.Multiply(sum0, scale).CopyTo(horizontal[..8]);
        Avx512F.Multiply(sum1, scale).CopyTo(horizontal.Slice(8, 8));
        Avx512F.Multiply(sum2, scale).CopyTo(horizontal.Slice(16, 8));
        Avx512F.Multiply(sum3, scale).CopyTo(horizontal.Slice(24, 8));
        Avx512F.Multiply(sum4, scale).CopyTo(horizontal.Slice(32, 8));
        Avx512F.Multiply(sum5, scale).CopyTo(horizontal.Slice(40, 8));
        Avx512F.Multiply(sum6, scale).CopyTo(horizontal.Slice(48, 8));
        Avx512F.Multiply(sum7, scale).CopyTo(horizontal.Slice(56, 8));
        var magnitudeMask = Vector512.Create(long.MaxValue); var signMask = Vector512.Create(long.MinValue);
        var half = Vector512.Create(0.5); var one = Vector512.Create(1.0).AsInt64();
        // 2^-30 exceeds the direct/separable quotient difference and the
        // rounding error of the distance test throughout the byte-input domain.
        var error = Vector512.Create(1.0 / 1073741824.0);
        for (var v = 0; v < 8; v++)
        {
            var coefficients = Vector512.Create(horizontal.Slice(v * 8, 8));
            var integers = Avx2.ConvertToVector256Int32(Vector128.Create(quantization.Slice(v * 8, 8)));
            var quotient = Avx512F.Divide(coefficients, Avx512F.ConvertToVector512Double(integers));
            var magnitude = Avx512F.And(quotient.AsInt64(), magnitudeMask).AsDouble();
            var integral = Avx512F.RoundScale(magnitude, 0x09);
            var fraction = Avx512F.Subtract(magnitude, integral);
            var distance = Avx512F.And(Avx512F.Subtract(fraction, half).AsInt64(), magnitudeMask).AsDouble();
            var uncertain = Vector512.ExtractMostSignificantBits(Avx512F.CompareLessThanOrEqual(distance, error));
            var atLeastHalf = Avx512F.CompareGreaterThanOrEqual(fraction, half);
            var rounded = Avx512F.Add(integral, Avx512F.And(atLeastHalf.AsInt64(), one).AsDouble());
            var signed = Avx512F.Or(rounded.AsInt64(), Avx512F.And(quotient.AsInt64(), signMask)).AsDouble();
            var quantized = Avx512F.ConvertToVector256Int32WithTruncation(signed);
            for (var u = 0; uncertain != 0 && u < 8; u++)
            {
                if ((uncertain & (1UL << u)) == 0) continue;
                var index = v * 8 + u;
                quantized = quantized.WithElement(u, (int)Math.Round(ForwardCoefficient(pixels, v, u) / quantization[index], MidpointRounding.AwayFromZero));
            }
            quantized.CopyTo(result.Slice(v * 8, 8));
        }
        return true;
    }

    private static double ForwardCoefficient(ReadOnlySpan<byte> pixels, int v, int u)
    {
        double sum = 0;
        for (var y = 0; y < 8; y++)
        {
            var vertical = Cosines[y * 8 + v];
            for (var x = 0; x < 8; x++) sum += (pixels[y * 8 + x] - 128) * Cosines[x * 8 + u] * vertical;
        }
        return sum / 4;
    }

    public static void Forward(ReadOnlySpan<byte> pixels, Span<double> coefficients, OperationContext context)
    {
        if (pixels.Length != 64 || coefficients.Length != 64)
            throw new ArgumentException("JPEG transforms operate on complete 8x8 blocks.");
        context.Work(4096);
        // T.81 A.3.3 coefficients are independent. SIMD lanes compute different
        // horizontal frequencies; each lane keeps the scalar y/x accumulation
        // order and its two separate multiplications (no FMA or reassociation).
        // Safe span construction/copy uses the existing frozen basis and output;
        // no new table, per-block allocation or image cache is needed.
        if (Avx512F.IsSupported)
        {
            // Keep basis initialization outside the accumulator lifetime.
            var basis = Cosines.AsSpan(0, 64);
            // The first horizontal product is identical for all eight vertical
            // frequencies. Share it without changing any lane's y/x additions
            // or combining its two rounded multiplications.
            var sum0 = Vector512<double>.Zero; var sum1 = Vector512<double>.Zero;
            var sum2 = Vector512<double>.Zero; var sum3 = Vector512<double>.Zero;
            var sum4 = Vector512<double>.Zero; var sum5 = Vector512<double>.Zero;
            var sum6 = Vector512<double>.Zero; var sum7 = Vector512<double>.Zero;
            for (var y = 0; y < 8; y++)
            {
                var vertical0 = Vector512.Create(basis[y * 8]);
                var vertical1 = Vector512.Create(basis[y * 8 + 1]);
                var vertical2 = Vector512.Create(basis[y * 8 + 2]);
                var vertical3 = Vector512.Create(basis[y * 8 + 3]);
                var vertical4 = Vector512.Create(basis[y * 8 + 4]);
                var vertical5 = Vector512.Create(basis[y * 8 + 5]);
                var vertical6 = Vector512.Create(basis[y * 8 + 6]);
                var vertical7 = Vector512.Create(basis[y * 8 + 7]);
                for (var x = 0; x < 8; x++)
                {
                    var sample = Vector512.Create((double)(pixels[y * 8 + x] - 128));
                    var horizontal = Avx512F.Multiply(sample, Vector512.Create(basis.Slice(x * 8, 8)));
                    sum0 = Avx512F.Add(sum0, Avx512F.Multiply(horizontal, vertical0));
                    sum1 = Avx512F.Add(sum1, Avx512F.Multiply(horizontal, vertical1));
                    sum2 = Avx512F.Add(sum2, Avx512F.Multiply(horizontal, vertical2));
                    sum3 = Avx512F.Add(sum3, Avx512F.Multiply(horizontal, vertical3));
                    sum4 = Avx512F.Add(sum4, Avx512F.Multiply(horizontal, vertical4));
                    sum5 = Avx512F.Add(sum5, Avx512F.Multiply(horizontal, vertical5));
                    sum6 = Avx512F.Add(sum6, Avx512F.Multiply(horizontal, vertical6));
                    sum7 = Avx512F.Add(sum7, Avx512F.Multiply(horizontal, vertical7));
                }
            }
            var divisor = Vector512.Create(4d);
            Avx512F.Divide(sum0, divisor).CopyTo(coefficients);
            Avx512F.Divide(sum1, divisor).CopyTo(coefficients[8..]);
            Avx512F.Divide(sum2, divisor).CopyTo(coefficients[16..]);
            Avx512F.Divide(sum3, divisor).CopyTo(coefficients[24..]);
            Avx512F.Divide(sum4, divisor).CopyTo(coefficients[32..]);
            Avx512F.Divide(sum5, divisor).CopyTo(coefficients[40..]);
            Avx512F.Divide(sum6, divisor).CopyTo(coefficients[48..]);
            Avx512F.Divide(sum7, divisor).CopyTo(coefficients[56..]);
            return;
        }
        if (Avx.IsSupported)
        {
            for (var v = 0; v < 8; v++)
            for (var u = 0; u < 8; u += 4)
            {
                var sum = Vector256<double>.Zero;
                for (var y = 0; y < 8; y++)
                {
                    var vertical = Vector256.Create(Cosines[y * 8 + v]);
                    for (var x = 0; x < 8; x++)
                    {
                        var sample = Vector256.Create((double)(pixels[y * 8 + x] - 128));
                        var horizontal = Vector256.Create(Cosines.AsSpan(x * 8 + u, 4));
                        var term = Avx.Multiply(Avx.Multiply(sample, horizontal), vertical);
                        sum = Avx.Add(sum, term);
                    }
                }
                Avx.Divide(sum, Vector256.Create(4d)).CopyTo(coefficients[(v * 8 + u)..]);
            }
            return;
        }
        for (var v = 0; v < 8; v++)
        for (var u = 0; u < 8; u++)
        {
            double sum = 0;
            for (var y = 0; y < 8; y++)
            {
                var vertical = Cosines[y * 8 + v];
                for (var x = 0; x < 8; x++) sum += (pixels[y * 8 + x] - 128) * Cosines[x * 8 + u] * vertical;
            }
            coefficients[v * 8 + u] = sum / 4;
        }
    }
}
