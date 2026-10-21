using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCompactColorTests
{
    [Theory]
    [InlineData(1, 800)] [InlineData(50, 16)] [InlineData(69, 10)] [InlineData(100, 0)]
    public void ExplicitColorPolicyHasBoundedTablesAndPreservesOriginalDefault(int quality, int floor)
    {
        using var image = OriginalPixels.Buffer(17, 11, 8, opaque: true);
        var options = new JpegEncodeOptions { Quality = quality };
        var original = Encode(image, options);
        Assert.Equal(JpegQuantizationPolicy.Original, options.QuantizationPolicy);
        Assert.Equal(original, Encode(image, options with { QuantizationPolicy = JpegQuantizationPolicy.Original }));
        var compact = Encode(image, options with { QuantizationPolicy = JpegQuantizationPolicy.CompactColor });
        var first = JpegQualitySyntax.Read(original); var second = JpegQualitySyntax.Read(compact);
        Assert.Equal(first.Sampling, second.Sampling); Assert.Equal(2, second.QuantizationTables.Length);
        for (var table = 0; table < 2; table++)
        {
            Assert.Equal(first.QuantizationTables[table][0], second.QuantizationTables[table][0]);
            for (var i = 1; i <= 64; i++)
                Assert.Equal(Math.Min(255, first.QuantizationTables[table][i] + floor), second.QuantizationTables[table][i]);
        }
        if (quality == 100) Assert.Equal(original, compact);
        if (quality == 1)
            Assert.All(second.QuantizationTables.SelectMany(table => table.Skip(1)), value => Assert.Equal(255, value));
    }

    [Theory]
    [InlineData(1)] [InlineData(69)] [InlineData(100)]
    public void BothGrayscaleBoundariesKeepOriginalPackets(int quality)
    {
        using var image = OriginalPixels.Buffer(129, 129, 8, grayscale: true, opaque: true);
        var options = new JpegEncodeOptions { Quality = quality, Sampling = JpegSampling.Grayscale, BackgroundRgb = 0 };
        var gray = new byte[image.Width * image.Height];
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++) gray[y * image.Width + x] = image.GetRow8(y)[x * 4];
        var original = Encode(image, options);
        Assert.Equal(original, Encode(image, options with { QuantizationPolicy = JpegQuantizationPolicy.CompactColor }));
        using var output = new MemoryStream();
        ImageCodec.EncodeJpeg(gray, image.Width, image.Height, JpegSampleFormat.Grayscale, output,
            options: options with { Sampling = JpegSampling.YCbCr420, QuantizationPolicy = JpegQuantizationPolicy.CompactColor });
        Assert.Equal(original, output.ToArray());
    }

    [Theory]
    [InlineData(17, 8, JpegSampling.YCbCr444)]
    [InlineData(17, 16, JpegSampling.YCbCr422)]
    [InlineData(129, 8, JpegSampling.YCbCr420)]
    public async Task CallerSamplesImageDepthsAndAsyncKeepTheSameReconstruction(int side, int depth, JpegSampling sampling)
    {
        using var source = OriginalPixels.Buffer(side, side - 2, 8, opaque: true);
        using var image = ImageBuffer.Create(source.Width, source.Height, depth);
        var stride = side * 3 + 7; var samples = Enumerable.Repeat((byte)0xA7, stride * source.Height + 9).ToArray();
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < side; x++)
        for (var c = 0; c < 4; c++)
        {
            var value = source.GetRow8(y)[x * 4 + c];
            if (depth == 8) image.GetRow8(y)[x * 4 + c] = value;
            else image.GetRow16(y)[x * 4 + c] = (ushort)(value * 257);
            if (c < 3) samples[y * stride + x * 3 + c] = value;
        }
        var unchanged = samples.ToArray();
        var options = new JpegEncodeOptions { Quality = 69, Sampling = sampling, BackgroundRgb = 0,
            RestartInterval = 3, QuantizationPolicy = JpegQuantizationPolicy.CompactColor };
        var expected = Encode(image, options);
        using var direct = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, side, source.Height, JpegSampleFormat.Rgb, direct, stride, options);
        Assert.Equal(expected, direct.ToArray());
        using var token = new CancellationTokenSource(); using var fixedOutput = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, side, source.Height, JpegSampleFormat.Rgb, fixedOutput, stride, options, token.Token);
        using var wanted = ImageCodec.Decode(expected); using var fixedPixels = ImageCodec.Decode(fixedOutput.ToArray());
        Assert.Equal(OriginalPixels.Bytes(wanted), OriginalPixels.Bytes(fixedPixels));
        using var asyncSamples = new MemoryStream(); using var asyncImage = new MemoryStream();
        await ImageCodec.EncodeJpegAsync(samples, side, source.Height, JpegSampleFormat.Rgb, asyncSamples, stride, options);
        await ImageCodec.EncodeJpegAsync(image, asyncImage, options);
        using var samplePixels = ImageCodec.Decode(asyncSamples.ToArray()); using var imagePixels = ImageCodec.Decode(asyncImage.ToArray());
        Assert.Equal(OriginalPixels.Bytes(wanted), OriginalPixels.Bytes(samplePixels));
        Assert.Equal(OriginalPixels.Bytes(wanted), OriginalPixels.Bytes(imagePixels));
        Assert.Equal(unchanged, samples); samples.AsSpan().Clear();
        Assert.Equal(OriginalPixels.Bytes(wanted), OriginalPixels.Bytes(samplePixels));
        Assert.True(asyncSamples.CanWrite); Assert.True(asyncImage.CanWrite);
        Assert.NotEqual(samples, unchanged);
    }

    [Fact]
    public async Task UndefinedPolicyRejectsAllInputAndDeliveryBoundariesWithoutWrites()
    {
        using var image = OriginalPixels.Buffer(17, 11, 8, opaque: true);
        var samples = new byte[17 * 11 * 3]; using var output = new MemoryStream();
        using var root = new TestMemoryBudget(8 * 1024 * 1024);
        var options = new JpegEncodeOptions { QuantizationPolicy = (JpegQuantizationPolicy)2, Limits = new() { MemoryBudget = root } };
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(image, output, options));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(samples, 17, 11, JpegSampleFormat.Rgb, output, options: options));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpegAsync(image, output, options).AsTask());
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpegAsync(samples, 17, 11, JpegSampleFormat.Rgb, output, options: options).AsTask());
        Assert.Equal(0, output.Length); Assert.True(output.CanWrite); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(JpegQuantizationPolicy.Original)] [InlineData(JpegQuantizationPolicy.CompactColor)]
    public void SharedRefusalAndCancellationRefundWithoutFallbackAndAllowLaterSuccess(JpegQuantizationPolicy policy)
    {
        var samples = new byte[129 * 129 * 3]; using var output = new MemoryStream();
        using var root = new TestMemoryBudget(8 * 1024 * 1024);
        var options = new JpegEncodeOptions { QuantizationPolicy = policy, Limits = new() { MemoryBudget = root } };
        Assert.True(root.TryReserve(8 * 1024 * 1024, out var hold));
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(samples, 129, 129, JpegSampleFormat.Rgb, output, options: options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (failure.Limit, failure.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(0, output.Length); hold!.Dispose(); Assert.Equal(0, root.Bytes);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        var canceled = Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeJpeg(samples, 129, 129, JpegSampleFormat.Rgb, output,
            options: options, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, canceled.CancellationToken); Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length);
        root.Admitted = null;
        ImageCodec.EncodeJpeg(samples, 129, 129, JpegSampleFormat.Rgb, output, options: options);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(output.CanWrite); Assert.True(output.Length > 0);
        using var rejected = new MemoryStream();
        var limit = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(samples, 129, 129, JpegSampleFormat.Rgb, rejected,
            options: options with { Limits = options.Limits with { MaxOutputBytes = 10 } }));
        Assert.Equal(ImageLimitKind.OutputBytes, limit.Limit); Assert.Equal(0, root.Bytes); Assert.True(rejected.CanWrite);
    }

    private static byte[] Encode(ImageBuffer image, JpegEncodeOptions options)
    {
        using var output = new MemoryStream(); ImageCodec.EncodeJpeg(image, output, options); return output.ToArray();
    }
}
