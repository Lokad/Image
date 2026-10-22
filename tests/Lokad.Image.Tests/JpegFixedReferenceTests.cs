using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegFixedReferenceTests
{
    [Fact]
    public void FixedFormatTablesMatchNativeIdentitiesWithinTheirAdmittedOwner()
    {
        using var root = new TestMemoryBudget(16384);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default);
        context.Reserve(16384);
        var tables = new JpegEncodingTable[4];
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 4; i++) tables[i] = JpegFixedReferenceTables.Create(i, context);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(allocated <= 16384, $"Fixed tables allocated {allocated} bytes.");
        Assert.Equal(16384, root.Bytes); Assert.Equal(16384, root.Peak);
        string[] expected = [
            "9fb04cb1a8076318f33f273ffef967a9ea7291ecf3adeca79f643dfd300de425",
            "af48d4f608dfb682fd19ee479c66dad5342dfbed5b1d1807b95393162a8715d1",
            "f27ce9b718f91cd426885e6ce4a37d0ba4900db219f719eef29ecc1f7f4fd5f0",
            "8cf60aa835f8474bb590e73332bee9c246bec9ce1e368aa725674c096f3efd2c"];
        for (var i = 0; i < 4; i++)
        {
            var table = tables[i];
            var payload = new byte[] { (byte)((i / 2 << 4) | i % 2) }.Concat(table.Counts).Concat(table.Symbols).ToArray();
            Assert.Equal(expected[i], Convert.ToHexStringLower(SHA256.HashData(payload)));
            foreach (var symbol in table.Symbols) Assert.NotEqual((1 << table.Lengths[symbol]) - 1, table.Codes[symbol]);
        }
        context.Release(16384); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
