using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegProgressivePlaneOwnershipTests
{
    // Tiny output, tail collision, partial blocks and a multi-strip output. Caller
    // RGB uses the separate complete-plane renderer; frozen native fixtures also
    // exercise nonconstant inverse/color results in the existing exact suite.
    [Theory]
    [InlineData(1, 1, false)]
    [InlineData(17, 11, true)]
    [InlineData(31, 23, false)]
    [InlineData(513, 259, false)]
    [InlineData(513, 259, true)]
    public void IndependentRgbaAndCallerRgbAgreeAcrossStripAndBlockTails(int width, int height, bool rgb)
    {
        var data = Input(width, height, rgb); var original = data.ToArray();
        var caller = new byte[width * height * 3];
        ImageCodec.DecodeJpeg(data, caller, JpegSampleFormat.Rgb);
        using var cancellation = new CancellationTokenSource();
        using var image = ImageCodec.Decode(data, cancellationToken: cancellation.Token);
        var pixels = OriginalPixels.Bytes(image);
        for (var i = 0; i < width * height; i++)
        {
            Assert.Equal(caller.AsSpan(i * 3, 3).ToArray(), pixels.AsSpan(i * 4, 3).ToArray());
            Assert.Equal(255, pixels[i * 4 + 3]);
        }
        Assert.Equal(original, data);
    }

    [Fact]
    public async Task SharedPeakAndSurvivingOutputsRemainBoundedAcrossStreamLifetimes()
    {
        const int width = 513, height = 259, frameBytes = width * height * 4;
        const long coefficientAndSamplePeak = 13L * 520 * 264;
        var data = Input(width, height, false);
        using var root = new TestMemoryBudget(coefficientAndSamplePeak + 65536 + frameBytes);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var first = ImageCodec.Decode(data, options);
        var expected = OriginalPixels.Bytes(first);
        using var second = ImageCodec.Decode(data, options);
        Assert.Equal(2L * frameBytes, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(2L * frameBytes, root.Bytes);
        first.Dispose();
        using var recovered = ImageCodec.Decode(data, options);
        Assert.Equal(expected, OriginalPixels.Bytes(second)); Assert.Equal(expected, OriginalPixels.Bytes(recovered));
        recovered.Dispose(); second.Dispose();
        // Stream buffering is a separately charged boundary. With both earlier
        // results released, the same root has room for input capacity and decode.
        using var stream = new MemoryStream(data, writable: false);
        using var cancellation = new CancellationTokenSource();
        using var asynchronous = await ImageCodec.DecodeAsync(stream, options, cancellation.Token);
        Assert.True(stream.CanRead); Assert.Equal(frameBytes, root.Bytes);
        data.AsSpan().Clear(); Assert.Equal(expected, OriginalPixels.Bytes(asynchronous));
        asynchronous.Dispose();
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.InRange(root.Peak, coefficientAndSamplePeak, coefficientAndSamplePeak + 65536 + frameBytes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OutputAdmissionRefusalOrCancellationReleasesAllTemporaryAndOwnedStorage(bool cancel)
    {
        var data = Input(513, 259, false); var original = data.ToArray();
        long outputGrowth;
        using (var probe = new TestMemoryBudget(4 * 1024 * 1024))
        using (var live = new CancellationTokenSource())
        {
            using (var image = ImageCodec.Decode(data, new() { Limits = new() { MemoryBudget = probe } }, live.Token))
            {
                Assert.Equal(513L * 259 * 4, probe.Bytes);
                outputGrowth = probe.GrowthAttempts;
            }
            Assert.Equal(0, probe.Bytes);
            Assert.InRange(probe.Peak, 13L * 520 * 264, 13L * 520 * 264 + 65536);
        }
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        using var cancellation = new CancellationTokenSource();
        if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == outputGrowth) cancellation.Cancel(); };
        else root.RefuseAtGrowth = outputGrowth;
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        if (cancel)
            Assert.Equal(cancellation.Token, Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(data, options, cancellation.Token)).CancellationToken);
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, options, cancellation.Token));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(1, root.Refusals); Assert.Equal(outputGrowth, root.GrowthAttempts);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(original, data);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        using (var image = ImageCodec.Decode(data, options)) Assert.Equal(513L * 259 * 4, root.Bytes);
        Assert.Equal(0, root.Bytes);
    }

    private static byte[] Input(int width, int height, bool rgb)
    {
        var data = AuthoredJpeg.ProgressiveFrame(width, height, JpegSampling.YCbCr444, restartInterval: 3);
        if (!rgb) return data;
        byte[] adobe = [255, 238, 0, 14, 65, 100, 111, 98, 101, 0, 100, 0, 0, 0, 0, 0];
        return data[..^2].Concat(adobe).Concat(new byte[] { 255, 217 }).ToArray();
    }
}
