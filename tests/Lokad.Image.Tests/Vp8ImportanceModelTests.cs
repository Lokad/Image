using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8ImportanceModelTests
{
    [Fact]
    public void ImportanceMatchesGaussianAndCheckedCopiesKeepIndependentTailsWithoutAllocation()
    {
        var expected = new ushort[16];
        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++) expected[y * 4 + x] = (ushort)Math.Floor(4 + 26 * Math.Exp(-(x * x + y * y) / 7d) + .5);
        var output = new ushort[18]; Array.Fill(output, (ushort)4567);
        Vp8TrellisQuantization.Weights(output.AsSpan(1));
        Assert.Equal(expected, output[1..17]); Assert.Equal((ushort)4567, output[0]); Assert.Equal((ushort)4567, output[17]);
        output[1] ^= 255;
        Vp8TrellisQuantization.Weights(output.AsSpan(1)); Assert.Equal(expected, output[1..17]);
        for (var count = 0; count < 16; count++)
        {
            var shortOutput = Enumerable.Repeat((ushort)1234, count).ToArray();
            Assert.Throws<ArgumentException>(() => Vp8TrellisQuantization.Weights(shortOutput));
            Assert.All(shortOutput, value => Assert.Equal((ushort)1234, value));
        }
        for (var i = 0; i < 1000; i++) Vp8TrellisQuantization.Weights(output);
        var start = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10000; i++) Vp8TrellisQuantization.Weights(output);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - start);
    }
}
