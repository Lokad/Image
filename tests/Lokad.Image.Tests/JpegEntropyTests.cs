using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegEntropyTests
{
    [Fact]
    public void MixedBitWidthsUnstuffCompleteBytesAndStopBeforeRestart()
    {
        // Original T.81 stream: eight payload bytes, three stuffed FF bytes,
        // then RST0. Explicit bit text is independent of buffer/refill mechanics.
        const string expected = "1000000011111111000000011111111000000111010101011111111111111111";
        var data = new byte[] { 0x80, 0xFF, 0, 0x01, 0xFE, 0x07, 0x55, 0xFF, 0, 0xFF, 0, 0xFF, 0xD0 };
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 11 }, default);
        var reader = new JpegEntropyReader(data, 0, context); var consumed = 0;
        foreach (var count in new[] { 0, 1, 7, 16, 3, 5, 8, 16, 1, 7, 0 })
        {
            var value = count == 0 ? 0 : Convert.ToInt32(expected.Substring(consumed, count), 2);
            Assert.Equal(value, reader.ReadBits(count)); consumed += count;
        }
        Assert.Equal(64, consumed); Assert.Equal(11, reader.Position);
        reader.Restart(0); Assert.Equal(data.Length, reader.Position);
        Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void CachedBitsDoNotReadAheadIntoAMarkerOrChangeFailedReadAccounting()
    {
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 3 }, default);
        var reader = new JpegEntropyReader(new byte[] { 0xAA, 0xFF, 0xD0 }, 0, context);
        Assert.Equal(10, reader.ReadBits(4)); Assert.Equal(1, reader.Position);
        Assert.Equal(10, reader.ReadBits(4)); Assert.Equal(1, reader.Position);
        try { reader.ReadBits(1); Assert.Fail("A marker cannot supply entropy bits."); }
        catch (ImageDataException) { Assert.Equal(3, reader.Position); }
        Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void ZeroBitReadStillChargesWorkAndObservesCancellation()
    {
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 1 }, default);
        var reader = new JpegEntropyReader([], 0, context);
        Assert.Equal(0, reader.ReadBits(0)); Assert.Equal(0, reader.Position);
        Assert.Throws<ImageLimitException>(() => context.Work(1));
        using var cancellation = new CancellationTokenSource();
        var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
        {
            var value = new JpegEntropyReader([], 0, canceled); value.ReadBits(0);
        });
    }

    [Theory]
    [InlineData(-1)] [InlineData(17)]
    public void InvalidWidthsRejectBeforeWorkAdmission(int count)
    {
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 1 }, default);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var reader = new JpegEntropyReader([], 0, context); reader.ReadBits(count);
        });
        context.Work(1); Assert.Throws<ImageLimitException>(() => context.Work(1));
    }
}
