using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegSampleEncodeTests
{
    [Theory]
    [InlineData(JpegSampleFormat.Grayscale, JpegSampling.YCbCr420, 100)]
    [InlineData(JpegSampleFormat.Rgb, JpegSampling.Grayscale, 1)]
    [InlineData(JpegSampleFormat.Rgb, JpegSampling.YCbCr444, 86)]
    [InlineData(JpegSampleFormat.Rgb, JpegSampling.YCbCr422, 1)]
    [InlineData(JpegSampleFormat.Rgb, JpegSampling.YCbCr420, 100)]
    public void CallerAndImageSamplesShareQualitySamplingAndPackets(JpegSampleFormat format, JpegSampling sampling, int quality)
    {
        using var source = OriginalPixels.Buffer(17, 11, 8, grayscale: format == JpegSampleFormat.Grayscale, opaque: true);
        var samples = Samples(source, format, 5); var original = samples.ToArray();
        var options = new JpegEncodeOptions { Sampling = sampling, Quality = quality, RestartInterval = 2 };
        using var expected = new MemoryStream();
        ImageCodec.EncodeJpeg(source, expected, Effective(options, format));
        using var actual = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, source.Width, source.Height, format, actual, source.Width * (int)format + 5, options);
        Assert.Equal(expected.ToArray(), actual.ToArray());
        Assert.Equal(original, samples); Assert.True(actual.CanWrite);
        var header = JpegQualitySyntax.Read(actual.ToArray());
        Assert.Equal(format == JpegSampleFormat.Grayscale || sampling == JpegSampling.Grayscale ? 1 : 3, header.Components);
        using var liveToken = new CancellationTokenSource(); using var cancelable = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, source.Width, source.Height, format, cancelable,
            source.Width * (int)format + 5, options, liveToken.Token);
        Assert.Equal(actual.ToArray(), cancelable.ToArray());
    }

    [Theory]
    [InlineData(1, 1, JpegSampleFormat.Rgb)]
    [InlineData(1, 17, JpegSampleFormat.Rgb)]
    [InlineData(17, 1, JpegSampleFormat.Rgb)]
    [InlineData(1, 17, JpegSampleFormat.Grayscale)]
    public void BoundedRowNormalizationClampsPartialBlocksWithoutReadingPadding(int width, int height, JpegSampleFormat format)
    {
        using var source = OriginalPixels.Buffer(width, height, 8, grayscale: format == JpegSampleFormat.Grayscale, opaque: true);
        var padded = Samples(source, format, 7); var packed = Samples(source, format, 0);
        using var expected = new MemoryStream(); using var actual = new MemoryStream(); using var tight = new MemoryStream();
        var options = new JpegEncodeOptions { Quality = 85, Sampling = JpegSampling.YCbCr420 };
        ImageCodec.EncodeJpeg(source, expected, Effective(options, format));
        ImageCodec.EncodeJpeg(padded, width, height, format, actual, width * (int)format + 7, options);
        ImageCodec.EncodeJpeg(packed, width, height, format, tight, options: options);
        Assert.Equal(expected.ToArray(), actual.ToArray()); Assert.Equal(actual.ToArray(), tight.ToArray());
    }

    [Fact]
    public void ConstantGraySamplesEncodeOneComponentAndOutputSurvivesSourceChanges()
    {
        var samples = Enumerable.Repeat((byte)123, 64).ToArray();
        using var output = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, 8, 8, JpegSampleFormat.Grayscale, output, options: new() { Quality = 100 });
        var encoded = output.ToArray(); samples.AsSpan().Clear();
        Assert.Equal(1, JpegQualitySyntax.Read(encoded).Components);
        var decoded = new byte[64]; ImageCodec.DecodeJpeg(encoded, decoded, JpegSampleFormat.Grayscale);
        Assert.All(decoded, value => Assert.Equal(123, value));
    }

    [Fact]
    public void InvalidArgumentsAndPreCancellationNeverWriteOrChangeSource()
    {
        var samples = Enumerable.Repeat((byte)123, 192).ToArray(); var original = samples.ToArray();
        using var output = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(samples, 8, 8, (JpegSampleFormat)2, output));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(samples, 8, 8, JpegSampleFormat.Rgb, output, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(samples, 8, 8, JpegSampleFormat.Rgb, output, 23));
        Assert.Throws<ArgumentException>(() => ImageCodec.EncodeJpeg(samples.AsSpan(0, 191), 8, 8, JpegSampleFormat.Rgb, output));
        Assert.Throws<ArgumentException>(() => ImageCodec.EncodeJpeg(samples, 8, 8, JpegSampleFormat.Rgb, output, int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(samples, 0, 8, JpegSampleFormat.Rgb, output));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(samples, 8, 8, JpegSampleFormat.Rgb, output, options: new() { Quality = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(samples, 8, 8, JpegSampleFormat.Grayscale, output, options: new() { Sampling = (JpegSampling)99 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(samples, 8, 8, JpegSampleFormat.Rgb, output, options: new() { BackgroundRgb = 0x1000000 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeJpeg(samples, 8, 8, JpegSampleFormat.Rgb, output, options: new() { RestartInterval = 65536 }));
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var failure = Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeJpeg(samples, 8, 8, JpegSampleFormat.Rgb, output,
            options: new() { Quality = 0 }, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, failure.CancellationToken);
        Assert.Equal(0, output.Length); Assert.True(output.CanWrite); Assert.Equal(original, samples);
    }

    [Fact]
    public void SharedSuccessfulWorkAndWorkFailureSettleWithoutTakingSourceOwnership()
    {
        using var source = OriginalPixels.Buffer(17, 11, 8, opaque: true);
        var samples = Samples(source, JpegSampleFormat.Rgb, 0); var original = samples.ToArray();
        var limits = new ImageLimits { MaxWorkingBytes = 1024 * 1024, MaxWork = 2_000_000 };
        var options = new JpegEncodeOptions { Limits = limits };
        var budget = new ImageBudget(limits.MaxWorkingBytes + samples.Length, limits.MaxWork);
        using var expected = new MemoryStream();
        using (budget.Reserve(samples.Length))
        {
            budget.EncodeJpeg(samples, source.Width, source.Height, JpegSampleFormat.Rgb, expected,
                options: options);
            Assert.Equal(limits.MaxWorkingBytes, budget.RemainingWorkingBytes);
        }
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.True(budget.ConsumedWork > 0);
        Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork);
        var shortBudget = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork);
        using var failed = new MemoryStream();
        var failure = Assert.Throws<ImageLimitException>(() => shortBudget.EncodeJpeg(samples, source.Width, source.Height,
            JpegSampleFormat.Rgb, failed, options: options with { Limits = limits with { MaxWork = budget.ConsumedWork - 1 } }));
        Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (failure.Limit, failure.Scope));
        // Reject the bounded plan before entropy work/output; release capacity.
        Assert.Equal(0, shortBudget.ConsumedWork); Assert.Equal(0, failed.Length);
        using var retry = new MemoryStream();
        shortBudget.EncodeJpeg(samples, source.Width, source.Height, JpegSampleFormat.Rgb, retry, options: options);
        Assert.Equal(expected.ToArray(), retry.ToArray());
        Assert.Equal(shortBudget.MaxWorkingBytes, shortBudget.RemainingWorkingBytes);
        Assert.Equal(shortBudget.MaxWork - shortBudget.ConsumedWork, shortBudget.RemainingWork);
        Assert.Equal(original, samples); Assert.True(failed.CanWrite);
    }

    [Fact]
    public void CallbackStreamFailureAndCancellationPreserveOwnershipAndSettledBudgets()
    {
        using var source = OriginalPixels.Buffer(129, 129, 8, opaque: true);
        var samples = Samples(source, JpegSampleFormat.Rgb, 0); var original = samples.ToArray();
        var limits = new ImageLimits { MaxWorkingBytes = 1024 * 1024, MaxWork = 50_000_000 };
        var budget = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork * 2);
        var options = new JpegEncodeOptions { Limits = limits, BackgroundRgb = 0, RestartInterval = 2 };
        using var expected = new FailingStream(); using var actual = new FailingStream();
        Assert.Same(expected.Failure, Assert.Throws<IOException>(() => ImageCodec.EncodeJpeg(source, expected, options)));
        Assert.Same(actual.Failure, Assert.Throws<IOException>(() => budget.EncodeJpeg(samples, source.Width, source.Height, JpegSampleFormat.Rgb, actual, options: options)));
        Assert.Equal(expected.ToArray(), actual.ToArray()); Assert.True(actual.CanWrite);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork);
        using var cancellation = new CancellationTokenSource(); using var canceledOutput = new CancelingStream(cancellation);
        var failure = Assert.Throws<OperationCanceledException>(() => budget.EncodeJpeg(samples, source.Width, source.Height,
            JpegSampleFormat.Rgb, canceledOutput, options: options, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.True(canceledOutput.CanWrite);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork);
        Assert.Equal(original, samples);
        var empty = new ImageBudget(0, limits.MaxWork);
        using var untouched = new MemoryStream();
        var quota = Assert.Throws<ImageLimitException>(() => empty.EncodeJpeg(samples, source.Width, source.Height,
            JpegSampleFormat.Rgb, untouched, options: options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (quota.Limit, quota.Scope));
        Assert.Equal(0, untouched.Length); Assert.Equal(0, empty.ConsumedWork);
    }

    private static JpegEncodeOptions Effective(JpegEncodeOptions options, JpegSampleFormat format)
        => format == JpegSampleFormat.Grayscale ? options with { Sampling = JpegSampling.Grayscale } : options;

    private static byte[] Samples(ImageBuffer image, JpegSampleFormat format, int padding)
    {
        var channels = (int)format; var stride = image.Width * channels + padding;
        var result = Enumerable.Repeat((byte)0xA7, stride * image.Height + 9).ToArray();
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
            image.GetRow8(y).Slice(x * 4, channels).CopyTo(result.AsSpan(y * stride + x * channels, channels));
        return result;
    }

    private sealed class FailingStream : MemoryStream
    {
        internal readonly IOException Failure = new("Authored JPEG output failure");
        private int writes;
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (++writes == 4) throw Failure;
            base.Write(buffer);
        }
    }

    private sealed class CancelingStream(CancellationTokenSource cancellation) : MemoryStream
    {
        public override void Write(ReadOnlySpan<byte> buffer) { base.Write(buffer); cancellation.Cancel(); }
    }
}
