using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegEntropyWriterTests
{
    [Fact]
    public void EverySignedAmplitudeKeepsCanonicalBytesStuffingPaddingRestart()
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
                using var context = Context();
                using var writer = new JpegEntropyWriter(output, context);
                writer.Signed(sign * magnitude, width); writer.Restart(0);
                Assert.True(expected.AsSpan(0, length).SequenceEqual(output.GetBuffer().AsSpan(0, (int)output.Length)));
                Assert.True(output.CanWrite);
            }
        }
    }

    [Fact]
    public void ZeroWidthStillAdmitsAndObservesCancellation()
    {
        using var output = new MemoryStream(); using var context = Context();
        using var writer = new JpegEntropyWriter(output, context); writer.Signed(0, 0);
        Assert.Equal(0, output.Length);         using var cancellation = new CancellationTokenSource();
        using var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        using var stopped = new JpegEntropyWriter(output, canceled);
        Assert.Throws<OperationCanceledException>(() => stopped.Signed(0, 0));
        Assert.Equal(0, output.Length);
    }

    [Fact]
    public void InvalidWidthsAndValuesRejectBeforeOrOutput()
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
            using var output = new MemoryStream(); using var context = Context();
            using var writer = new JpegEntropyWriter(output, context);
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => writer.Signed(value, width));
            Assert.Equal("value", error.ParamName); Assert.Equal(0, output.Length);
        }
    }

    [Fact]
    public void PairedGroupsMatchIndependentBitsAcrossWidthsTailsFlushesAndRestarts()
    {
        for (var tail = 0; tail < 8; tail++)
        {
            using var output = new MemoryStream(); using var context = Context();
            using var writer = new JpegEntropyWriter(output, context);
            var expected = new List<byte>(); var pending = 0; var available = 0;
            void Bits(int value, int count)
            {
                for (var bit = count - 1; bit >= 0; bit--)
                {
                    pending = pending * 2 + ((value >> bit) & 1);
                    if (++available != 8) continue;
                    expected.Add((byte)pending); if (pending == 255) expected.Add(0);
                    pending = 0; available = 0;
                }
            }
            void Pad() { if (available != 0) Bits((1 << (8 - available)) - 1, 8 - available); }
            var ordinal = 0;
            for (var firstCount = 0; firstCount <= 16; firstCount++)
            for (var secondCount = 0; secondCount <= 16; secondCount++)
            for (var pattern = 0; pattern < 4; pattern++)
            {
                // Restore every possible pending tail before every pair, including
                // 16+16 ones with seven live bits; a 32-bit accumulator loses data.
                Pad(); writer.Align();
                var prefix = (1 << tail) - 1; writer.Write(prefix, tail); Bits(prefix, tail);
                var first = (pattern switch { 0 => 0, 1 => 65535, 2 => 0xaaaa, _ => ordinal * 271 + 13 }) & ((1 << firstCount) - 1);
                var second = (pattern switch { 0 => 0, 1 => 65535, 2 => 0x5555, _ => ordinal * 419 + 31 }) & ((1 << secondCount) - 1);
                writer.WritePair(first, firstCount, second, secondCount); Bits(first, firstCount); Bits(second, secondCount);
                for (var repeat = 0; repeat < 7; repeat++)
                {
                    writer.WritePair(first, firstCount, second, secondCount); Bits(first, firstCount); Bits(second, secondCount);
                }
                if (++ordinal % 23 == 0)
                {
                    Pad(); expected.Add(255); expected.Add((byte)(0xd0 + (ordinal & 7))); writer.Restart(ordinal);
                }
            }
            // Continue without alignment across both 256- and 512-byte flushes,
            // including stuffed 0xff bytes on either side of a chunk boundary.
            for (var i = 0; i < 1024; i++)
            {
                writer.WritePair(65535, 16, i, 16); Bits(65535, 16); Bits(i, 16);
            }
            Pad(); writer.Align();
            Assert.Equal(expected, output.ToArray()); Assert.True(output.CanWrite);
        }
    }

    [Fact]
    public void InvalidPairsLeavePendingBitsAndAdmissionUntouched()
    {
        using var root = new Lokad.Image.Tooling.TestMemoryBudget(JpegEntropyWriter.StorageBytes);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var output = new MemoryStream(); using var writer = new JpegEntropyWriter(output, context);
        writer.Write(5, 3);
        foreach (var count in new[] { int.MinValue, -1, 17, 32, int.MaxValue })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.WritePair(0, count, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.WritePair(0, 0, 0, count));
        }
        for (var count = 0; count <= 16; count++)
        foreach (var value in new[] { -1, 1 << count, int.MaxValue })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.WritePair(value, count, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.WritePair(0, 0, value, count));
        }
        Assert.Equal(0, root.Attempts); Assert.Equal(0, output.Length);
        writer.WritePair(1, 1, 0, 4); writer.Align(); Assert.Equal(new byte[] { 0xb0 }, output.ToArray());
    }

    [Fact]
    public void EvenZeroWidthPairsObserveCancellationAndDisposalWithoutAdmission()
    {
        using var root = new Lokad.Image.Tooling.TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var output = new MemoryStream(); using var writer = new JpegEntropyWriter(output, context);
        writer.WritePair(0, 0, 0, 0); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => writer.WritePair(0, 0, 0, 0));
        writer.Dispose(); Assert.Throws<ObjectDisposedException>(() => writer.WritePair(0, 0, 0, 0));
        Assert.Equal(0, root.Attempts); Assert.Equal(0, output.Length); Assert.True(output.CanWrite);
    }

    private static OperationContext Context() => new(ImageLimits.Default, default);
}
