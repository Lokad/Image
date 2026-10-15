using System.Numerics;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegMaskedRefinementTests
{
    [Theory]
    [InlineData(1)] [InlineData(8192)]
    public void MaskedCorrectionMatchesOriginalBitTraversal(int increment)
    {
        using var cancellation = new CancellationTokenSource();
        for (var seed = 0; seed < 65536; seed++)
        {
            var mask = (ulong)(uint)seed | ((ulong)(uint)(seed ^ 0xA5C3) << 16)
                | ((ulong)(uint)((seed * 40503) & 65535) << 32) | ((ulong)(uint)(seed ^ 0xFFFF) << 48);
            var original = new int[64];
            for (var i = 0; i < 64; i++) if ((mask & (1UL << i)) != 0)
                original[i] = (i % 3 == 0 ? 2 * increment : 3 * increment) * ((i & 1) == 0 ? 1 : -1);
            var data = Payload(seed); var actual = original.ToArray(); var expected = original.ToArray();
            var cost = BitOperations.PopCount(mask);
            var context = new OperationContext(ImageLimits.Default with { MaxWork = cost }, (seed & 1) == 0 ? default : cancellation.Token);
            var referenceContext = new OperationContext(ImageLimits.Default with { MaxWork = cost }, default);
            var reader = new JpegEntropyReader(data, 0, context); var reference = new JpegEntropyReader(data, 0, referenceContext);
            for (var i = 0; i < 64; i++) if ((mask & (1UL << i)) != 0)
            {
                ref var value = ref expected[i];
                if (reference.ReadBits(1) != 0 && (Math.Abs(value) & increment) == 0) value += value > 0 ? increment : -increment;
            }
            reader.BeginLocalWork();
            try { JpegDecoder.RefineMaskedBits(ref reader, actual, mask, increment); }
            finally { reader.EndLocalWork(); }
            Assert.Equal(expected, actual); Assert.Equal(reference.Position, reader.Position);
            Assert.Equal(referenceContext.RemainingWork, context.RemainingWork);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void FailedOrCancellableBatchPreservesOriginalPartialUpdates(int reason)
    {
        foreach (var local in new[] { false, true })
        {
            byte[] data = reason switch { 0 => [], 1 => [0xA5, 255, 217], _ => [0xA5, 0xC3, 0xFF, 0] };
            using var cancellation = new CancellationTokenSource();
            var token = reason == 3 ? cancellation.Token : default;
            var limit = reason == 2 ? 9 : 100;
            var a = Enumerable.Repeat(-2, 64).ToArray(); var b = a.ToArray();
            var actualContext = new OperationContext(ImageLimits.Default with { MaxWork = limit }, token);
            var referenceContext = new OperationContext(ImageLimits.Default with { MaxWork = limit }, token);
            var reader = new JpegEntropyReader(data, 0, actualContext); var reference = new JpegEntropyReader(data, 0, referenceContext);
            Exception? expectedError = null, actualError = null;
            try { for (var i = 0; i < 32; i++) if (reference.ReadBits(1) != 0) b[i]--; } catch (Exception error) { expectedError = error; }
            if (local) reader.BeginLocalWork();
            try { JpegDecoder.RefineMaskedBits(ref reader, a, 0xFFFF_FFFFUL, 1); } catch (Exception error) { actualError = error; }
            finally { if (local) reader.EndLocalWork(); }
            Assert.Equal(expectedError?.GetType(), actualError?.GetType()); Assert.Equal(expectedError?.Message, actualError?.Message);
            Assert.Equal(b, a); Assert.Equal(reference.Position, reader.Position); Assert.Equal(referenceContext.RemainingWork, actualContext.RemainingWork);
        }
    }

    private static byte[] Payload(int seed)
    {
        var bytes = new List<byte>();
        for (var i = 0; i < 8; i++) { var value = (byte)(seed * 37 + i * 73); bytes.Add(value); if (value == 255) bytes.Add(0); }
        return bytes.ToArray();
    }
}
