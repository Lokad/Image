using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8PreparedQuantizationTests
{
    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void PlaneSequencePreservesIndependentBlocksNonzeroBitsAndGuardedTails(int count)
    {
        // The single-block kernel has independent scalar/native golden coverage.
        // Check the new sequence boundary, block order and publication against it.
        var values = new int[80]; var coefficients = new short[count * 16 + 4];
        var levels = new short[coefficients.Length];
        Array.Fill(coefficients, (short)29999); Array.Fill(levels, (short)-29999);
        for (var p = 0; p < 16; p++)
        {
            var step = 4 + (p % 4) * 4; var reciprocal = 131072 / step; var bias = 110 << 9;
            values[p] = step; values[16 + p] = reciprocal; values[32 + p] = bias;
            values[48 + p] = (131071 - bias) / reciprocal; values[64 + p] = p % 3;
        }
        for (var b = 0; b < count; b++) for (var p = 0; p < 16; p++)
            coefficients[2 + b * 16 + p] = b % 3 == 0 ? (short)0
                : (short)((b & 1) == 0 ? 16320 - p * 73 : -16320 + p * 131);
        var original = coefficients.ToArray(); var expected = coefficients.ToArray(); var expectedLevels = levels.ToArray();
        var matrix = new Vp8QuantizationMatrix(values); uint nonzero = 0;
        for (var b = 0; b < count; b++)
            if (Vp8BlockQuantization.Apply(expected.AsSpan(2 + b * 16, 16), expectedLevels.AsSpan(2 + b * 16, 16), matrix))
                nonzero |= 1u << b;
        Assert.Equal(nonzero, Vp8BlockQuantization.ApplyBlocks(coefficients.AsSpan(2), levels.AsSpan(2), matrix, count));
        Assert.Equal(expected, coefficients); Assert.Equal(expectedLevels, levels);
        for (var repeat = 0; repeat < 100; repeat++)
        {
            original.CopyTo(coefficients, 0);
            Vp8BlockQuantization.ApplyBlocks(coefficients.AsSpan(2), levels.AsSpan(2), matrix, count);
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var repeat = 0; repeat < 1000; repeat++)
        {
            original.CopyTo(coefficients, 0);
            Vp8BlockQuantization.ApplyBlocks(coefficients.AsSpan(2), levels.AsSpan(2), matrix, count);
        }
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DelayedPublicationRetainsExactQuantizationRatesAndIndependentValueLifetime(int initialContext)
    {
        if (!Avx512F.IsSupported || !Avx2.IsSupported) return;
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var rate = Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, context);
        var values = new int[80]; var input = new short[18]; var raster = new short[18]; var levels = new short[18];
        var expectedRaster = new short[16]; var expectedLevels = new short[16];
        int[] order = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        uint random = 0x917a2cd1; var retained = root.Bytes; var zeroBlocks = 0; var maximumLevel = 0;
        for (var block = 0; block < 384; block++)
        {
            for (var p = 0; p < 16; p++)
            {
                var bias = (p == 0 ? 96 : 110) << 9;
                values[p] = 4; values[16 + p] = 32768; values[32 + p] = bias;
                values[48 + p] = (131071 - bias) / 32768; values[64 + p] = p % 3;
                random ^= random << 13; random ^= random >> 17; random ^= random << 5;
                input[p] = (short)((block % 4) switch { 0 => 0, 1 => p == (block / 4) % 16 ? 16320 : 0,
                    2 => (p & 1) == 0 ? 16320 : -16320, _ => (int)(random % 32641) - 16320 });
                var magnitude = Math.Abs((int)input[p]) + values[64 + p];
                var level = magnitude <= values[48 + p] ? 0 : Math.Min(2047, (magnitude * 32768 + bias) >> 17);
                if (input[p] < 0) level = -level;
                expectedRaster[p] = (short)(level * 4);
            }
            for (var p = 0; p < 16; p++) expectedLevels[p] = (short)(expectedRaster[order[p]] / 4);
            if (block % 4 == 0) { Assert.True(input[..16].All(v => v == 0)); zeroBlocks++; }
            maximumLevel = Math.Max(maximumLevel, expectedLevels.Max(v => Math.Abs((int)v)));
            input[16] = 30000; input[17] = -30000;
            var before = input.ToArray();
            var quantizer = new Vp8BlockQuantization.FullBlockQuantizer(new(values));
            var prepared = quantizer.Prepare(input);
            Assert.Equal(before, input);
            Assert.Equal(expectedLevels.Any(v => v != 0), prepared.Mask != 0);
            Assert.Equal(expectedLevels.Skip(1).Count(v => v != 0) <= 3, prepared.FlatNonDc);
            var expected = rate.Residual(expectedLevels, 3, 0, initialContext, out _);
            Assert.Equal(expected, Vp8RatePlan.Luma4Prepared(rate.Values, prepared, initialContext));
            input.AsSpan(0, 16).Fill(short.MinValue); Array.Fill(values, 0);
            Array.Fill(raster, (short)28000); Array.Fill(levels, (short)-28000);
            prepared.PublishRaster(raster); prepared.PublishLevels(levels);
            Assert.Equal(expectedRaster, raster[..16]); Assert.Equal(expectedLevels, levels[..16]);
            // The search reuses fixed operands without retaining its matrix.
            // Mutating that source must leave later prepared results independent.
            var repeated = quantizer.Prepare(before);
            repeated.PublishRaster(raster); repeated.PublishLevels(levels);
            Assert.Equal(expectedRaster, raster[..16]); Assert.Equal(expectedLevels, levels[..16]);
            Assert.Equal((28000, 28000, -28000, -28000), ((int)raster[16], (int)raster[17], (int)levels[16], (int)levels[17]));
        }
        Assert.Equal(96, zeroBlocks); Assert.Equal(2047, maximumLevel);
        // Artificial int32 wrap is part of the general quantizer's frozen domain;
        // it must still be rejected before indexing the bounded residual tables.
        for (var p = 0; p < 16; p++) { values[p] = 1; values[16 + p] = 131072; }
        Array.Clear(input); input[0] = 16384;
        var invalid = new Vp8BlockQuantization.FullBlockQuantizer(new(values)).Prepare(input);
        Assert.Throws<ArgumentException>(() => Vp8RatePlan.Luma4Prepared(rate.Values, invalid, initialContext));
        var empty = default(Vp8BlockQuantization.PreparedBlock);
        Assert.Throws<ArgumentException>(() => Vp8RatePlan.Luma4Prepared(rate.Values, empty, 3));
        for (var p = 0; p < 16; p++) { values[p] = 4; values[16 + p] = 32768; }
        input.AsSpan(0, 16).Fill(37);
        var reused = new Vp8BlockQuantization.FullBlockQuantizer(new(values));
        for (var i = 0; i < 1000; i++) Vp8RatePlan.Luma4Prepared(rate.Values, reused.Prepare(input), initialContext);
        var start = GC.GetAllocatedBytesForCurrentThread(); var checksum = 0;
        for (var i = 0; i < 10000; i++) checksum ^= Vp8RatePlan.Luma4Prepared(rate.Values, reused.Prepare(input), initialContext);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - start); Assert.Equal(0, checksum); Assert.Equal(retained, root.Bytes);
    }
}
