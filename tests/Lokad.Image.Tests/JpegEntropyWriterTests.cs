using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegEntropyWriterTests
{
    [Fact]
    public void EverySignedAmplitudeKeepsCanonicalBytesStuffingPaddingRestartAndWork()
    {
        var expected = new byte[6];
        for (var width = 1; width <= 16; width++)
        {
            var minimum = 1 << (width - 1); var maximum = (1 << width) - 1;
            for (var magnitude = minimum; magnitude <= maximum; magnitude++)
            for (var sign = -1; sign <= 1; sign += 2)
            {
                // Independent T.81 payload from magnitude/complement, padded
                // with ones and byte-stuffed before an original RST0 marker.
                var code = sign > 0 ? magnitude : maximum - magnitude;
                var byteCount = (width + 7) / 8; var padding = byteCount * 8 - width;
                var padded = (code << padding) | ((1 << padding) - 1); var length = 0;
                for (var index = byteCount - 1; index >= 0; index--)
                {
                    var next = (byte)(padded >> (index * 8)); expected[length++] = next;
                    if (next == 255) expected[length++] = 0;
                }
                expected[length++] = 255; expected[length++] = 0xD0;
                using var output = new MemoryStream();
                using var context = Context(padding == 0 ? 1 : 2);
                using var writer = new JpegEntropyWriter(output, context);
                writer.Signed(sign * magnitude, width); writer.Restart(0);
                Assert.True(expected.AsSpan(0, length).SequenceEqual(output.GetBuffer().AsSpan(0, (int)output.Length)));
                Assert.Throws<ImageLimitException>(() => context.Work(1));
                Assert.True(output.CanWrite);
            }
        }
    }

    [Fact]
    public void ZeroWidthStillAdmitsWorkAndObservesCancellation()
    {
        using var output = new MemoryStream(); using var context = Context(1);
        using var writer = new JpegEntropyWriter(output, context); writer.Signed(0, 0);
        Assert.Equal(0, output.Length); Assert.Throws<ImageLimitException>(() => context.Work(1));
        using var cancellation = new CancellationTokenSource();
        using var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        using var stopped = new JpegEntropyWriter(output, canceled);
        Assert.Throws<OperationCanceledException>(() => stopped.Signed(0, 0));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void InvalidWidthsAndValuesRejectBeforeWorkOrOutput()
    {
        foreach (var width in new[] { int.MinValue, -1, 17, 31, 32, int.MaxValue })
        foreach (var value in new[] { int.MinValue, -1, 0, 1, int.MaxValue }) Check(value, width);
        for (var width = 0; width <= 16; width++)
        {
            Check(int.MinValue, width); Check(-(1 << width), width);
            Check(1 << width, width); Check(int.MaxValue, width);
        }
        static void Check(int value, int width)
        {
            using var output = new MemoryStream(); using var context = Context(1);
            using var writer = new JpegEntropyWriter(output, context);
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => writer.Signed(value, width));
            Assert.Equal("value", error.ParamName); Assert.Equal(0, output.Length);
            context.Work(1); Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    private static OperationContext Context(long work) => new(ImageLimits.Default with { MaxWork = work }, default);
}
