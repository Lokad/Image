using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WideWordWeightsTests
{
    [Theory]
    [InlineData(16, 16, 1, true)]
    [InlineData(16, 16, 0, false)]
    [InlineData(17, 16, 1, false)]
    [InlineData(16, 17, 1, false)]
    public void CompactWeightsPreserveOriginalBitsAndOwnedViews(int tapsA, int tapsB, byte first, bool prepared)
    {
        var weights = new double[51];
        for (var i = 0; i < weights.Length; i++)
            weights[i] = i % 11 == 0 ? -0.0 : Math.BitIncrement(Math.ScaleB(i - 25, -7));
        int[] counts = [tapsA, tapsB, 16];
        var offsets = new byte[51];
        offsets[16] = first;
        offsets[50] = 1;
        var original = weights.ToArray();
        var before = Bits(weights);
        var scratch = Enumerable.Repeat(double.NaN, 36).ToArray();
        var guards = Bits(scratch);

        WideColumnBicubicResize.PrepareWordWeights(weights, offsets, counts, scratch.AsSpan(2, 32));
        Assert.Equal(guards[..2], Bits(scratch.AsSpan(0, 2)));
        Assert.Equal(guards[34..], Bits(scratch.AsSpan(34, 2)));
        Assert.Equal(before[32..], Bits(weights.AsSpan(32)));
        if (!prepared)
        {
            Assert.Equal(before, Bits(weights));
            Assert.Equal(guards, Bits(scratch));
            return;
        }

        var packed = Bits(weights);
        for (var lane = 0; lane < 2; lane++)
        {
            WideColumnBicubicResize.RestoreWordWeights(weights.AsSpan(0, 32), lane, scratch.AsSpan(2, 16));
            Assert.Equal(Bits(original.AsSpan(lane * 17, 16)), Bits(scratch.AsSpan(2, 16)));
        }
        Assert.Equal(packed, Bits(weights));
    }

    [Fact]
    public void CompactFilterRetainsEveryOrderedMultiplyAndAddBit()
    {
        if (!Avx512F.IsSupported) return;
        var random = new Random(1297);
        var pixels = new Vector256<double>[22];
        var original = new double[34];
        var packed = new double[34];
        var scratch = new double[32];
        var offsets = new byte[34];
        offsets[16] = 1;
        int[] counts = [16, 16];
        pixels[0] = pixels[^1] = Vector256.Create(double.NaN);

        for (var round = 0; round < 128; round++)
        {
            for (var i = 1; i <= 20; i++)
            {
                var alpha = round % 8 == 0 ? 1d : random.Next(65536);
                pixels[i] = Vector256.Create(random.Next(65536) * alpha, random.Next(65536) * alpha,
                    random.Next(65536) * alpha, alpha);
            }
            for (var i = 0; i < original.Length; i++)
                original[i] = random.NextDouble() * 2 - 1;
            original[3] = -0d;
            original.CopyTo(packed, 0);

            WideColumnBicubicResize.PrepareWordWeights(packed, offsets, counts, scratch);
            var expected = BicubicMath.FilterOverlappingHorizontalPair(pixels.AsSpan(1, 20),
                original.AsSpan(0, 16), original.AsSpan(17, 16), 16, 4);
            var actual = WideColumnBicubicResize.FilterPreparedWordPair(pixels.AsSpan(1, 20), packed.AsSpan(0, 32));
            for (var lane = 0; lane < Vector512<long>.Count; lane++)
                Assert.Equal(expected.AsInt64().GetElement(lane), actual.AsInt64().GetElement(lane));
        }
    }

    private static long[] Bits(ReadOnlySpan<double> values)
        => MemoryMarshal.Cast<double, long>(values).ToArray();
}
