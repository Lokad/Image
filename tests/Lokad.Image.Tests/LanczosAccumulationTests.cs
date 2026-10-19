using System.Numerics;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class LanczosAccumulationTests
{
    private const long Bound = 65535L * 65535 * (1L << 31);

    [Theory]
    [InlineData(4)] [InlineData(8)] [InlineData(4100)]
    public void SignedLimbExtremaAndEveryPrefixMatchBigInteger(int channels)
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        long[] extremes = [Bound, -Bound, int.MinValue, int.MaxValue, 0, -1, 1, 1L << 32,
            -(1L << 32), (1L << 32) + int.MaxValue, -(1L << 32) + int.MinValue];
        foreach (var weights in new int[][] { [int.MinValue], [int.MaxValue, -1], [1 << 30, -(1 << 30)], [-1, 1, -7, 11] })
        {
            var sums = new Int128[channels]; var expected = new BigInteger[channels]; var row = new long[channels];
            for (var tap = 0; tap < weights.Length; tap++)
            {
                for (var i = 0; i < channels; i++) row[i] = extremes[(i + tap) % extremes.Length];
                LanczosResize.AccumulateVertical(row, sums, weights[tap], context);
                for (var i = 0; i < channels; i++)
                {
                    expected[i] += (BigInteger)row[i] * weights[tap];
                    Assert.Equal(expected[i], (BigInteger)LanczosResize.ReadSum(sums, i));
                }
            }
        }
    }

    [Fact]
    public void DeterministicMixedRowsMatchIndependentWideSums()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        ulong state = 0x7EA561329B4F08CD;
        ulong Next() => state = unchecked(state * 6364136223846793005 + 1442695040888963407);
        var row = new long[12]; var sums = new Int128[12]; var expected = new BigInteger[12];
        for (var trial = 0; trial < 256; trial++)
        {
            Array.Clear(sums); Array.Clear(expected); long remaining = 1L << 31;
            for (var tap = 0; tap < 12; tap++)
            {
                var weight = (int)(Next() % (ulong)(Math.Min(remaining, int.MaxValue) + 1));
                remaining -= weight; if ((Next() & 1) != 0) weight = -weight;
                for (var i = 0; i < row.Length; i++)
                    row[i] = (long)(Next() % (ulong)Bound) * ((Next() & 1) == 0 ? 1 : -1);
                LanczosResize.AccumulateVertical(row, sums, weight, context);
                for (var i = 0; i < row.Length; i++)
                {
                    expected[i] += (BigInteger)row[i] * weight;
                    Assert.Equal(expected[i], (BigInteger)LanczosResize.ReadSum(sums, i));
                }
            }
        }
    }

    [Fact]
    public void CanceledAccumulationLeavesEverySumUnchanged()
    {
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var sums = new Int128[8]; var row = new long[8]; Array.Fill(row, Bound);
        LanczosResize.AccumulateVertical(row, sums, 1, context); var before = sums.ToArray();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => LanczosResize.AccumulateVertical(row, sums, 1, context));
        Assert.Equal(before, sums);
    }
}
