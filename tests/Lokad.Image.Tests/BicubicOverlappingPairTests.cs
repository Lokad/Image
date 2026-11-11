using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class BicubicOverlappingPairTests
{
    [Theory]
    [InlineData(255, false, 4, 2)] [InlineData(255, true, 4, 2)]
    [InlineData(65535, false, 4, 2)] [InlineData(65535, true, 4, 2)]
    [InlineData(255, false, 8, 2)] [InlineData(255, true, 8, 2)]
    [InlineData(65535, false, 8, 2)] [InlineData(65535, true, 8, 2)]
    [InlineData(255, false, 16, 4)] [InlineData(255, true, 16, 4)]
    [InlineData(65535, false, 16, 4)] [InlineData(65535, true, 16, 4)]
    public void SharedWindowsPreserveEveryOrderedDoubleAndBorrowedInput(int maximum, bool transparent, int taps, int stride)
    {
        if (!Avx512F.IsSupported) return;
        var random = new Random(715);
        var input = new Vector256<double>[taps + stride + 2];
        var weights0 = new double[taps + 2]; var weights1 = new double[taps + 2];
        for (var trial = 0; trial < 32; trial++)
        {
            for (var p = 0; p < input.Length; p++)
            {
                var a = transparent ? random.Next(3) : random.Next(maximum + 1);
                input[p] = Vector256.Create((double)a * random.Next(maximum + 1),
                    (double)a * random.Next(maximum + 1), (double)a * random.Next(maximum + 1), a);
            }
            for (var t = 0; t < weights0.Length; t++)
            {
                weights0[t] = t % 2 == 0 ? (t + 1) * .1 : -(t + 1) * .0625;
                weights1[t] = t % 3 == 0 ? -(t + 1) * .01 : (t + 1) * .125;
            }
            var saved = input.ToArray(); var saved0 = weights0.ToArray(); var saved1 = weights1.ToArray();
            var pair = BicubicMath.FilterOverlappingHorizontalPair(input.AsSpan(1, taps + stride),
                weights0.AsSpan(1, taps), weights1.AsSpan(1, taps), taps, stride);
            for (var c = 0; c < 8; c++)
            {
                double expected = 0;
                for (var tap = 0; tap < taps; tap++)
                    expected += Product(input[1 + tap + (c >= 4 ? stride : 0)].GetElement(c & 3),
                        (c < 4 ? weights0 : weights1)[tap + 1]);
                Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(pair.GetElement(c)));
            }
            Assert.Equal(saved, input); Assert.Equal(saved0, weights0); Assert.Equal(saved1, weights1);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double Product(double a, double b) => a * b;

    [Theory]
    [InlineData(255, false)] [InlineData(255, true)]
    [InlineData(65535, false)] [InlineData(65535, true)]
    public void VerticalWindowsPreserveBothColumnPairsAndEveryOrderedDouble(int maximum, bool transparent)
    {
        if (!Avx512F.IsSupported) return;
        var random = new Random(720);
        var input = new Vector512<double>[12];
        var weights0 = new double[10]; var weights1 = new double[10];
        for (var trial = 0; trial < 32; trial++)
        {
            for (var row = 0; row < input.Length; row++)
            {
                var a = transparent ? random.Next(3) : random.Next(maximum + 1);
                var b = transparent ? random.Next(3) : random.Next(maximum + 1);
                input[row] = Vector512.Create(Vector256.Create((double)a * random.Next(maximum + 1),
                    (double)a * random.Next(maximum + 1), (double)a * random.Next(maximum + 1), a),
                    Vector256.Create((double)b * random.Next(maximum + 1), (double)b * random.Next(maximum + 1),
                        (double)b * random.Next(maximum + 1), b));
            }
            for (var tap = 0; tap < weights0.Length; tap++)
            {
                weights0[tap] = tap % 2 == 0 ? (tap + 1) * .1 : -(tap + 1) * .0625;
                weights1[tap] = tap % 3 == 0 ? -(tap + 1) * .01 : (tap + 1) * .125;
            }
            var saved = input.ToArray(); var saved0 = weights0.ToArray(); var saved1 = weights1.ToArray();
            BicubicMath.FilterOverlappingVerticalPair(input.AsSpan(1, 10),
                weights0.AsSpan(1, 8), weights1.AsSpan(1, 8), out var first, out var second);
            for (var row = 0; row < 2; row++) for (var channel = 0; channel < 8; channel++)
            {
                double expected = 0;
                for (var tap = 0; tap < 8; tap++)
                    expected += Product(input[1 + tap + row * 2].GetElement(channel),
                        (row == 0 ? weights0 : weights1)[tap + 1]);
                Assert.Equal(BitConverter.DoubleToInt64Bits(expected),
                    BitConverter.DoubleToInt64Bits((row == 0 ? first : second).GetElement(channel)));
            }
            Assert.Equal(saved, input); Assert.Equal(saved0, weights0); Assert.Equal(saved1, weights1);
        }
    }

    [Theory]
    [InlineData(8, 2, 10)] [InlineData(16, 4, 17)]
    public void PreparationResetsStaleProofsAndPreservesLiveTapsFullColumnsAndOddTail(int taps, int stride, int capacity)
    {
        var counts = new[] { taps, taps, taps, taps + 1, capacity, capacity, taps };
        var savedCounts = counts.ToArray();
        var offsets = Enumerable.Repeat((byte)254, counts.Length * capacity).ToArray();
        for (var x = 0; x < counts.Length; x++) for (var t = 0; t < counts[x]; t++)
            offsets[x * capacity + t] = (byte)(3 + x * stride + t);
        var original = offsets.ToArray();
        BicubicMath.PrepareOverlappingPairs(offsets, counts, counts.Length, 48, capacity, taps, stride, default);
        for (var x = 0; x < counts.Length; x++)
        {
            Assert.Equal(original.AsSpan(x * capacity, counts[x]).ToArray(), offsets.AsSpan(x * capacity, counts[x]).ToArray());
            if (counts[x] < capacity) Assert.Equal(x == 0 ? (byte)4 : (byte)0, offsets[x * capacity + capacity - 1]);
        }
        offsets[capacity + 4]++; // A later strip reuses the same storage with a discontinuity.
        var changed = offsets.ToArray();
        BicubicMath.PrepareOverlappingPairs(offsets, counts, counts.Length, 48, capacity, taps, stride, default);
        Assert.Equal(0, offsets[capacity - 1]);
        for (var x = 0; x < counts.Length; x++)
            Assert.Equal(changed.AsSpan(x * capacity, counts[x]).ToArray(), offsets.AsSpan(x * capacity, counts[x]).ToArray());
        Assert.Equal(savedCounts, counts);
    }

    [Theory]
    [InlineData(8, 2, 10, 0, 9)] [InlineData(8, 2, 10, 20, 29)] [InlineData(8, 2, 10, 247, 256)]
    [InlineData(16, 4, 17, 0, 19)] [InlineData(16, 4, 17, 20, 39)] [InlineData(16, 4, 17, 237, 256)]
    public void IncompleteSourceWindowsNeverAcquireAProof(int taps, int stride, int capacity, byte first, int sourceCount)
    {
        var offsets = Enumerable.Repeat((byte)253, 2 * capacity).ToArray(); var counts = new[] { taps, taps };
        for (var t = 0; t < taps; t++) { offsets[t] = (byte)(first + t); offsets[capacity + t] = (byte)(first + t + stride); }
        var saved = offsets.ToArray();
        BicubicMath.PrepareOverlappingPairs(offsets, counts, 2, sourceCount, capacity, taps, stride, default);
        Assert.Equal(0, offsets[capacity - 1]); Assert.Equal(0, offsets[2 * capacity - 1]);
        Assert.Equal(saved.AsSpan(0, taps).ToArray(), offsets.AsSpan(0, taps).ToArray());
        Assert.Equal(saved.AsSpan(capacity, taps).ToArray(), offsets.AsSpan(capacity, taps).ToArray());
    }

    [Theory]
    [InlineData(8, 2, 10)] [InlineData(16, 4, 17)]
    public void CanceledPreparationLeavesBorrowedMetadataUntouched(int taps, int stride, int capacity)
    {
        var offsets = Enumerable.Repeat((byte)253, 2 * capacity).ToArray(); var saved = offsets.ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() =>
            BicubicMath.PrepareOverlappingPairs(offsets, new[] { taps, taps }, 2, 48, capacity, taps, stride, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(saved, offsets);
    }

    [Theory]
    [InlineData(8, 2)] [InlineData(16, 4)]
    public void FullCapacityHasNoPaddingToHoldAProof(int taps, int stride)
    {
        var offsets = new byte[2 * taps]; var counts = new[] { taps, taps };
        for (var t = 0; t < taps; t++) { offsets[t] = (byte)t; offsets[taps + t] = (byte)(t + stride); }
        var saved = offsets.ToArray();
        BicubicMath.PrepareOverlappingPairs(offsets, counts, 2, taps + stride, taps, taps, stride, default);
        Assert.Equal(saved, offsets);
    }
}
