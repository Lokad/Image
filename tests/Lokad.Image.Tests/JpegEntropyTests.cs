using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegEntropyTests
{
    [Fact]
    public void EverySignedAmplitudeKeepsValueStuffingPositionPadding()
    {
        // Original T.81 F.2.2.1 payloads: enumerate signed values in each
        // category, encode negative values by complementing their magnitude,
        // and append an independently constructed RST0 marker.
        var data = new byte[6];
        for (var width = 1; width <= 16; width++)
        {
            var minimum = 1 << (width - 1); var maximum = (1 << width) - 1;
            var context = new OperationContext(ImageLimits.Default, default);
            for (var magnitude = minimum; magnitude <= maximum; magnitude++)
            foreach (var sign in new[] { -1, 1 })
            {
                var expected = sign * magnitude;
                var code = sign > 0 ? magnitude : maximum - magnitude;
                var byteCount = (width + 7) / 8; var padding = byteCount * 8 - width;
                var padded = (code << padding) | ((1 << padding) - 1); var length = 0;
                for (var index = byteCount - 1; index >= 0; index--)
                {
                    var next = (byte)(padded >> (index * 8)); data[length++] = next;
                    if (next == 255) data[length++] = 0;
                }
                var marker = length; data[length++] = 255; data[length++] = 0xD0;
                var reader = new JpegEntropyReader(data.AsSpan(0, length), 0, context);
                Assert.Equal(expected, reader.ReadSigned(width)); Assert.Equal(marker, reader.Position);
                reader.Restart(0); Assert.Equal(length, reader.Position);
            }
        }
    }

    [Fact]
    public void MixedBitWidthsUnstuffCompleteBytesAndStopBeforeRestart()
    {
        // Original T.81 stream: eight payload bytes, three stuffed FF bytes,
        // then RST0. Explicit bit text is independent of buffer/refill mechanics.
        const string expected = "1000000011111111000000011111111000000111010101011111111111111111";
        var data = new byte[] { 0x80, 0xFF, 0, 0x01, 0xFE, 0x07, 0x55, 0xFF, 0, 0xFF, 0, 0xFF, 0xD0 };
        var context = new OperationContext(ImageLimits.Default, default);
        var reader = new JpegEntropyReader(data, 0, context); var consumed = 0;
        foreach (var count in new[] { 0, 1, 7, 16, 3, 5, 8, 16, 1, 7, 0 })
        {
            var value = count == 0 ? 0 : Convert.ToInt32(expected.Substring(consumed, count), 2);
            Assert.Equal(value, reader.ReadBits(count)); consumed += count;
        }
        Assert.Equal(64, consumed); Assert.Equal(11, reader.Position);
        reader.Restart(0); Assert.Equal(data.Length, reader.Position);
    }

    [Fact]
    public void CachedBitsDoNotReadAheadIntoAMarkerOrChangeFailedReadAccounting()
    {
        var context = new OperationContext(ImageLimits.Default, default);
        var reader = new JpegEntropyReader(new byte[] { 0xAA, 0xFF, 0xD0 }, 0, context);
        Assert.Equal(10, reader.ReadBits(4)); Assert.Equal(1, reader.Position);
        Assert.Equal(10, reader.ReadBits(4)); Assert.Equal(1, reader.Position);
        try { reader.ReadBits(1); Assert.Fail("A marker cannot supply entropy bits."); }
        catch (ImageDataException) { Assert.Equal(3, reader.Position); }
    }

    [Fact]
    public void ZeroBitReadStillChargesAndObservesCancellation()
    {
        var context = new OperationContext(ImageLimits.Default, default);
        var reader = new JpegEntropyReader([], 0, context);
        Assert.Equal(0, reader.ReadBits(0)); Assert.Equal(0, reader.Position);
        using var cancellation = new CancellationTokenSource();
        var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
        {
            var value = new JpegEntropyReader([], 0, canceled); value.ReadBits(0);
        });
    }

    [Theory]
    [InlineData(-1)] [InlineData(17)]
    public void InvalidWidthsRejectBeforeAdmission(int count)
    {
        var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var reader = new JpegEntropyReader([], 0, context); reader.ReadBits(count);
        });
    }
}
