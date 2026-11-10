using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class ColumnPairedBicubicWeightsTests
{
    private static long[] Bits(ReadOnlySpan<double> values)
        => MemoryMarshal.Cast<double, long>(values).ToArray();

    [Fact]
    public void PreparationKeepsGenericFullAndOddColumnsAndCapacity()
    {
        var weights = new double[70];
        for (var i = 0; i < weights.Length; i++)
            weights[i] = i % 11 == 0 ? BitConverter.Int64BitsToDouble(long.MinValue) : Math.ScaleB(i - 35, -7);
        var original = weights.ToArray(); var expected = weights.ToArray();
        int[] counts = [8, 8, 8, 8, 10, 8, 8]; var offsets = new byte[70];
        offsets[9] = 1; offsets[49] = 25; // A full column's last offset is not a marker.
        for (var tap = 0; tap < 8; tap += 2)
        {
            expected[tap * 2] = original[tap]; expected[tap * 2 + 1] = original[tap + 1];
            expected[tap * 2 + 2] = original[10 + tap]; expected[tap * 2 + 3] = original[11 + tap];
        }
        var scratch = Enumerable.Repeat(double.NaN, 20).ToArray(); var guards = Bits(scratch);
        var savedOffsets = offsets.ToArray(); var savedCounts = counts.ToArray();
        ColumnBicubicResize.PreparePairedByteWeights(weights, offsets, counts, scratch.AsSpan(2, 16));
        Assert.Equal(Bits(expected), Bits(weights));
        Assert.Equal(guards[..2], Bits(scratch.AsSpan(0, 2)));
        Assert.Equal(guards[18..], Bits(scratch.AsSpan(18, 2)));
        Assert.Equal(savedOffsets, offsets); Assert.Equal(savedCounts, counts);
        original.CopyTo(weights, 0);
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        ColumnBicubicResize.PreparePairedByteWeights(weights, offsets, counts, scratch.AsSpan(2, 16));
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
        Assert.Equal(Bits(expected), Bits(weights));
    }

    [Fact]
    public void PreparedKernelKeepsOriginalOrderedBitsAndBorrowedViews()
    {
        if (!Avx512F.IsSupported) return;
        for (var pattern = 0; pattern < 12; pattern++)
        {
            var decoded = Enumerable.Repeat(Vector256.Create(double.NaN), 14).ToArray();
            var alpha = (pattern % 4) switch { 0 => 0, 1 => 1, 2 => 255, _ => 65535 };
            for (var x = 0; x < 10; x++)
            {
                var maximum = pattern < 6 ? 256 : 65536;
                decoded[x + 2] = Vector256.Create((double)((x * 47 + pattern) % maximum) * alpha,
                    (double)((x * 31 + 83) % maximum) * alpha, (double)((x * 71 + 17) % maximum) * alpha, alpha);
            }
            var original = Enumerable.Repeat(double.NaN, 20).ToArray();
            for (var tap = 0; tap < 8; tap++)
            {
                original[tap] = Math.ScaleB((tap * 13 + pattern * 3) % 29 - 14, -5);
                original[10 + tap] = Math.ScaleB((tap * 17 + pattern * 7) % 31 - 15, -5);
            }
            if ((pattern & 1) != 0) original[0] = Math.BitIncrement(.5);
            if (pattern % 3 == 0) original[10] = BitConverter.Int64BitsToDouble(long.MinValue);
            var packed = original.ToArray(); var scratch = new double[16];
            var offsets = new byte[20]; offsets[9] = 1; int[] counts = [8, 8];
            ColumnBicubicResize.PreparePairedByteWeights(packed, offsets, counts, scratch);
            var sourceBytes = MemoryMarshal.AsBytes(decoded.AsSpan()).ToArray(); var saved = Bits(packed);
            var expected = BicubicMath.FilterOverlappingHorizontalPair(decoded.AsSpan(2, 10),
                original.AsSpan(0, 8), original.AsSpan(10, 8), 8, 2);
            _ = ColumnBicubicResize.FilterPreparedBytePair(decoded.AsSpan(2, 10), packed.AsSpan(0, 16));
            var allocated = GC.GetAllocatedBytesForCurrentThread();
            var actual = ColumnBicubicResize.FilterPreparedBytePair(decoded.AsSpan(2, 10), packed.AsSpan(0, 16));
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
            for (var lane = 0; lane < 8; lane++) Assert.Equal(expected.AsInt64().GetElement(lane), actual.AsInt64().GetElement(lane));
            Assert.Equal(sourceBytes, MemoryMarshal.AsBytes(decoded.AsSpan()).ToArray()); Assert.Equal(saved, Bits(packed));
        }
    }

    [Fact]
    public void RepairKeepsHalfRoundingPartialRefusalCancellationAndDeadScratch()
    {
        for (var layout = 0; layout < 3; layout++)
        for (var mode = 0; mode < 5; mode++)
        {
            using var source = ImageBuffer.Create(10, 1, 8);
            for (var x = 0; x < 10; x++)
            {
                var color = x == 0 ? 50 : x == 1 ? 51 : x == 2 ? 83 : x == 3 ? 84 : 23;
                var alpha = mode == 1 ? 0 : mode == 2 ? 1 : 255;
                if (mode == 3 && x < 2) alpha = x;
                if (mode == 4 && x is 2 or 3) alpha = x - 2;
                source.GetRow8(0).Slice(x * 4, 3).Fill((byte)color); source.GetRow8(0)[x * 4 + 3] = (byte)alpha;
            }
            var input = source.Pixels.ToArray();
            var weights = new double[20]; weights[0] = .5; weights[1] = .5; weights[10] = .75; weights[11] = .25;
            var original = weights.ToArray(); int[] counts = [layout == 2 ? 10 : 8, 8]; var offsets = new byte[20];
            for (var tap = 0; tap < counts[0]; tap++) offsets[tap] = (byte)tap;
            for (var tap = 0; tap < 8; tap++) offsets[10 + tap] = (byte)(tap + 2);
            if (layout == 1) offsets[9] = 1;
            ColumnBicubicResize.PreparePairedByteWeights(weights, offsets, counts, new double[16]);
            var savedWeights = Bits(weights); double[] vertical = [1];
            using var root = new TestMemoryBudget(0);
            using var context = new OperationContext(new() { MaxWorkingBytes = 1, MemoryBudget = root }, default);
            using var expected = ImageBuffer.Create(2, 1, 8); using var actual = ImageBuffer.Create(2, 1, 8);
            expected.Pixels.Fill(0xa5); actual.Pixels.Fill(0xa5); var complete = true;
            for (var lane = 0; lane < 2; lane++)
                if (!BicubicMath.TryWriteOriginalByte(source, expected, lane, 0,
                    original.AsSpan(lane * 10, counts[lane]), offsets.AsSpan(lane * 10, counts[lane]), 0, vertical, 0, context))
                { complete = false; break; }
            var scratch = Enumerable.Repeat(double.NaN, 12).ToArray(); var guards = Bits(scratch);
            Assert.Equal(complete, ColumnBicubicResize.TryRepairBytePair(source, actual, 0, 0,
                weights, offsets, counts, 0, vertical, 0, scratch.AsSpan(2, 8), context));
            Assert.Equal(expected.Pixels.ToArray(), actual.Pixels.ToArray());
            Assert.Equal(guards[..2], Bits(scratch.AsSpan(0, 2))); Assert.Equal(guards[10..], Bits(scratch.AsSpan(10, 2)));
            if (layout != 1) Assert.Equal(guards, Bits(scratch));
            if (mode is 0 or 2) { Assert.Equal(51, actual.GetRow8(0)[0]); Assert.Equal(83, actual.GetRow8(0)[4]); }
            var allocated = GC.GetAllocatedBytesForCurrentThread();
            var repeated = ColumnBicubicResize.TryRepairBytePair(source, actual, 0, 0,
                weights, offsets, counts, 0, vertical, 0, scratch.AsSpan(2, 8), context);
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated); Assert.Equal(complete, repeated);
            actual.Pixels.Fill(0xa5);
            using var cancellation = new CancellationTokenSource();
            using var canceled = new OperationContext(new() { MaxWorkingBytes = 1, MemoryBudget = root }, cancellation.Token);
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => ColumnBicubicResize.TryRepairBytePair(source, actual, 0, 0,
                weights, offsets, counts, 0, vertical, 0, scratch.AsSpan(2, 8), canceled));
            Assert.Equal(-1, actual.Pixels.IndexOfAnyExcept((byte)0xa5));
            Assert.Equal(complete, ColumnBicubicResize.TryRepairBytePair(source, actual, 0, 0,
                weights, offsets, counts, 0, vertical, 0, scratch.AsSpan(2, 8), context));
            Assert.Equal(expected.Pixels.ToArray(), actual.Pixels.ToArray()); Assert.Equal(input, source.Pixels.ToArray());
            Assert.Equal(savedWeights, Bits(weights)); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
        }
    }
}
