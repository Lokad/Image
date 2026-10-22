using System.IO.Compression;
using System.Runtime.InteropServices;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class AsyncOutputTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task ReusedPrivateOutputDoesNotExposeAnEarlierPacketsTail(bool jpeg)
    {
        using var priming = new MemoryStream();
        using var image = OriginalPixels.Buffer(jpeg ? 129 : 33, jpeg ? 129 : 17, 8, grayscale: jpeg, opaque: true);
        if (jpeg) await ImageCodec.EncodeJpegAsync(image, priming,
            new() { Quality = 100, Sampling = JpegSampling.Grayscale, BackgroundRgb = 0 });
        else await ImageCodec.EncodePngAsync(image, priming);
        using var output = new AsyncOnlyOutput { RequireZeroUnusedTail = true };
        if (jpeg) await ImageCodec.EncodeJpegAsync(image, output,
            new() { Quality = 1, Sampling = JpegSampling.Grayscale, BackgroundRgb = 0 });
        else
        {
            using var smaller = OriginalPixels.Buffer(1, 1, 8, opaque: true);
            await ImageCodec.EncodePngAsync(smaller, output);
        }
        Assert.Equal(1, output.WriteCalls); Assert.True(priming.Length > output.Bytes.Length);
    }

    [Theory]
    [InlineData("png", 8)] [InlineData("png", 16)] [InlineData("jpeg", 8)] [InlineData("jpeg", 16)]
    [InlineData("vp8l", 8)] [InlineData("vp8l", 16)] [InlineData("vp8", 8)] [InlineData("vp8", 16)]
    public async Task AsyncOnlyOutputPreservesPacketsPixelsAndSource(string codec, int depth)
    {
        using var image = OriginalPixels.Buffer(17, 11, depth);
        var source = OriginalPixels.Bytes(image); using var expected = new MemoryStream();
        using var output = new AsyncOnlyOutput();
        Encode(image, expected, codec); await EncodeAsync(image, output, codec);
        Assert.Equal(expected.ToArray(), output.Bytes); Assert.Equal(source, OriginalPixels.Bytes(image));
        using var decoded = ImageCodec.Decode(output.Bytes); using var reference = ImageCodec.Decode(expected.ToArray());
        Assert.Equal(OriginalPixels.Bytes(reference), OriginalPixels.Bytes(decoded));
        Assert.False(output.WasDisposed); Assert.True(output.WriteCalls > 0); Assert.InRange(output.LargestWrite, 1, 65536);
        var saved = output.Bytes; using var second = new AsyncOnlyOutput();
        await EncodeAsync(image, second, codec); Assert.Equal(saved, output.Bytes); Assert.Equal(saved, second.Bytes);
    }

    [Theory]
    [InlineData(JpegSampleFormat.Grayscale, JpegSampling.YCbCr444)]
    [InlineData(JpegSampleFormat.Rgb, JpegSampling.Grayscale)]
    [InlineData(JpegSampleFormat.Rgb, JpegSampling.YCbCr444)]
    [InlineData(JpegSampleFormat.Rgb, JpegSampling.YCbCr422)]
    [InlineData(JpegSampleFormat.Rgb, JpegSampling.YCbCr420)]
    public async Task CallerSamplesKeepAdaptivePacketsPaddingAndOwnership(JpegSampleFormat format, JpegSampling sampling)
    {
        using var image = OriginalPixels.Buffer(129, 129, 8, grayscale: format == JpegSampleFormat.Grayscale, opaque: true);
        var stride = image.Width * (int)format + 5;
        var samples = Enumerable.Repeat((byte)0xA7, stride * image.Height + 9).ToArray();
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
            image.GetRow8(y).Slice(x * 4, (int)format).CopyTo(samples.AsSpan(y * stride + x * (int)format));
        var original = samples.ToArray();
        var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 69, Sampling = sampling, RestartInterval = 3 };
        using var expected = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, image.Width, image.Height, format, expected, stride, options);
        using var output = new AsyncOnlyOutput();
        await ImageCodec.EncodeJpegAsync(samples.AsMemory(), image.Width, image.Height, format, output, stride, options);
        Assert.Equal(expected.ToArray(), output.Bytes); Assert.Equal(original, samples); Assert.False(output.WasDisposed);
        using var token = new CancellationTokenSource(); using var livePacket = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, image.Width, image.Height, format, livePacket, stride, options, token.Token);
        Assert.Equal(output.Bytes, livePacket.ToArray());
        using var fixedPacket = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, image.Width, image.Height, format, fixedPacket, stride,
            options with { Limits = new() { MaxWorkingBytes = samples.Length + 8192L } });
        Assert.True(output.Bytes.Length < fixedPacket.Length);
        using var exact = new AsyncOnlyOutput();
        await ImageCodec.EncodeJpegAsync(samples.AsMemory(), image.Width, image.Height, format, exact, stride,
            options with { Limits = new() { MaxOutputBytes = checked((int)expected.Length) } });
        Assert.Equal(expected.ToArray(), exact.Bytes);
    }

    [Theory]
    [InlineData(JpegSampling.Grayscale, 8)] [InlineData(JpegSampling.YCbCr420, 8)] [InlineData(JpegSampling.YCbCr422, 16)]
    public async Task ImageJpegRetainsAdaptiveCodingAndExactOutputQuota(JpegSampling sampling, int depth)
    {
        using var source = OriginalPixels.Buffer(129, 129, depth, grayscale: sampling == JpegSampling.Grayscale);
        var options = new JpegEncodeOptions { Quality = 69, Sampling = sampling, BackgroundRgb = 0x204060 };
        using var expected = new MemoryStream(); ImageCodec.EncodeJpeg(source, expected, options);
        using var output = new AsyncOnlyOutput(); await ImageCodec.EncodeJpegAsync(source, output, options);
        Assert.Equal(expected.ToArray(), output.Bytes);
        using var exact = new AsyncOnlyOutput(); await ImageCodec.EncodeJpegAsync(source, exact,
            options with { Limits = new() { MaxOutputBytes = checked((int)expected.Length) } });
        Assert.Equal(expected.ToArray(), exact.Bytes);
        using var rejected = new AsyncOnlyOutput();
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.EncodeJpegAsync(source, rejected,
            options with { Limits = new() { MaxOutputBytes = checked((int)expected.Length) - 1 } }).AsTask());
        Assert.Equal(ImageLimitKind.OutputBytes, failure.Limit); Assert.Empty(rejected.Bytes); Assert.False(rejected.WasDisposed);
    }

    [Fact]
    public async Task TightJpegAdmissionUsesEstablishedFallbackWithoutChangingPixels()
    {
        using var source = OriginalPixels.Buffer(129, 129, 8, grayscale: true, opaque: true);
        var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 69, Sampling = JpegSampling.Grayscale, BackgroundRgb = 0 };
        using var adaptive = new MemoryStream(); ImageCodec.EncodeJpeg(source, adaptive, options);
        var limited = options with { Limits = new() { MaxWorkingBytes = 129 * 129 * 4 + 8192 + 65536 + 320 } };
        using var expected = new MemoryStream(); ImageCodec.EncodeJpeg(source, expected, limited);
        using var output = new AsyncOnlyOutput(); await ImageCodec.EncodeJpegAsync(source, output, limited);
        Assert.Equal(expected.ToArray(), output.Bytes); Assert.True(output.Bytes.Length > adaptive.Length);
        using var pixels = ImageCodec.Decode(output.Bytes); using var reference = ImageCodec.Decode(adaptive.ToArray());
        Assert.Equal(OriginalPixels.Bytes(reference), OriginalPixels.Bytes(pixels));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task LiveJpegCancellationRetainsEstablishedFixedPackets(bool samples)
    {
        using var image = OriginalPixels.Buffer(129, 129, 8, grayscale: true, opaque: true);
        var gray = new byte[129 * 129];
        for (var y = 0; y < 129; y++) for (var x = 0; x < 129; x++) gray[y * 129 + x] = image.GetRow8(y)[x * 4];
        var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 69, Sampling = JpegSampling.Grayscale, BackgroundRgb = 0 };
        using var token = new CancellationTokenSource(); using var expected = new MemoryStream(); using var output = new AsyncOnlyOutput();
        if (samples)
        {
            ImageCodec.EncodeJpeg(gray, 129, 129, JpegSampleFormat.Grayscale, expected, options: options, cancellationToken: token.Token);
            await ImageCodec.EncodeJpegAsync(gray.AsMemory(), 129, 129, JpegSampleFormat.Grayscale, output, options: options, cancellationToken: token.Token);
        }
        else
        {
            ImageCodec.EncodeJpeg(image, expected, options, token.Token);
            await ImageCodec.EncodeJpegAsync(image, output, options, token.Token);
        }
        Assert.Equal(expected.ToArray(), output.Bytes); Assert.Equal(token.Token, output.LastToken);
    }

    [Fact]
    public async Task TightCallerSamplesUseFixedCodingWithUnchangedPixels()
    {
        using var image = OriginalPixels.Buffer(129, 129, 8, grayscale: true, opaque: true);
        var gray = new byte[129 * 129];
        for (var y = 0; y < 129; y++) for (var x = 0; x < 129; x++) gray[y * 129 + x] = image.GetRow8(y)[x * 4];
        var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 69, Limits = new() { MaxWorkingBytes = gray.Length + 8192 + 65536 + 320 } };
        using var expected = new MemoryStream(); ImageCodec.EncodeJpeg(gray, 129, 129, JpegSampleFormat.Grayscale, expected, options: options);
        using var output = new AsyncOnlyOutput();
        await ImageCodec.EncodeJpegAsync(gray.AsMemory(), 129, 129, JpegSampleFormat.Grayscale, output, options: options);
        Assert.Equal(expected.ToArray(), output.Bytes);
    }

    [Theory]
    [InlineData("jpeg")] [InlineData("vp8l")] [InlineData("vp8")] [InlineData("samples")]
    public async Task EveryBudgetedEncoderKeepsItsReservationUntilDeliveryCompletes(string codec)
    {
        using var image = OriginalPixels.Buffer(17, 11, 8, opaque: true);
        var source = OriginalPixels.Bytes(image); var rgb = new byte[17 * 11 * 3];
        for (var y = 0; y < 11; y++) for (var x = 0; x < 17; x++) image.GetRow8(y).Slice(x * 4, 3).CopyTo(rgb.AsSpan((y * 17 + x) * 3));
        var limits = new ImageLimits { MaxWorkingBytes = 2_000_000, MaxWork = 5_000_000 };
        var budget = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork); using var expected = new MemoryStream();
        using var output = new AsyncOnlyOutput { Pause = true };
        Task pending;
        if (codec == "samples")
        {
            ImageCodec.EncodeJpeg(rgb, 17, 11, JpegSampleFormat.Rgb, expected, options: new() { Quality = 69 });
            pending = budget.EncodeJpegAsync(rgb.AsMemory(), 17, 11, JpegSampleFormat.Rgb, output,
                options: new() { Quality = 69, Limits = limits }).AsTask();
        }
        else
        {
            Encode(image, expected, codec);
            pending = codec == "jpeg" ? budget.EncodeJpegAsync(image, output, new() { Quality = 69, BackgroundRgb = 0x204060, Limits = limits }).AsTask()
                : budget.EncodeWebpAsync(image, output, new() { Lossless = codec == "vp8l", Quality = 85, Effort = 4, Limits = limits }).AsTask();
        }
        await output.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(pending.IsCompleted); Assert.Equal(0, budget.RemainingWorkingBytes);
        output.Resume.TrySetResult(); await pending;
        Assert.Equal(expected.ToArray(), output.Bytes); Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.True(budget.ConsumedWork > 0); Assert.Equal(source, OriginalPixels.Bytes(image)); Assert.False(output.WasDisposed);
    }

    [Fact]
    public async Task ExactPrivateStagingMemoryIsAdmittedAndRefundedOnFailure()
    {
        using var image = OriginalPixels.Buffer(1, 1, 8); using var expected = new MemoryStream();
        ImageCodec.EncodePng(image, expected, new() { Compression = CompressionLevel.NoCompression });
        var limits = new ImageLimits { MaxOutputBytes = checked((int)expected.Length), MaxWorkingBytes = 4 + 4 + 1024 * 1024 + 65536 + 320 + expected.Length, MaxWork = 1_000_000 };
        var budget = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork * 2);
        using var output = new AsyncOnlyOutput();
        await budget.EncodePngAsync(image, output, new() { Limits = limits, Compression = CompressionLevel.NoCompression });
        Assert.Equal(expected.ToArray(), output.Bytes); Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        using var failed = new AsyncOnlyOutput();
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => budget.EncodePngAsync(image, failed,
            new() { Limits = limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }, Compression = CompressionLevel.NoCompression }).AsTask());
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Empty(failed.Bytes);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.False(failed.WasDisposed);
    }

    [Fact]
    public async Task SharedBudgetSurvivesOutputAwaitAndIndependentOperationsDoNotAlias()
    {
        using var image = OriginalPixels.Buffer(17, 11, 8); var source = OriginalPixels.Bytes(image);
        var limits = new ImageLimits { MaxWorkingBytes = 2_000_000, MaxWork = 5_000_000 };
        var budget = new ImageBudget(limits.MaxWorkingBytes + 77, limits.MaxWork * 3);
        using var caller = budget.Reserve(77); using var output = new AsyncOnlyOutput { Pause = true };
        var pending = budget.EncodePngAsync(image, output, new() { Limits = limits }).AsTask();
        await output.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(pending.IsCompleted); Assert.Equal(0, budget.RemainingWorkingBytes);
        var failure = Assert.Throws<ImageLimitException>(() => budget.Reserve(1)); Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
        using var independent = new AsyncOnlyOutput(); await ImageCodec.EncodePngAsync(image, independent);
        var saved = independent.Bytes; output.Resume.TrySetResult(); await pending;
        Assert.Equal(saved, output.Bytes); Assert.Equal(saved, independent.Bytes); Assert.Equal(source, OriginalPixels.Bytes(image));
        Assert.Equal(limits.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.False(output.WasDisposed);
    }

    [Fact]
    public async Task CancellationDuringSuspendedWriteReturnsPrivateStorageAndSharedReservation()
    {
        using var image = OriginalPixels.Buffer(17, 11, 8);
        var limits = new ImageLimits { MaxWorkingBytes = 2_000_000, MaxWork = 5_000_000 };
        var budget = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork);
        using var cancellation = new CancellationTokenSource(); using var output = new AsyncOnlyOutput { Pause = true };
        var pending = budget.EncodePngAsync(image, output, new() { Limits = limits }, cancellation.Token).AsTask();
        await output.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); cancellation.Cancel();
        var failure = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.Equal(cancellation.Token, output.LastToken);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.True(budget.ConsumedWork > 0);
        Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork); Assert.False(output.WasDisposed);
    }

    [Fact]
    public async Task PreCancellationDoesNotWriteOrConsumeSharedBudget()
    {
        using var image = OriginalPixels.Buffer(1, 1, 8); using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var budget = new ImageBudget(0, 0); using var output = new AsyncOnlyOutput();
        var failure = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => budget.EncodeWebpAsync(image, output, cancellationToken: cancellation.Token).AsTask());
        Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.Equal(0, output.WriteCalls); Assert.Equal(0, budget.ConsumedWork);
    }

    [Fact]
    public async Task CallerPartialIoFailureIsPreservedAndSurvivingOutputStaysIndependent()
    {
        using var image = OriginalPixels.Buffer(257, 129, 8); var source = OriginalPixels.Bytes(image);
        using var expected = new MemoryStream(); ImageCodec.EncodePng(image, expected, new() { Compression = CompressionLevel.NoCompression });
        var limits = new ImageLimits { MaxWorkingBytes = 4_000_000, MaxWork = 50_000_000 };
        var budget = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork * 2);
        var original = new IOException("Original caller write failure.");
        using var output = new AsyncOnlyOutput { FailAt = 2, Failure = original };
        var actual = await Assert.ThrowsAsync<IOException>(() => budget.EncodePngAsync(image, output,
            new() { Limits = limits, Compression = CompressionLevel.NoCompression }).AsTask());
        Assert.Same(original, actual); Assert.Equal(65536 + 7, output.Bytes.Length);
        Assert.Equal(expected.ToArray()[..output.Bytes.Length], output.Bytes); Assert.False(output.WasDisposed);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); var prefix = output.Bytes;
        using var recovery = new AsyncOnlyOutput();
        await budget.EncodePngAsync(image, recovery, new() { Limits = limits, Compression = CompressionLevel.NoCompression });
        Assert.Equal(expected.ToArray(), recovery.Bytes); Assert.Equal(prefix, output.Bytes); Assert.Equal(source, OriginalPixels.Bytes(image));
    }

    [Fact]
    public async Task WorkLimitDuringDeliveryLeavesOnlyCompletedPrefixAndRefundsMemory()
    {
        using var image = OriginalPixels.Buffer(257, 129, 8);
        var limits = new ImageLimits { MaxWorkingBytes = 4_000_000, MaxWork = 50_000_000 };
        var measured = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork); using var expected = new AsyncOnlyOutput();
        await measured.EncodePngAsync(image, expected, new() { Limits = limits, Compression = CompressionLevel.NoCompression });
        var constrained = limits with { MaxWork = measured.ConsumedWork - 1 };
        var budget = new ImageBudget(constrained.MaxWorkingBytes, constrained.MaxWork); using var output = new AsyncOnlyOutput();
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => budget.EncodePngAsync(image, output,
            new() { Limits = constrained, Compression = CompressionLevel.NoCompression }).AsTask());
        Assert.Equal(ImageLimitKind.Work, failure.Limit); Assert.InRange(output.Bytes.Length, 65536, expected.Bytes.Length - 1);
        Assert.Equal(expected.Bytes[..output.Bytes.Length], output.Bytes); Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
    }

    [Fact]
    public async Task CancellationReportedByTheFinalWriteKeepsTheCallerToken()
    {
        using var image = OriginalPixels.Buffer(1, 1, 8); using var cancellation = new CancellationTokenSource();
        using var output = new AsyncOnlyOutput { CancelAfterWrite = cancellation };
        var failure = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageCodec.EncodePngAsync(image, output, cancellationToken: cancellation.Token).AsTask());
        Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.NotEmpty(output.Bytes); Assert.False(output.WasDisposed);
    }

    [Fact]
    public async Task CmykSampleEncodingKeepsStructuredUnsupportedFailure()
    {
        using var output = new AsyncOnlyOutput();
        var failure = await Assert.ThrowsAsync<UnsupportedImageException>(() => ImageCodec.EncodeJpegAsync(new byte[4].AsMemory(),
            1, 1, JpegSampleFormat.Cmyk, output).AsTask());
        Assert.Equal(UnsupportedImageFeature.ComponentLayout, failure.Feature); Assert.Equal(ImageFormat.Jpeg, failure.Format); Assert.Empty(output.Bytes);
    }

    private static void Encode(ImageBuffer source, Stream output, string codec)
    {
        if (codec == "png") ImageCodec.EncodePng(source, output);
        else if (codec == "jpeg") ImageCodec.EncodeJpeg(source, output, new() { Quality = 69, BackgroundRgb = 0x204060 });
        else ImageCodec.EncodeWebp(source, output, new() { Lossless = codec == "vp8l", Quality = 85, Effort = 4 });
    }
    private static ValueTask EncodeAsync(ImageBuffer source, Stream output, string codec)
        => codec == "png" ? ImageCodec.EncodePngAsync(source, output)
            : codec == "jpeg" ? ImageCodec.EncodeJpegAsync(source, output, new() { Quality = 69, BackgroundRgb = 0x204060 })
            : ImageCodec.EncodeWebpAsync(source, output, new() { Lossless = codec == "vp8l", Quality = 85, Effort = 4 });

    private sealed class AsyncOnlyOutput : Stream
    {
        private readonly MemoryStream content = new();
        public byte[] Bytes => content.ToArray();
        public int WriteCalls, LargestWrite;
        public int FailAt = int.MaxValue;
        public IOException? Failure;
        public CancellationTokenSource? CancelAfterWrite;
        public bool WasDisposed, Pause, RequireZeroUnusedTail;
        public CancellationToken LastToken;
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(WasDisposed, this);
            WriteCalls++; LargestWrite = Math.Max(LargestWrite, buffer.Length); LastToken = cancellationToken;
            await Task.Yield(); cancellationToken.ThrowIfCancellationRequested();
            if (Pause) { Entered.TrySetResult(); await Resume.Task.WaitAsync(cancellationToken); }
            if (RequireZeroUnusedTail && MemoryMarshal.TryGetArray(buffer, out var segment))
                Assert.Equal(-1, segment.Array!.AsSpan(segment.Offset + segment.Count).IndexOfAnyExcept((byte)0));
            if (WriteCalls == FailAt && Failure is { } failure) { content.Write(buffer.Span[..Math.Min(7, buffer.Length)]); throw failure; }
            content.Write(buffer.Span); CancelAfterWrite?.Cancel();
        }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !WasDisposed;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Synchronous write is forbidden.");
        public override void Write(ReadOnlySpan<byte> buffer) => throw new InvalidOperationException("Synchronous write is forbidden.");
        public override void WriteByte(byte value) => throw new InvalidOperationException("Synchronous write is forbidden.");
        public override void Flush() => throw new InvalidOperationException("Caller flush is forbidden.");
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
