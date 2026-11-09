using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class BicubicOverlappingPairTests
{
    [Theory]
    [InlineData(255, false)] [InlineData(255, true)]
    [InlineData(65535, false)] [InlineData(65535, true)]
    public void SharedWindowsPreserveEveryOrderedDoubleAndBorrowedInput(int maximum, bool transparent)
    {
        if (!Avx512F.IsSupported) return;
        var random = new Random(715);
        var input = new Vector256<double>[12];
        var weights0 = new double[10]; var weights1 = new double[10];
        for (var trial = 0; trial < 32; trial++)
        {
            for (var p = 0; p < input.Length; p++)
            {
                var a = transparent ? random.Next(3) : random.Next(maximum + 1);
                input[p] = Vector256.Create((double)a * random.Next(maximum + 1),
                    (double)a * random.Next(maximum + 1), (double)a * random.Next(maximum + 1), a);
            }
            for (var t = 0; t < 10; t++)
            {
                weights0[t] = t % 2 == 0 ? (t + 1) * .1 : -(t + 1) * .0625;
                weights1[t] = t % 3 == 0 ? -(t + 1) * .01 : (t + 1) * .125;
            }
            var saved = input.ToArray(); var saved0 = weights0.ToArray(); var saved1 = weights1.ToArray();
            var pair = BicubicMath.FilterOverlappingHorizontalPair(input.AsSpan(1, 10),
                weights0.AsSpan(1, 8), weights1.AsSpan(1, 8));
            for (var c = 0; c < 8; c++)
            {
                double expected = 0;
                for (var tap = 0; tap < 8; tap++)
                    expected += Product(input[1 + tap + (c >= 4 ? 2 : 0)].GetElement(c & 3),
                        (c < 4 ? weights0 : weights1)[tap + 1]);
                Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(pair.GetElement(c)));
            }
            Assert.Equal(saved, input); Assert.Equal(saved0, weights0); Assert.Equal(saved1, weights1);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double Product(double a, double b) => a * b;

    [Fact]
    public void PreparationResetsStaleProofsAndPreservesLiveTapsFullColumnsAndOddTail()
    {
        var counts = new[] { 8, 8, 8, 9, 10, 10, 8 };
        var offsets = Enumerable.Repeat((byte)254, counts.Length * 10).ToArray();
        for (var x = 0; x < counts.Length; x++) for (var t = 0; t < counts[x]; t++)
            offsets[x * 10 + t] = (byte)(3 + x * 2 + t);
        var original = offsets.ToArray();
        ColumnBicubicResize.PrepareOverlappingPairs(offsets, counts, counts.Length, 32, default);
        for (var x = 0; x < counts.Length; x++)
        {
            Assert.Equal(original.AsSpan(x * 10, counts[x]).ToArray(), offsets.AsSpan(x * 10, counts[x]).ToArray());
            if (counts[x] < 10) Assert.Equal(x == 0 ? (byte)4 : (byte)0, offsets[x * 10 + 9]);
        }
        offsets[10 + 4]++; // A later strip reuses the same storage with a discontinuity.
        var changed = offsets.ToArray();
        ColumnBicubicResize.PrepareOverlappingPairs(offsets, counts, counts.Length, 32, default);
        Assert.Equal(0, offsets[9]);
        for (var x = 0; x < counts.Length; x++)
            Assert.Equal(changed.AsSpan(x * 10, counts[x]).ToArray(), offsets.AsSpan(x * 10, counts[x]).ToArray());
        Assert.Equal(new[] { 8, 8, 8, 9, 10, 10, 8 }, counts);
    }

    [Theory]
    [InlineData(0, 9)] [InlineData(20, 29)] [InlineData(247, 256)]
    public void IncompleteSourceWindowsNeverAcquireAProof(byte first, int sourceCount)
    {
        var offsets = Enumerable.Repeat((byte)253, 20).ToArray(); var counts = new[] { 8, 8 };
        for (var t = 0; t < 8; t++) { offsets[t] = (byte)(first + t); offsets[10 + t] = (byte)(first + t + 2); }
        var saved = offsets.ToArray();
        ColumnBicubicResize.PrepareOverlappingPairs(offsets, counts, 2, sourceCount, default);
        Assert.Equal(0, offsets[9]); Assert.Equal(0, offsets[19]);
        Assert.Equal(saved.AsSpan(0, 8).ToArray(), offsets.AsSpan(0, 8).ToArray());
        Assert.Equal(saved.AsSpan(10, 8).ToArray(), offsets.AsSpan(10, 8).ToArray());
    }

    [Fact]
    public void CanceledPreparationLeavesBorrowedMetadataUntouched()
    {
        var offsets = Enumerable.Repeat((byte)253, 20).ToArray(); var saved = offsets.ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() =>
            ColumnBicubicResize.PrepareOverlappingPairs(offsets, new[] { 8, 8 }, 2, 32, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(saved, offsets);
    }
}
