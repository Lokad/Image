using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Independent fixed-point coefficient quantization. RFC6386 section13 supplies
// diagonal scan geometry; the encoder's level cap/rounding/sharpening are pinned
// separately. Caller buffers and the matrix view are borrowed, with no heap,
// cache or per-block admission. The outer encoder admits cancellation.
internal static class Vp8BlockQuantization
{
    private static readonly Vector256<byte> ZigzagLow = ZigzagMask(false), ZigzagHigh = ZigzagMask(true);

    private static int ScanRank(int position)
    {
        var x = position & 3; var y = position >> 2; var diagonal = x + y;
        var before = diagonal < 4 ? diagonal * (diagonal + 1) / 2
            : 16 - (7 - diagonal) * (8 - diagonal) / 2;
        return before + ((diagonal & 1) != 0 ? Math.Min(3, diagonal) - x : x - Math.Max(0, diagonal - 3));
    }

    private static Vector256<byte> ZigzagMask(bool upper)
    {
        // Generate byte selectors from the same RFC6386 diagonal geometry.
        Span<byte> bytes = stackalloc byte[32]; bytes.Fill(0x80);
        for (var p = 0; p < 16; p++)
        {
            if ((p >= 8) != upper) continue;
            var rank = ScanRank(p);
            bytes[rank * 2] = (byte)((p & 7) * 2); bytes[rank * 2 + 1] = (byte)((p & 7) * 2 + 1);
        }
        return Vector256.Create(bytes);
    }

    // Sixteen raster-order coefficients become dequantized raster samples;
    // sixteen signed levels are written in zigzag order. Trailing samples stay
    // untouched. The spans and borrowed matrix storage must not overlap. The transform domain includes the
    // worst16DC Walsh sum,16320, and keeps intermediate/dequantized arithmetic
    // within the native signed coefficient representation.
    internal static bool Apply(Span<short> coefficients, Span<short> levels, Vp8QuantizationMatrix matrix)
    {
        if (coefficients.Length < 16 || levels.Length < 16 || matrix.Values.Length < 80)
            throw new ArgumentException("Quantization requires16coefficients,16levels and a complete matrix.");
        if (coefficients.Overlaps(levels)) throw new ArgumentException("Coefficient and level buffers must not overlap.");
        var matrixBytes = MemoryMarshal.AsBytes(matrix.Values);
        if (matrixBytes.Overlaps(MemoryMarshal.AsBytes(coefficients)) || matrixBytes.Overlaps(MemoryMarshal.AsBytes(levels)))
            throw new ArgumentException("Output buffers must not overlap borrowed matrix storage.");
        if (Avx2.IsSupported)
        {
            // Signed comparisons also reject Int16.MinValue without an absolute-value overflow.
            var input = Vector256.Create(coefficients);
            var invalid = Avx2.Or(Avx2.CompareGreaterThan(input, Vector256.Create((short)16320)),
                Avx2.CompareGreaterThan(Vector256.Create((short)-16320), input));
            if (Avx2.MoveMask(invalid.AsByte()) != 0)
                throw new ArgumentOutOfRangeException(nameof(coefficients), "Coefficient exceeds the VP8 transform domain.");
            return ApplyVectors(coefficients, levels, matrix);
        }
        for (var p = 0; p < 16; p++)
            if (coefficients[p] is < -16320 or > 16320) throw new ArgumentOutOfRangeException(nameof(coefficients), "Coefficient exceeds the VP8 transform domain.");
        var nonzero = false;
        for (var p = 0; p < 16; p++)
        {
            var value = coefficients[p]; var magnitude = Math.Abs((int)value) + matrix.Sharpening[p];
            var level = magnitude <= matrix.ZeroThresholds[p] ? 0
                : Math.Min(2047, (magnitude * matrix.Reciprocals[p] + matrix.Biases[p]) >> 17);
            if (value < 0) level = -level;
            coefficients[p] = checked((short)(level * matrix.Steps[p]));
            levels[ScanRank(p)] = (short)level; nonzero |= level != 0;
        }
        return nonzero;
    }

    private static Vector256<int> QuantizedLevels(ReadOnlySpan<short> coefficients, Vp8QuantizationMatrix matrix, int offset)
    {
        var input = Avx2.ConvertToVector256Int32(Vector128.Create(coefficients));
        var magnitude = Avx2.Add(Avx2.Abs(input).AsInt32(), Vector256.Create(matrix.Sharpening[offset..]));
        var scaled = Avx2.Add(Avx2.MultiplyLow(magnitude, Vector256.Create(matrix.Reciprocals[offset..])),
            Vector256.Create(matrix.Biases[offset..]));
        var level = Avx2.Min(Vector256.Create(2047), Avx2.ShiftRightArithmetic(scaled, 17));
        // A zero coefficient need not quantize to zero under every matrix. Only
        // negative inputs negate the level; PSIGN's zero-input rule differs.
        var negative = Avx2.CompareGreaterThan(Vector256<int>.Zero, input);
        var signed = Avx2.Subtract(Avx2.Xor(level, negative), negative);
        return Avx2.And(signed, Avx2.CompareGreaterThan(magnitude, Vector256.Create(matrix.ZeroThresholds[offset..])));
    }

    private static bool ApplyVectors(Span<short> coefficients, Span<short> levels, Vp8QuantizationMatrix matrix)
    {
        var first = QuantizedLevels(coefficients, matrix, 0);
        var second = QuantizedLevels(coefficients[8..], matrix, 8);
        var steps0 = Vector256.Create(matrix.Steps); var steps1 = Vector256.Create(matrix.Steps[8..]);
        // The17-bit shift and sign mapping bound every level to[-16384,16384],
        // even for malformed matrices. Steps in[0,32767] therefore multiply
        // exactly in Int32. Check signed16 representation before ANY store.
        var stepBits = Avx2.And(Avx2.Or(steps0, steps1), Vector256.Create(~32767));
        if (Avx2.MoveMask(Avx2.CompareEqual(stepBits, Vector256<int>.Zero).AsByte()) == -1)
        {
            var product0 = Avx2.MultiplyLow(first, steps0); var product1 = Avx2.MultiplyLow(second, steps1);
            var lower = Vector256.Create(-32768); var upper = Vector256.Create(32767);
            var invalid = Avx2.Or(Avx2.Or(Avx2.CompareGreaterThan(lower, product0), Avx2.CompareGreaterThan(product0, upper)),
                Avx2.Or(Avx2.CompareGreaterThan(lower, product1), Avx2.CompareGreaterThan(product1, upper)));
            if (Avx2.MoveMask(invalid.AsByte()) == 0)
            {
                var raster = Avx2.Permute4x64(Avx2.PackSignedSaturate(product0, product1).AsInt64(), 0xd8).AsInt16();
                var quantizedRaster = Avx2.Permute4x64(Avx2.PackSignedSaturate(first, second).AsInt64(), 0xd8).AsByte();
                var low = Vector256.Create(quantizedRaster.GetLower(), quantizedRaster.GetLower());
                var high = Vector256.Create(quantizedRaster.GetUpper(), quantizedRaster.GetUpper());
                var zigzag = Avx2.Or(Avx2.Shuffle(low, ZigzagLow), Avx2.Shuffle(high, ZigzagHigh)).AsInt16();
                raster.CopyTo(coefficients); zigzag.CopyTo(levels);
                return Avx2.MoveMask(Avx2.CompareEqual(Avx2.Or(first, second), Vector256<int>.Zero).AsByte()) != -1;
            }
        }
        // Keep checked multiplication/narrowing and valid-prefix publication
        // when a matrix or result cannot use the packed path. No stores above.
        // Fixed64-byte scratch fits the outer admitted macroblock workspace.
        Span<int> quantized = stackalloc int[16];
        first.CopyTo(quantized); second.CopyTo(quantized[8..]);
        var nonzero = false;
        for (var p = 0; p < 16; p++)
        {
            var level = quantized[p];
            // Preserve checked Int32 multiplication, signed16 narrowing and
            // publication order, including the valid prefix before a failure.
            coefficients[p] = checked((short)(level * matrix.Steps[p]));
            levels[ScanRank(p)] = (short)level; nonzero |= level != 0;
        }
        return nonzero;
    }
}
