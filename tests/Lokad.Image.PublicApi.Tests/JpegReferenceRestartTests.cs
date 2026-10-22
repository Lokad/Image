using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegReferenceRestartTests
{
    private static readonly JpegEncodeOptions Options = new()
    {
        BackgroundRgb = 0x204060
    };

    [Theory]
    [InlineData(JpegSampling.YCbCr444, 8, 2)]
    [InlineData(JpegSampling.YCbCr422, 8, 3)]
    [InlineData(JpegSampling.YCbCr420, 16, 3)]
    [InlineData(JpegSampling.Grayscale, 8, 3)]
    [InlineData(JpegSampling.Grayscale, 16, 1)]
    public async Task NonRowIntervalsPreserveCoefficientsAcrossOwnedAndBorrowedInputs(JpegSampling sampling, int depth, int interval)
    {
        // Reuse the existing18x19 edge shape. These intervals cross MCU rows or
        // divide the MCU count exactly, qualifying predictor reset and no final RST.
        using var image = OriginalPixels.Buffer(18, 19, depth);
        var source = OriginalPixels.Bytes(image);
        var options = Options with { Sampling = sampling, RestartInterval = interval };
        using var baseline = new MemoryStream(); ImageCodec.EncodeJpeg(image, baseline, options with { RestartInterval = 0 });
        using var encoded = new MemoryStream(); ImageCodec.EncodeJpeg(image, encoded, options);
        var packet = encoded.ToArray();
        using var first = ImageCodec.Decode(baseline.ToArray()); using var second = ImageCodec.Decode(packet);
        Assert.Equal(OriginalPixels.Bytes(first), OriginalPixels.Bytes(second));
        var mcus = sampling == JpegSampling.Grayscale ? 3 * 3 : (sampling == JpegSampling.YCbCr444 ? 3 : 2) * 2;
        var markers = Enumerable.Range(0, packet.Length - 1)
            .Where(i => packet[i] == 255 && packet[i + 1] is >= 208 and <= 215).ToArray();
        Assert.Equal((mcus - 1) / interval, markers.Length);
        for (var i = 0; i < markers.Length; i++) Assert.Equal(208 + i % 8, packet[markers[i] + 1]);

        var normalized = JpegQualityCorpus.Reference(source, depth, 0x204060);
        const int stride = 18 * 3 + 5;
        var rgb = Enumerable.Repeat((byte)165, stride * 19 + 7).ToArray();
        for (var y = 0; y < 19; y++)
        for (var x = 0; x < 18; x++) normalized.AsSpan((y * 18 + x) * 4, 3).CopyTo(rgb.AsSpan(y * stride + x * 3, 3));
        var before = rgb.ToArray();
        using var samples = new MemoryStream(); ImageCodec.EncodeJpeg(rgb, 18, 19, JpegSampleFormat.Rgb, samples, stride, options);
        using var asynchronous = new MemoryStream(); await ImageCodec.EncodeJpegAsync(rgb, 18, 19, JpegSampleFormat.Rgb, asynchronous, stride, options);
        Assert.Equal(packet, samples.ToArray()); Assert.Equal(packet, asynchronous.ToArray());
        Assert.Equal(before, rgb); Assert.Equal(source, OriginalPixels.Bytes(image));
    }

    [Theory]
    [InlineData(1)] [InlineData(2)]
    public void FixedTableRefusalIsTerminalBeforeWritingHeaders(long growth)
    {
        using var image = OriginalPixels.Buffer(18, 19, 8);
        var before = OriginalPixels.Bytes(image);
        using var root = new TestMemoryBudget(1024 * 1024) { RefuseAtGrowth = growth };
        var options = Options with { RestartInterval = 3, Limits = ImageLimits.Default with { MemoryBudget = root } };
        using var output = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, output, options));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length); Assert.Equal(before, OriginalPixels.Bytes(image));
        root.RefuseAtGrowth = 0; ImageCodec.EncodeJpeg(image, output, options);
        Assert.True(output.Length > 0); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)] [InlineData(4)]
    public void GrayscaleTableRefusalIsTerminalBeforeWritingHeaders(long growth)
    {
        using var image = OriginalPixels.Buffer(18, 19, 8);
        using var root = new TestMemoryBudget(1024 * 1024) { RefuseAtGrowth = growth };
        var options = Options with { Sampling = JpegSampling.Grayscale, RestartInterval = 3,
            Limits = ImageLimits.Default with { MemoryBudget = root } };
        using var output = new MemoryStream();
        var before = OriginalPixels.Bytes(image);
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, output, options));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length); Assert.Equal(before, OriginalPixels.Bytes(image));
        root.RefuseAtGrowth = 0; ImageCodec.EncodeJpeg(image, output, options);
        Assert.True(output.Length > 0); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
