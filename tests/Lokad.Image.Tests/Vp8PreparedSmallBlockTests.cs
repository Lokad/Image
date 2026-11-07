using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8PreparedSmallBlockTests
{
    [Fact]
    public void OneBorderSnapshotServesAllModesAfterBorrowedInputsChange()
    {
        var random = new Random(6386474); var top = new byte[8]; var left = new byte[4];
        var plane = new byte[96]; var expected = new byte[16]; var actual = new byte[18];
        Span<int> prepared = stackalloc int[15];
        int[] geometry = [0, 1, 2, 3, 5, 6, 4, 7, 8, 9];
        for (var sample = 0; sample < 128; sample++)
        {
            random.NextBytes(top); random.NextBytes(left); var corner = (byte)random.Next(256);
            if (sample < 2) { top.AsSpan().Fill((byte)(sample * 255)); left.AsSpan().Fill((byte)(255 - sample * 255)); }
            top.CopyTo(plane.AsSpan(4)); plane[3] = corner;
            for (var y = 0; y < 4; y++) plane[(y + 1) * 16 + 3] = left[y];
            prepared.Fill(-12345);
            var dc = Vp8Prediction.PrepareSmallBlock(top, left, corner, prepared);
            top.AsSpan().Fill(73); left.AsSpan().Fill(91);
            for (var mode = 0; mode < 10; mode++)
            {
                Vp8PredictionReference.Small(plane, 16, 4, 1, 0, 0, geometry[mode]);
                for (var y = 0; y < 4; y++) plane.AsSpan((y + 1) * 16 + 4, 4).CopyTo(expected.AsSpan(y * 4));
                actual.AsSpan().Fill(0xa5);
                Vp8Prediction.SmallBlock((Vp8SmallIntraMode)mode, prepared, dc, actual);
                Assert.True(expected.AsSpan().SequenceEqual(actual.AsSpan(0, 16)));
                Assert.Equal((byte)0xa5, actual[16]); Assert.Equal((byte)0xa5, actual[17]);
                Assert.Equal(-12345, prepared[13]); Assert.Equal(-12345, prepared[14]);
            }
        }
        // The prepared view is stack-owned and does not create per-mode objects.
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++) Vp8Prediction.SmallBlock((Vp8SmallIntraMode)(i % 10), prepared, 128, actual);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void PreparationRejectsIncompleteOrAliasedScratchBeforeWriting()
    {
        var top = new byte[8]; var left = new byte[4]; var scratch = Enumerable.Repeat(-12345, 15).ToArray();
        Assert.Throws<ArgumentException>(() => Vp8Prediction.PrepareSmallBlock(top.AsSpan(0, 7), left, 0, scratch));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.PrepareSmallBlock(top, left.AsSpan(0, 3), 0, scratch));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.PrepareSmallBlock(top, left, 0, scratch.AsSpan(0, 12)));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.PrepareSmallBlock(MemoryMarshal.AsBytes(scratch.AsSpan()).Slice(2, 8), left, 0, scratch));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.PrepareSmallBlock(top, MemoryMarshal.AsBytes(scratch.AsSpan()).Slice(20, 4), 0, scratch));
        Assert.All(scratch, n => Assert.Equal(-12345, n));
    }

    [Fact]
    public void InvalidPreparedModeOrExtentPreservesDestination()
    {
        var neighbors = new int[13]; var output = Enumerable.Repeat((byte)73, 18).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8Prediction.SmallBlock((Vp8SmallIntraMode)255, neighbors, 128, output));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.SmallBlock(Vp8SmallIntraMode.Dc, neighbors.AsSpan(0, 12), 128, output));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.SmallBlock(Vp8SmallIntraMode.Dc, neighbors, 128, output.AsSpan(0, 15)));
        Assert.All(output, n => Assert.Equal((byte)73, n));
    }
}
