using System.Numerics;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegShortRefinementTests
{
    [Theory]
    [InlineData(1)] [InlineData(8192)]
    public void ShortMasksMatchOriginalBitTraversal(int increment)
    {
        using var cancellation = new CancellationTokenSource();
        foreach (var separated in new[] { false, true })
        for (var selection = 0; selection < 16; selection++)
        for (var prefix = 0; prefix < 8; prefix++)
        for (var seed = 0; seed < 256; seed++)
        {
            var mask = Mask(selection, separated);
            var bytes = new List<byte> { (byte)seed };
            if (seed == 255) bytes.Add(0);
            bytes.AddRange([255, 0, 0xA5]);
            var data = bytes.ToArray();
            var actual = Coefficients(increment); var expected = actual.ToArray();
            var cost = BitOperations.PopCount(mask) + (prefix == 0 ? 0 : 1) + 1;
            var context = new OperationContext(ImageLimits.Default, (seed & 1) == 0 ? default : cancellation.Token);
            var referenceContext = new OperationContext(ImageLimits.Default, default);
            var reader = new JpegEntropyReader(data, 0, context);
            var reference = new JpegEntropyReader(data, 0, referenceContext);
            var local = (seed & 2) != 0;
            {
                if (prefix != 0) Assert.Equal(reference.ReadBits(prefix), reader.ReadBits(prefix));
                for (var bits = mask; bits != 0; bits &= bits - 1)
                {
                    ref var value = ref expected[BitOperations.TrailingZeroCount(bits)];
                    if (reference.ReadBits(1) != 0 && (Math.Abs(value) & increment) == 0)
                        value += value > 0 ? increment : -increment;
                }
                JpegDecoder.RefineMaskedBits(ref reader, actual, mask, increment);
                Assert.Equal(reference.ReadBits(3), reader.ReadBits(3));
            }
            Assert.Equal(expected, actual); Assert.Equal(reference.Position, reader.Position);
        }
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void RefusedShortFieldsKeepScalarPartialUpdatesAndFailures(int count)
    {
        using var cancellation = new CancellationTokenSource();
        foreach (var cancelable in new[] { false, true })
        foreach (var local in new[] { false, true })
        foreach (var bytes in new byte[][] { [], [0xA5], [0xA5, 255], [0xA5, 255, 217], [0xA5, 255, 0] })
        {
            Assert.Equal(Run(false), Run(true));

            (Type? Error, string? Message, int Position, string Coefficients) Run(bool optimized)
            {
                var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
                var reader = new JpegEntropyReader(bytes, 0, context);
                var values = Coefficients(1); var mask = Mask((1 << count) - 1, true);
                Exception? failure = null;
                try
                {
                    if (bytes.Length != 0) reader.ReadBits(7); else reader.ReadBits(0);
                    if (optimized) JpegDecoder.RefineMaskedBits(ref reader, values, mask, 1);
                    else
                        for (var bits = mask; bits != 0; bits &= bits - 1)
                        {
                            ref var value = ref values[BitOperations.TrailingZeroCount(bits)];
                            if (reader.ReadBits(1) != 0 && (Math.Abs(value) & 1) == 0)
                                value += value > 0 ? 1 : -1;
                        }
                }
                catch (Exception error) when (error is ImageDataException or ImageLimitException) { failure = error; }
                return (failure?.GetType(), failure?.Message, reader.Position, string.Join(',', values));
            }
        }
    }

    private static ulong Mask(int selection, bool separated)
    {
        ulong mask = 0;
        int[] positions = separated ? [0, 7, 31, 63] : [0, 1, 2, 3];
        for (var i = 0; i < 4; i++) if ((selection & (1 << i)) != 0) mask |= 1UL << positions[i];
        return mask;
    }

    private static int[] Coefficients(int increment)
        => Enumerable.Range(0, 64).Select(i => (2 + i % 2) * increment * (i % 3 == 0 ? -1 : 1)).ToArray();
}
