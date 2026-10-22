using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegReferenceAdmissionTests
{
    [Fact]
    public void TableScratchCoversRetainedAndReplacementCapacityAndRefundsIndependently()
    {
        var histogram = Enumerable.Repeat(1, 256).ToArray();
        using var root = new TestMemoryBudget(1024 * 1024);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default);
        context.Reserve(16384); // Owner of the returned canonical table, established before allocation.
        var before = GC.GetAllocatedBytesForCurrentThread();
        var table = JpegReferenceTables.Huffman(histogram, context);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(16384, root.Bytes);
        // Cumulative allocated bytes are stronger than the live-storage bound:
        // this includes every discarded backing array during list replacement.
        Assert.True(allocated <= root.Peak, $"Allocated {allocated}, admitted peak {root.Peak}.");
        Assert.Equal(16384 + 65536 + 257 * 2048, root.Peak);
        Assert.Equal(256, table.Symbols.Length);
        Assert.Equal("8a10a4806270e1a3763dc644c5de0eb7e799edeb0d62edbca648728989f3bc48",
            Convert.ToHexStringLower(SHA256.HashData(table.Counts.Concat(table.Symbols).ToArray())));
        Assert.All(table.Lengths, length => Assert.InRange(length, (byte)1, (byte)16));
        for (var i = 0; i < 256; i++) Assert.NotEqual((1 << table.Lengths[i]) - 1, table.Codes[i]);
        context.Release(16384); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
