using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;
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

    [Fact]
    public void PairedPopulationsMatchFrozenCostsWithoutChangingEitherInput()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var encoded = new byte[8]; var cases = 0; long pairedCounts = 0;
        foreach (var length in new[] { 0, 1, 3, 4, 5, 40, 256, 280, 1304 })
        {
            var a = new int[length]; var b = new int[length]; var sum = new int[length];
            foreach (var value in new[] { 0, 1, 2, 3, 255, 256, 257, 4095, 4096, 65535, 65536, 200000 })
            {
                Array.Fill(a, value / 2); Array.Fill(b, value - value / 2); CheckPair();
                for (var i = 0; i < length; i++) { a[i] = (i & 1) == 0 ? value : 0; b[i] = value - a[i]; }
                CheckPair();
                foreach (var run in new[] { 1, 2, 3, 4, 5, 17 })
                {
                    for (var i = 0; i < length; i++)
                    {
                        var count = (i / run % 3) switch { 0 => 0, 1 => value, _ => value + 1 };
                        a[i] = count / 3; b[i] = count - a[i];
                    }
                    CheckPair();
                }
            }
            uint state = 0x9e3779b9;
            for (var sample = 0; sample < 1024; sample++)
            {
                for (var i = 0; i < length; i++)
                {
                    a[i] = Next(); b[i] = Next();
                }
                CheckPair();
            }
            int Next()
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                return (state & 7) == 0 ? (int)(state % 200001) : (state & 3) == 0 ? (int)(state % 257) : 0;
            }
            void CheckPair()
            {
                for (var i = 0; i < length; i++) sum[i] = a[i] + b[i];
                var before = Checksum(a, b);
                var expected = Vp8lReferenceEntropy.Population(sum, context);
                Assert.Equal(expected, Vp8lReferenceEntropy.PopulationSum(a, b, context));
                Assert.Equal(before, Checksum(a, b));
                BinaryPrimitives.WriteInt64LittleEndian(encoded, expected); hash.AppendData(encoded);
                cases++; pairedCounts += length;
            }
        }
        // Frozen against the independently qualified production Population(a+b),
        // before replacing the clustering primitive. These are original inputs.
        Assert.Equal(10080, cases); Assert.Equal(2120160, pairedCounts);
        Assert.Equal("cf96f30f42a423e1f1b61bec17319d534e9df4af09fb936c9a93ce53c0e879ca",
            Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void PairedPopulationRejectsMismatchedLengths()
    {
        int[] a = [0, 1, 1, 1, 1];
        using var invalid = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentException>(() => Vp8lReferenceEntropy.PopulationSum(a, [], invalid));
    }

    [Fact]
    public void PairedPopulationKeepsFullRangeCostsAcrossNarrowArithmeticBounds()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        foreach (var size in new[] { 3, 4, 5, 256, 1304 })
        foreach (var total in new[] { (1 << 28) - 1, 1 << 28, (1 << 28) + 1, int.MaxValue })
        {
            var a = new int[size]; var b = new int[size]; var combined = new int[size];
            for (var i = 0; i < size; i++)
            {
                combined[i] = total / size + (i < total % size ? 1 : 0);
                a[i] = combined[i] / 2; b[i] = combined[i] - a[i];
            }
            Assert.Equal(Vp8lReferenceEntropy.Population(combined, context),
                Vp8lReferenceEntropy.PopulationSum(a, b, context));
        }
        foreach (var count in new[] { 65535, 65536, 65537, 131072 })
        {
            var a = new int[1304]; var b = new int[1304]; var combined = new int[1304];
            Array.Fill(a, count); Array.Fill(b, 1); Array.Fill(combined, count + 1);
            Assert.Equal(Vp8lReferenceEntropy.Population(combined, context),
                Vp8lReferenceEntropy.PopulationSum(a, b, context));
        }
    }


    [Fact]
    public void PairedPopulationHonorsCancellationBeforeReadingInputs()
    {
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceEntropy.PopulationSum([1], [2], context));
    }

    [Fact]
    public void PairedPopulationAllocatesNoManagedStorage()
    {
        int[] a = [0, 1, 2, 3, 255, 256, 257, 4095, 4096, 65535, 65536, 200000];
        var b = new int[a.Length]; Array.Fill(b, 1);
        using var context = new OperationContext(ImageLimits.Default, default); long sum = 0;
        for (var i = 0; i < 1024; i++) sum ^= Vp8lReferenceEntropy.PopulationSum(a, b, context);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1024; i++) sum ^= Vp8lReferenceEntropy.PopulationSum(a, b, context);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, sum); Assert.Equal(0, allocated);
    }

    private static ulong Checksum(ReadOnlySpan<int> a, ReadOnlySpan<int> b)
    {
        ulong value = 0;
        for (var i = 0; i < a.Length; i++) value = unchecked(value * 31 + (uint)a[i] * 257u + (uint)b[i]);
        return value;
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
