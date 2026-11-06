using System.Numerics;
using System.Runtime.CompilerServices;
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
    private static readonly Vector512<int> ZigzagOrder = FullBlockOrder();

    private static Vector512<int> FullBlockOrder()
    {
        Span<int> order = stackalloc int[16];
        for (var p = 0; p < 16; p++) order[ScanRank(p)] = p;
        return Vector512.Create(order);
    }

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
        // Private callers provide disjoint block buffers and a live generated
        // plan. Byte residuals, Walsh scaling and bounded DC diffusion establish
        // the coefficient domain before this per-block operation.
        if (Avx512F.IsSupported) return ApplyFullBlock(coefficients, levels, matrix);
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

    private static bool ApplyFullBlock(Span<short> coefficients, Span<short> levels, Vp8QuantizationMatrix matrix)
    {
        var prepared = new FullBlockQuantizer(matrix).Prepare(coefficients);
        prepared.PublishRaster(coefficients); prepared.PublishLevels(levels);
        return prepared.Mask != 0;
    }

    // Independent contiguous blocks share one matrix and no prediction feedback.
    // The caller keeps per-mode cancellation/decisions outside this sequence.
    // Prepare fixed operands and dispatch once; preserve the single-block math
    // and each block's independent raster/diagonal publication.
    internal static uint ApplyBlocks(Span<short> coefficients, Span<short> levels,
        Vp8QuantizationMatrix matrix, int count)
    {
        uint nonzero = 0;
        if (Avx512F.IsSupported)
        {
            var quantizer = new FullBlockQuantizer(matrix);
            for (var block = 0; block < count; block++)
            {
                var prepared = quantizer.Prepare(coefficients.Slice(block * 16, 16));
                prepared.PublishRaster(coefficients.Slice(block * 16, 16));
                prepared.PublishLevels(levels.Slice(block * 16, 16));
                if (prepared.Mask != 0) nonzero |= 1u << block;
            }
        }
        else
        {
            for (var block = 0; block < count; block++)
                if (Apply(coefficients.Slice(block * 16, 16), levels.Slice(block * 16, 16), matrix))
                    nonzero |= 1u << block;
        }
        return nonzero;
    }

    // A value snapshot: no source/matrix span escapes preparation. Luma4 may
    // price/reject it before publishing reconstruction or selected levels.
    internal readonly struct PreparedBlock
    {
        private readonly Vector256<short> raster;
        internal Vector256<short> Levels { get; }
        internal uint Mask { get; }
        internal bool FlatNonDc => BitOperations.PopCount(Mask & 0xfffeu) <= 3;
        internal PreparedBlock(Vector256<short> raster, Vector256<short> levels)
        {
            this.raster = raster; Levels = levels;
            Mask = ~Vector256.ExtractMostSignificantBits(Vector256.Equals(levels, Vector256<short>.Zero)) & 0xffffu;
        }
        internal Vector256<short> Raster => raster;
        internal void PublishRaster(Span<short> output) => raster.CopyTo(output);
        internal void PublishLevels(Span<short> output) => Levels.CopyTo(output);
    }

    // Caller guards AVX512F. Snapshot fixed operands once for ordered candidate
    // searches; the one-shot entry uses the same widened Q17 kernel. These
    // values contain no borrowed views and fit the enclosing admitted scratch.
    internal readonly struct FullBlockQuantizer
    {
        private readonly Vector512<int> sharpening, reciprocals, biases, thresholds, steps;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal FullBlockQuantizer(Vp8QuantizationMatrix matrix)
        {
            sharpening = Vector512.Create(matrix.Sharpening);
            reciprocals = Vector512.Create(matrix.Reciprocals);
            biases = Vector512.Create(matrix.Biases);
            thresholds = Vector512.Create(matrix.ZeroThresholds);
            steps = Vector512.Create(matrix.Steps);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal PreparedBlock Prepare(ReadOnlySpan<short> coefficients)
        {
            var input = Avx512F.ConvertToVector512Int32(Vector256.Create(coefficients));
            var magnitude = Avx512F.Add(Avx512F.Abs(input).AsInt32(), sharpening);
            var scaled = Avx512F.Add(Avx512F.MultiplyLow(magnitude, reciprocals), biases);
            var level = Avx512F.Min(Vector512.Create(2047), Avx512F.ShiftRightArithmetic(scaled, 17));
            var negative = Avx512F.CompareGreaterThan(Vector512<int>.Zero, input);
            var signed = Avx512F.Subtract(Avx512F.Xor(level, negative), negative);
            var quantized = Avx512F.And(signed, Avx512F.CompareGreaterThan(magnitude, thresholds));
            var raster = Avx512F.MultiplyLow(quantized, steps);
            var diagonal = Avx512F.PermuteVar16x32(quantized, ZigzagOrder);
            return new(Avx512F.ConvertToVector256Int16WithSaturation(raster),
                Avx512F.ConvertToVector256Int16WithSaturation(diagonal));
        }
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
        // Generated steps/reciprocals/biases bound every reconstruction below
        // signed16, including the Walsh domain. Packing cannot saturate.
        var product0 = Avx2.MultiplyLow(first, Vector256.Create(matrix.Steps));
        var product1 = Avx2.MultiplyLow(second, Vector256.Create(matrix.Steps[8..]));
        var raster = Avx2.Permute4x64(Avx2.PackSignedSaturate(product0, product1).AsInt64(), 0xd8).AsInt16();
        var quantizedRaster = Avx2.Permute4x64(Avx2.PackSignedSaturate(first, second).AsInt64(), 0xd8).AsByte();
        var low = Vector256.Create(quantizedRaster.GetLower(), quantizedRaster.GetLower());
        var high = Vector256.Create(quantizedRaster.GetUpper(), quantizedRaster.GetUpper());
        var zigzag = Avx2.Or(Avx2.Shuffle(low, ZigzagLow), Avx2.Shuffle(high, ZigzagHigh)).AsInt16();
        raster.CopyTo(coefficients); zigzag.CopyTo(levels);
        return Avx2.MoveMask(Avx2.CompareEqual(Avx2.Or(first, second), Vector256<int>.Zero).AsByte()) != -1;
    }
}
