using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8PreparedQuantizationTests
{
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
            var prepared = Vp8BlockQuantization.PrepareFullBlock(input, new(values));
            Assert.Equal(before, input);
            Assert.Equal(expectedLevels.Any(v => v != 0), prepared.Mask != 0);
            Assert.Equal(expectedLevels.Skip(1).Count(v => v != 0) <= 3, prepared.FlatNonDc);
            var expected = rate.Residual(expectedLevels, 3, 0, initialContext, out _);
            Assert.Equal(expected, rate.Luma4Prepared(prepared, initialContext));
            input.AsSpan(0, 16).Fill(short.MinValue); Array.Fill(values, 0);
            Array.Fill(raster, (short)28000); Array.Fill(levels, (short)-28000);
            prepared.PublishRaster(raster); prepared.PublishLevels(levels);
            Assert.Equal(expectedRaster, raster[..16]); Assert.Equal(expectedLevels, levels[..16]);
            Assert.Equal((28000, 28000, -28000, -28000), ((int)raster[16], (int)raster[17], (int)levels[16], (int)levels[17]));
        }
        Assert.Equal(96, zeroBlocks); Assert.Equal(2047, maximumLevel);
        // Artificial int32 wrap is part of the general quantizer's frozen domain;
        // it must still be rejected before indexing the bounded residual tables.
        for (var p = 0; p < 16; p++) { values[p] = 1; values[16 + p] = 131072; }
        Array.Clear(input); input[0] = 16384;
        var invalid = Vp8BlockQuantization.PrepareFullBlock(input, new(values));
        Assert.Throws<ArgumentException>(() => rate.Luma4Prepared(invalid, initialContext));
        var empty = default(Vp8BlockQuantization.PreparedBlock);
        Assert.Throws<ArgumentException>(() => rate.Luma4Prepared(empty, 3));
        for (var p = 0; p < 16; p++) { values[p] = 4; values[16 + p] = 32768; }
        input.AsSpan(0, 16).Fill(37);
        for (var i = 0; i < 1000; i++) rate.Luma4Prepared(Vp8BlockQuantization.PrepareFullBlock(input, new(values)), initialContext);
        var start = GC.GetAllocatedBytesForCurrentThread(); var checksum = 0;
        for (var i = 0; i < 10000; i++) checksum ^= rate.Luma4Prepared(Vp8BlockQuantization.PrepareFullBlock(input, new(values)), initialContext);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - start); Assert.Equal(0, checksum); Assert.Equal(retained, root.Bytes);
    }
}
