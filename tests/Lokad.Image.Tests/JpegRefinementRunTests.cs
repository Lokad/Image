using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegRefinementRunTests
{
    [Theory]
    [InlineData(1)] [InlineData(4)] [InlineData(8192)]
    public void GroupMatchesScalarCorrectionAndZeroRunBoundaries(int increment)
    {
        foreach (var start in new[] { 0, 7, 24, 56 })
        foreach (var width in new[] { 1, 4, 8 })
        for (var zeroMask = 0; zeroMask < 256; zeroMask++)
        foreach (var initialRun in new[] { 0, 1, 3, 7, 16 })
        foreach (var inserting in new[] { false, true })
        foreach (var bits in new byte[] { 0, 0xA5, 255 })
        {
            if (!inserting && initialRun == 0) continue;
            var original = Enumerable.Repeat(97, 64).ToArray();
            for (var i = 0; i < 8; i++) original[start + i] = (zeroMask & (1 << i)) != 0 ? 0
                : (i % 3 == 0 ? 2 * increment : 3 * increment) * ((i & 1) == 0 ? 1 : -1);
            var actual = original.ToArray(); var expected = original.ToArray();
            byte[] data = bits == 255 ? [255, 0] : [bits];
            var context = new OperationContext(ImageLimits.Default, default);
            var referenceContext = new OperationContext(ImageLimits.Default, default);
            var reader = new JpegEntropyReader(data, 0, context);
            var reference = new JpegEntropyReader(data, 0, referenceContext);
            var position = start; var run = initialRun;
            var success = JpegDecoder.TryRefineRun(ref reader, actual, ref position, start + width - 1, ref run, inserting, increment, out var finished);
            if (!Avx2.IsSupported || !Bmi2.IsSupported)
            {
                Assert.False(success); Assert.False(finished); Assert.Equal(original, actual);
                Assert.Equal(start, position); Assert.Equal(initialRun, run); Assert.Equal(0, reader.Position);
continue;
            }
            Assert.True(success);
            var expectedPosition = start; var expectedRun = initialRun; var expectedFinished = false;
            while (expectedPosition < start + width)
            {
                ref var value = ref expected[expectedPosition];
                if (value != 0)
                {
                    if (reference.ReadBits(1) != 0 && (Math.Abs(value) & increment) == 0)
                        value += value > 0 ? increment : -increment;
                }
                else
                {
                    if (inserting && expectedRun == 0) { expectedFinished = true; break; }
                    expectedRun--;
                    if (expectedRun == 0 && !inserting) { expectedPosition++; expectedFinished = true; break; }
                }
                expectedPosition++;
            }
            Assert.Equal(expected, actual); Assert.Equal(expectedPosition, position);
            Assert.Equal(expectedRun, run); Assert.Equal(expectedFinished, finished);
            Assert.Equal(reference.Position, reader.Position);         }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)]  [InlineData(3)] [InlineData(4)]
    public void RefusalPreservesCoefficientsReaderAndTraversal(int reason)
    {
        byte[] data = reason switch { 0 => [], 1 => [255, 217], _ => [255, 0] };
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default,
            reason == 3 ? cancellation.Token : default);
        var reader = new JpegEntropyReader(data, 0, context);
        var original = Enumerable.Repeat(-6, 64).ToArray(); var block = original.ToArray();
        var position = reason == 4 ? 57 : 0; var run = 16;         Assert.False(JpegDecoder.TryRefineRun(ref reader, block, ref position, 63, ref run, false, 1, out var finished));
        Assert.False(finished); Assert.Equal(original, block); Assert.Equal(reason == 4 ? 57 : 0, position);
        Assert.Equal(16, run); Assert.Equal(0, reader.Position);     }
}
