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
    public async Task CallerSamplesKeepPacketsPaddingAndOwnership(JpegSampleFormat format, JpegSampling sampling)
    {
        using var image = OriginalPixels.Buffer(129, 129, 8, grayscale: format == JpegSampleFormat.Grayscale, opaque: true);
        var stride = image.Width * (int)format + 5;
        var samples = Enumerable.Repeat((byte)0xA7, stride * image.Height + 9).ToArray();
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
            image.GetRow8(y).Slice(x * 4, (int)format).CopyTo(samples.AsSpan(y * stride + x * (int)format));
        var original = samples.ToArray();
        var options = new JpegEncodeOptions { Quality = 69, Sampling = sampling, RestartInterval = 3 };
        using var expected = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, image.Width, image.Height, format, expected, stride, options);
        using var output = new AsyncOnlyOutput();
        await ImageCodec.EncodeJpegAsync(samples.AsMemory(), image.Width, image.Height, format, output, stride, options);
        Assert.Equal(expected.ToArray(), output.Bytes); Assert.Equal(original, samples); Assert.False(output.WasDisposed);
        using var token = new CancellationTokenSource(); using var livePacket = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, image.Width, image.Height, format, livePacket, stride, options, token.Token);
        Assert.Equal(output.Bytes, livePacket.ToArray());
        using var exact = new AsyncOnlyOutput();
        await ImageCodec.EncodeJpegAsync(samples.AsMemory(), image.Width, image.Height, format, exact, stride,
            options with { Limits = new() { MaxOutputBytes = checked((int)expected.Length) } });
        Assert.Equal(expected.ToArray(), exact.Bytes);
    }

    [Theory]
    [InlineData(JpegSampling.Grayscale, 8)] [InlineData(JpegSampling.YCbCr420, 8)] [InlineData(JpegSampling.YCbCr422, 16)]
    public async Task ImageJpegKeepsSyncAsyncPacketsAndExactOutputQuota(JpegSampling sampling, int depth)
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

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task LiveJpegCancellationRetainsEstablishedFixedPackets(bool samples)
    {
        using var image = OriginalPixels.Buffer(129, 129, 8, grayscale: true, opaque: true);
        var gray = new byte[129 * 129];
        for (var y = 0; y < 129; y++) for (var x = 0; x < 129; x++) gray[y * 129 + x] = image.GetRow8(y)[x * 4];
        var options = new JpegEncodeOptions { Quality = 69, Sampling = JpegSampling.Grayscale, BackgroundRgb = 0 };
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
    public async Task CallerPartialIoFailureIsPreservedAndSurvivingOutputStaysIndependent()
    {
        using var image = OriginalPixels.Buffer(257, 129, 8); var source = OriginalPixels.Bytes(image);
        using var expected = new MemoryStream(); ImageCodec.EncodePng(image, expected, new() { Compression = CompressionLevel.NoCompression });
        var limits = new ImageLimits { MaxWorkingBytes = 4_000_000};
        using var root = new TestMemoryBudget(limits.MaxWorkingBytes); limits = limits with { MemoryBudget = root };
        var original = new IOException("Original caller write failure.");
        using var output = new AsyncOnlyOutput { FailAt = 2, Failure = original };
        var actual = await Assert.ThrowsAsync<IOException>(() => ImageCodec.EncodePngAsync(image, output,
            new() { Limits = limits, Compression = CompressionLevel.NoCompression }).AsTask());
        Assert.Same(original, actual); Assert.Equal(65536 + 7, output.Bytes.Length);
        Assert.Equal(expected.ToArray()[..output.Bytes.Length], output.Bytes); Assert.False(output.WasDisposed);
        Assert.Equal(0, root.Bytes); var prefix = output.Bytes;
        using var recovery = new AsyncOnlyOutput();
        await ImageCodec.EncodePngAsync(image, recovery, new() { Limits = limits, Compression = CompressionLevel.NoCompression });
        Assert.Equal(expected.ToArray(), recovery.Bytes); Assert.Equal(prefix, output.Bytes); Assert.Equal(source, OriginalPixels.Bytes(image));
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
        public bool WasDisposed, RequireZeroUnusedTail;
        public CancellationToken LastToken;
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(WasDisposed, this);
            WriteCalls++; LargestWrite = Math.Max(LargestWrite, buffer.Length); LastToken = cancellationToken;
            await Task.Yield(); cancellationToken.ThrowIfCancellationRequested();
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
