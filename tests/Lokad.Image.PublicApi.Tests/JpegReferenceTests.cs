using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegReferenceTests
{
    private static readonly JpegEncodeOptions Options = new()
    {
        QuantizationPolicy = JpegQuantizationPolicy.Reference, Sampling = JpegSampling.YCbCr444,
        BackgroundRgb = 0x204060
    };

    [Theory]
    [InlineData(8, "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80", 133039,
        "21389750d4936f4c45f8252020843576916cd5689913de2396ee4113df379ecd")]
    [InlineData(16, "3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9", 133041,
        "f69ce841c602373b71afac9ce403d2d634fe84630cca21cd585c370169cc2a07")]
    public async Task FrozenNativePacketsMatchAcrossOwnedAndCallerBoundaries(int depth, string sourceHash, int length, string packetHash)
    {
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var limits = ImageLimits.Default with { MemoryBudget = root };
        using var image = ImageBuffer.Create(513, 259, depth, limits);
        var original = OriginalPixels.Generate(513, 259, depth);
        original.AsSpan(3 * (depth / 8), depth / 8).Clear();
        original.AsSpan((513 * 4 - 1) * (depth / 8), depth / 8).Clear();
        Assert.Equal(sourceHash, Hash(original));
        for (var y = 0; y < 259; y++)
            if (depth == 8) original.AsSpan(y * 513 * 4, 513 * 4).CopyTo(image.GetRow8(y));
            else for (var x = 0; x < 513 * 4; x++)
                image.GetRow16(y)[x] = BinaryPrimitives.ReadUInt16LittleEndian(original.AsSpan((y * 513 * 4 + x) * 2));
        var options = Options with { Limits = limits };
        using var first = new MemoryStream(); ImageCodec.EncodeJpeg(image, first, options);
        AssertPacket(first.ToArray(), length, packetHash); Assert.Equal(original.Length, root.Bytes);
        using var forward = new ForwardOutput(); ImageCodec.EncodeJpeg(image, forward, options);
        AssertPacket(forward.ToArray(), length, packetHash);
        using var asynchronous = new MemoryStream(); await ImageCodec.EncodeJpegAsync(image, asynchronous, options);
        AssertPacket(asynchronous.ToArray(), length, packetHash); Assert.Equal(original.Length, root.Bytes);

        // Same effective source samples, padded caller stride and suffix, no RGBA
        // input required by the sample API. The reference normalization is shared
        // fixture tooling, independent of the encoder's image-row reader.
        var normalized = JpegQualityCorpus.Reference(original, depth, 0x204060);
        const int stride = 513 * 3 + 5;
        var rgb = Enumerable.Repeat((byte)165, stride * 259 + 7).ToArray();
        for (var y = 0; y < 259; y++)
        for (var x = 0; x < 513; x++)
            normalized.AsSpan((y * 513 + x) * 4, 3).CopyTo(rgb.AsSpan(y * stride + x * 3, 3));
        var beforeRgb = rgb.ToArray();
        using var samples = new MemoryStream();
        ImageCodec.EncodeJpeg(rgb, 513, 259, JpegSampleFormat.Rgb, samples, stride, options);
        AssertPacket(samples.ToArray(), length, packetHash);
        using var asyncSamples = new MemoryStream();
        await ImageCodec.EncodeJpegAsync(rgb, 513, 259, JpegSampleFormat.Rgb, asyncSamples, stride, options);
        AssertPacket(asyncSamples.ToArray(), length, packetHash);
        Assert.Equal(beforeRgb, rgb); Assert.Equal(original, OriginalPixels.Bytes(image));
        Assert.Equal(original.Length, root.Bytes); image.Dispose(); Assert.Equal(0, root.Bytes);
        AssertPacket(first.ToArray(), length, packetHash); AssertPacket(samples.ToArray(), length, packetHash);
        Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)] [InlineData(85)] [InlineData(100)]
    public void SmallImagesAndQualityEndpointsRemainValid(int quality)
    {
        using var image = OriginalPixels.Buffer(1, 1, 16, opaque: true);
        using var output = new MemoryStream();
        ImageCodec.EncodeJpeg(image, output, Options with { Quality = quality });
        using var decoded = ImageCodec.Decode(output.ToArray());
        Assert.Equal((1, 1, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
        Assert.Equal((byte)255, decoded.GetRow8(0)[3]);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void RefusalIsTerminalAndReleasesPriorReservations(long atGrowth)
    {
        using var image = OriginalPixels.Buffer(17, 11, 8);
        var before = OriginalPixels.Bytes(image);
        using var root = new TestMemoryBudget(1024 * 1024) { RefuseAtGrowth = atGrowth };
        var options = Options with { Limits = ImageLimits.Default with { MemoryBudget = root } };
        using var output = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, output, options));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length); Assert.Equal(before, OriginalPixels.Bytes(image));
        root.RefuseAtGrowth = 0; ImageCodec.EncodeJpeg(image, output, options);
        Assert.True(output.Length > 0); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void WorkCancellationAndFailedWritesCleanUpWithoutDisposingTheCaller()
    {
        using var image = OriginalPixels.Buffer(17, 11, 8);
        using var root = new TestMemoryBudget(1024 * 1024);
        var options = Options with { Limits = ImageLimits.Default with { MemoryBudget = root } };
        using var output = new MemoryStream();
        var limited = options with { Limits = options.Limits with { MaxWork = 1 } };
        Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, output, limited)).Limit);
        var local = options with { Limits = options.Limits with { MaxWorkingBytes = 17 * 11 * 4 + 8192 + 16384 - 1 } };
        var localError = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, output, local));
        Assert.Equal(ImageLimitKind.WorkingBytes, localError.Limit); Assert.Equal(ImageLimitScope.Operation, localError.Scope);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length);
        using var cancel = new CancellationTokenSource(); root.Admitted = _ => cancel.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.EncodeJpeg(image, output, options, cancel.Token));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length); root.Admitted = null;
        using var failing = new ThrowOutput();
        Assert.Throws<IOException>(() => ImageCodec.EncodeJpeg(image, failing, options));
        Assert.True(failing.CanWrite); Assert.Equal(0, root.Bytes);
        ImageCodec.EncodeJpeg(image, output, options); var size = (int)output.Length;
        using var exact = new MemoryStream(); ImageCodec.EncodeJpeg(image, exact, options with { Limits = options.Limits with { MaxOutputBytes = size } });
        using var tooSmall = new MemoryStream();
        Assert.Equal(ImageLimitKind.OutputBytes, Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, tooSmall,
            options with { Limits = options.Limits with { MaxOutputBytes = size - 1 } })).Limit);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, false)] [InlineData(false, true)]
    public async Task SharedRootIncludesPendingActualAsyncUsersAndRecovers(bool cancel, bool fail)
    {
        using var image = OriginalPixels.Buffer(17, 11, 8);
        long required;
        using (var measure = new TestMemoryBudget(1024 * 1024))
        using (var probe = new MemoryStream())
        {
            ImageCodec.EncodeJpeg(image, probe, Options with { Limits = ImageLimits.Default with { MemoryBudget = measure } });
            required = measure.Peak;
        }
        using var root = new TestMemoryBudget(required);
        var options = Options with { Limits = ImageLimits.Default with { MemoryBudget = root } };
        using var token = new CancellationTokenSource(); using var held = new HeldOutput(root, fail);
        var pending = ImageCodec.EncodeJpegAsync(image, held, options, token.Token).AsTask();
        await held.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            var charged = root.Bytes; Assert.True(charged > 0);
            using var competing = new MemoryStream();
            Assert.Equal(ImageLimitScope.SharedBudget,
                Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, competing, options)).Scope);
            Assert.Equal(charged, root.Bytes); Assert.Equal(0, competing.Length);
            if (cancel) { token.Cancel(); Assert.False(pending.IsCompleted); Assert.Equal(charged, root.Bytes); }
        }
        finally { held.Resume.TrySetResult(); }
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        else if (fail) await Assert.ThrowsAsync<IOException>(() => pending);
        else await pending;
        Assert.Equal(0, root.Bytes); Assert.True(held.CanWrite);
        using var next = new MemoryStream(); ImageCodec.EncodeJpeg(image, next, options);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(JpegSampling.YCbCr420, 0, UnsupportedImageFeature.SamplingLayout)]
    [InlineData(JpegSampling.YCbCr444, 1, UnsupportedImageFeature.ControlMarker)]
    public void PendingReferenceFeaturesAreStructuredFailures(JpegSampling sampling, int restart, UnsupportedImageFeature feature)
    {
        using var image = OriginalPixels.Buffer(1, 1, 8, opaque: true);
        using var output = new MemoryStream();
        var error = Assert.Throws<UnsupportedImageException>(() => ImageCodec.EncodeJpeg(image, output,
            Options with { Sampling = sampling, RestartInterval = restart }));
        Assert.Equal(feature, error.Feature); Assert.Equal(0, output.Length);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static void AssertPacket(byte[] packet, int length, string hash)
    { Assert.Equal(length, packet.Length); Assert.Equal(hash, Hash(packet)); }
    private sealed class ForwardOutput : MemoryStream { public override bool CanSeek => false; }
    private sealed class ThrowOutput : MemoryStream
    {
        public override void Write(ReadOnlySpan<byte> buffer)
        { for (var i = 0; i < Math.Min(3, buffer.Length); i++) base.WriteByte(buffer[i]); throw new IOException("Authored failed write."); }
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
    }
    private sealed class HeldOutput(TestMemoryBudget root, bool fail) : MemoryStream
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Entered.TrySetResult(); await Resume.Task; // Deliberately finish the actual borrow after cancellation.
            Assert.True(root.Bytes > 0);
            if (fail) throw new IOException("Authored asynchronous failure.");
            await base.WriteAsync(buffer, CancellationToken.None);
        }
    }
}
