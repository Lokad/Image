using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegGrayQuantizationTests
{
    [Fact]
    public void EverySignedCoefficientAndQuantizerMatchesDivision()
    {
        Span<short> source = stackalloc short[64], output = stackalloc short[64];
        Span<ushort> table = stackalloc ushort[64], reciprocals = stackalloc ushort[64];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var blocks = 0;
        for (var q = 1; q <= 255; q++)
        {
            table.Fill((ushort)q); JpegGrayQuantization.Prepare(table, reciprocals);
            var divisor = q * 8;
            for (var start = -32768; start < 32768; start += 64)
            {
                for (var k = 0; k < 64; k++) source[k] = (short)(start + k);
                JpegGrayQuantization.Quantize(source, table, reciprocals, output);
                for (var k = 0; k < 64; k++)
                {
                    int value = source[k]; var exact = (Math.Abs(value) + divisor / 2) / divisor;
                    Assert.Equal(value < 0 ? -exact : exact, output[k]);
                    Assert.Equal(start + k, source[k]); Assert.Equal(q, table[k]);
                }
                hash.AppendData(MemoryMarshal.AsBytes(output)); blocks++;
            }
        }
        uint seed = 0x6d2b79f5;
        for (var block = 0; block < 16384; block++)
        {
            for (var k = 0; k < 64; k++)
            {
                seed ^= seed << 13; seed ^= seed >> 17; seed ^= seed << 5;
                source[k] = (short)seed; table[k] = (ushort)(1 + (block * 67 + k * 31) % 255);
            }
            JpegGrayQuantization.Prepare(table, reciprocals);
            JpegGrayQuantization.Quantize(source, table, reciprocals, output);
            for (var k = 0; k < 64; k++)
            {
                int value = source[k], divisor = table[k] * 8;
                var exact = (Math.Abs(value) + divisor / 2) / divisor;
                Assert.Equal(value < 0 ? -exact : exact, output[k]);
            }
            hash.AppendData(MemoryMarshal.AsBytes(output)); blocks++;
        }
        Assert.Equal(277504, blocks);
        Assert.Equal("9c9a66371046df0798963b0844877447d95d1299067e99127c818d20f08bafba",
            Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void UnalignedAndInPlaceBlocksKeepGuardsAndBorrowedTables()
    {
        var source = Enumerable.Range(0, 68).Select(k => (short)(k * 1031 - 32768)).ToArray();
        var before = source.ToArray(); var output = Enumerable.Repeat((short)777, 69).ToArray();
        var table = Enumerable.Range(0, 66).Select(k => (ushort)(1 + k * 31 % 255)).ToArray();
        var tableBefore = table.ToArray(); var reciprocals = Enumerable.Repeat((ushort)777, 67).ToArray();
        JpegGrayQuantization.Prepare(table.AsSpan(1, 64), reciprocals.AsSpan(2, 64));
        var reciprocalBefore = reciprocals.ToArray();
        JpegGrayQuantization.Quantize(source.AsSpan(1, 64), table.AsSpan(1, 64), reciprocals.AsSpan(2, 64), output.AsSpan(2, 64));
        JpegGrayQuantization.Quantize(source.AsSpan(1, 64), table.AsSpan(1, 64), reciprocals.AsSpan(2, 64), source.AsSpan(1, 64));
        Assert.Equal(output.AsSpan(2, 64).ToArray(), source.AsSpan(1, 64).ToArray());
        Assert.Equal(before[0], source[0]); Assert.Equal(before.AsSpan(65).ToArray(), source.AsSpan(65).ToArray());
        Assert.All(output.Take(2).Concat(output.Skip(66)), value => Assert.Equal((short)777, value));
        Assert.Equal(tableBefore, table); Assert.Equal(reciprocalBefore, reciprocals);
        Assert.Equal((ushort)777, reciprocals[0]); Assert.Equal((ushort)777, reciprocals[1]); Assert.Equal((ushort)777, reciprocals[66]);
    }

    [Fact]
    public void CompletePreparedBlocksAreRequired()
    {
        var table = Enumerable.Repeat((ushort)1, 64).ToArray(); var prepared = new ushort[64];
        var source = new short[64]; var output = Enumerable.Repeat((short)777, 64).ToArray();
        foreach (var length in new[] { 0, 63, 65 })
        {
            Assert.Throws<ArgumentException>(() => JpegGrayQuantization.Prepare(new ushort[length], prepared));
            Assert.Throws<ArgumentException>(() => JpegGrayQuantization.Prepare(table, new ushort[length]));
            Assert.Throws<ArgumentException>(() => JpegGrayQuantization.Quantize(new short[length], table, prepared, output));
            Assert.Throws<ArgumentException>(() => JpegGrayQuantization.Quantize(source, new ushort[length], prepared, output));
            Assert.Throws<ArgumentException>(() => JpegGrayQuantization.Quantize(source, table, new ushort[length], output));
            Assert.Throws<ArgumentException>(() => JpegGrayQuantization.Quantize(source, table, prepared, new short[length]));
        }
        foreach (var invalid in new ushort[] { 0, 256, ushort.MaxValue })
        {
            table[63] = invalid; Assert.Throws<ArgumentException>(() => JpegGrayQuantization.Prepare(table, prepared));
        }
        Assert.All(output, value => Assert.Equal((short)777, value));
    }

    [Fact]
    public void WarmPreparationAndQuantizationAddNoAllocationOrAdmission()
    {
        using var root = new TestMemoryBudget(10000);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default);
        context.Reserve(8192); var attempts = root.Attempts;
        Span<short> source = stackalloc short[64], output = stackalloc short[64];
        Span<ushort> table = stackalloc ushort[64], reciprocals = stackalloc ushort[64];
        source.Fill(short.MinValue); table.Fill(255);
        for (var k = 0; k < 1000; k++)
        {
            JpegGrayQuantization.Prepare(table, reciprocals);
            JpegGrayQuantization.Quantize(source, table, reciprocals, output);
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var k = 0; k < 10000; k++)
        {
            JpegGrayQuantization.Prepare(table, reciprocals);
            JpegGrayQuantization.Quantize(source, table, reciprocals, output);
        }
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.Equal(8192, root.Bytes); Assert.Equal(attempts, root.Attempts); Assert.Equal(0, context.ConsumedWork);
        context.Release(8192); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
