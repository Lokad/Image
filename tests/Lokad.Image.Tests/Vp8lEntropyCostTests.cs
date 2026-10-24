using System.Numerics;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8lEntropyCostTests
{
    [Fact]
    public void EverySmallAndReducedCountPreservesOriginalMathematicalRounding()
    {
        for (var count = -2; count < 65536; count++) Check(count);
    }

    [Fact]
    public void LargeCountsKeepOriginalFormulasAndThresholds()
    {
        foreach (var count in new[] { int.MinValue, -1, 0, 1, 255, 256, 257, 4095, 4096, 4097, 65535, 65536, 65537, int.MaxValue }) Check(count);
        for (var power = 16; power <= 30; power++)
        for (var offset = -2; offset <= 2; offset++) Check((1 << power) + offset);
        uint state = 0x9e3779b9;
        for (var i = 0; i < 65536; i++)
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            Check((int)(state & int.MaxValue));
        }
    }

    [Fact]
    public void MathematicalConstantsAllocateNoManagedStorage()
    {
        long sum = 0;
        for (var count = 0; count < 65536; count++) sum += Vp8lReferenceCost.SLog(count) ^ Vp8lReferenceEntropy.Log(count);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var count = 0; count < 65536; count++) sum += Vp8lReferenceCost.SLog(count) ^ Vp8lReferenceEntropy.Log(count);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.NotEqual(0, sum); Assert.Equal(0, allocated);
    }

    private static void Check(int count)
    {
        const long scale = 1L << 23;
        long logarithm, product;
        if (count < 2) logarithm = product = 0;
        else if (count < 256)
        {
            logarithm = (long)Math.Round(scale * Math.Log2(count));
            product = (long)Math.Round(scale * Math.Log2(count) * count);
        }
        else if (count >= 65536)
        {
            logarithm = (long)Math.Floor(scale / Math.Log(2) * Math.Log(count) + 0.5);
            product = (long)Math.Floor(scale / Math.Log(2) * count * Math.Log(count) + 0.5);
        }
        else
        {
            var shift = BitOperations.Log2((uint)count) - 7; var head = count >> shift;
            var rounded = (long)Math.Round(scale * Math.Log2(head)) + shift * scale;
            product = count * rounded + (long)(scale / Math.Log(2)) * (count - (head << shift));
            logarithm = rounded;
            if (count >= 4096) logarithm += ((long)(scale / Math.Log(2)) * (count & ((1 << shift) - 1)) + count / 2) / count;
        }
        Assert.Equal(logarithm, Vp8lReferenceEntropy.Log(count));
        Assert.Equal(product, Vp8lReferenceCost.SLog(count));
    }
}
