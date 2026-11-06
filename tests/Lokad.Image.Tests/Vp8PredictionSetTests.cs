using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8PredictionSetTests
{
    [Fact]
    public void CompleteSetMatchesIndependentGeometryWithoutAllocationOrTailWrites()
    {
        var random = new Random(638612); var top = new byte[8]; var left = new byte[4];
        var plane = new byte[96]; var actual = new byte[163]; var expected = new byte[163];
        int[] geometry = [0, 1, 2, 3, 5, 6, 4, 7, 8, 9];
        for (var sample = 0; sample < 128; sample++)
        {
            random.NextBytes(top); random.NextBytes(left); var corner = (byte)random.Next(256);
            if (sample < 2)
            {
                top.AsSpan().Fill(sample == 0 ? (byte)0 : (byte)255);
                left.AsSpan().Fill(sample == 0 ? (byte)255 : (byte)0);
                corner = sample == 0 ? (byte)255 : (byte)0;
            }
            top.CopyTo(plane, 4); plane[3] = corner;
            for (var y = 0; y < 4; y++) plane[(y + 1) * 16 + 3] = left[y];
            actual.AsSpan().Fill(73); expected.AsSpan().Fill(73);
            for (var mode = 0; mode < 10; mode++)
            {
                Vp8PredictionReference.Small(plane, 16, 4, 1, 0, 0, geometry[mode]);
                for (var y = 0; y < 4; y++) plane.AsSpan((y + 1) * 16 + 4, 4).CopyTo(expected.AsSpan(mode * 16 + y * 4));
            }
            var originalTop = top.ToArray(); var originalLeft = left.ToArray();
            Vp8Prediction.SmallCandidates(top, left, corner, actual);
            Assert.Equal(expected, actual); Assert.Equal(originalTop, top); Assert.Equal(originalLeft, left);
        }
        for (var i = 0; i < 128; i++) Vp8Prediction.SmallCandidates(top, left, 129, actual);
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++) Vp8Prediction.SmallCandidates(top, left, 129, actual);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
    }

    [Fact]
    public void CompleteSetSnapshotsBothBordersBeforeAnyAliasedWrite()
    {
        var original = Enumerable.Range(0, 256).Select(i => (byte)(i * 37 + i / 11)).ToArray();
        var separate = new byte[160];
        Vp8Prediction.SmallCandidates(original.AsSpan(1, 8), original.AsSpan(17, 4), 173, separate);
        foreach (var offset in new[] { 0, 3, 8, 17, 40 })
        {
            var actual = original.ToArray(); var expected = original.ToArray(); separate.CopyTo(expected, offset);
            Vp8Prediction.SmallCandidates(actual.AsSpan(1, 8), actual.AsSpan(17, 4), 173, actual.AsSpan(offset, 163));
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void IncompleteBordersOrOutputFailBeforeAnyWrite()
    {
        var top = new byte[8]; var left = new byte[4]; var output = Enumerable.Repeat((byte)73, 163).ToArray();
        Assert.Throws<ArgumentException>(() => Vp8Prediction.SmallCandidates(top.AsSpan(0, 7), left, 0, output));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.SmallCandidates(top, left.AsSpan(0, 3), 0, output));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.SmallCandidates(top, left, 0, output.AsSpan(0, 159)));
        Assert.All(output, v => Assert.Equal((byte)73, v));
    }
}
