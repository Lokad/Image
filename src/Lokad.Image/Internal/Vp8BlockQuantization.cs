using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Independent fixed-point coefficient quantization. RFC6386 section13 supplies
// diagonal scan geometry; the encoder's level cap/rounding/sharpening are pinned
// separately. Caller buffers and the matrix view are borrowed, with no heap,
// cache or per-block admission. The outer encoder admits work/cancellation.
internal static class Vp8BlockQuantization
{
    private static int ScanRank(int position)
    {
        var x = position & 3; var y = position >> 2; var diagonal = x + y;
        var before = diagonal < 4 ? diagonal * (diagonal + 1) / 2
            : 16 - (7 - diagonal) * (8 - diagonal) / 2;
        return before + ((diagonal & 1) != 0 ? Math.Min(3, diagonal) - x : x - Math.Max(0, diagonal - 3));
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
        for (var p = 0; p < 16; p++)
            if (coefficients[p] is < -16320 or > 16320) throw new ArgumentOutOfRangeException(nameof(coefficients), "Coefficient exceeds the VP8 transform domain.");
        if (Avx2.IsSupported) return ApplyVectors(coefficients, levels, matrix);
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
        // Fixed64-byte scratch fits the outer admitted macroblock workspace.
        Span<int> quantized = stackalloc int[16];
        QuantizedLevels(coefficients, matrix, 0).CopyTo(quantized);
        QuantizedLevels(coefficients[8..], matrix, 8).CopyTo(quantized[8..]);
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
