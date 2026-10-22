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
    [InlineData(JpegSampling.YCbCr444, 8, 85, 0, "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80", 133039,
        "21389750d4936f4c45f8252020843576916cd5689913de2396ee4113df379ecd")]
    [InlineData(JpegSampling.YCbCr444, 16, 85, 0, "3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9", 133041,
        "f69ce841c602373b71afac9ce403d2d634fe84630cca21cd585c370169cc2a07")]
    [InlineData(JpegSampling.YCbCr422, 8, 85, 0, "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80", 71166,
        "a1d3d66671a19606075b7604173922d7677236928381a14355bc6fb640ebd3a4")]
    [InlineData(JpegSampling.YCbCr422, 16, 85, 0, "3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9", 71208,
        "afe4c4f147555ca198d3c82858c481eb0901dab08cd3ccf7c287915fba00d030")]
    [InlineData(JpegSampling.YCbCr420, 8, 85, 0, "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80", 52091,
        "83e9e80a87c1928c8e23e2929b15348d0e8e8ce5568247420f6e366abeb830e3")]
    [InlineData(JpegSampling.YCbCr420, 16, 85, 0, "3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9", 52115,
        "20461d0d4c85dd2d07e1068e2f9d7a856805c637f1d9576201b35456eeee71ed")]
    [InlineData(JpegSampling.Grayscale, 8, 69, 0, "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80", 58910,
        "444e39e6126dd3077d22c0b4f60e38d8a22611fa5721b226c5168a63b7e94423")]
    [InlineData(JpegSampling.YCbCr420, 8, 85, 33, "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80", 58010,
        "1af33b85d17d4dd24a19f98e66242eba42c7c77b7347dfa564a83d64b19feea4")]
    [InlineData(JpegSampling.Grayscale, 16, 69, 0, "3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9", 58942,
        "2a99dee8551c14e8acceb8df6adec6a2a263a9f037ec00cabe2791c96b178bfc")]
    public async Task FrozenNativePacketsMatchAcrossOwnedAndCallerBoundaries(JpegSampling sampling, int depth, int quality, int restart, string sourceHash, int length, string packetHash)
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
        var options = Options with { Limits = limits, Sampling = sampling, Quality = quality, RestartInterval = restart };
        using var first = new MemoryStream(); ImageCodec.EncodeJpeg(image, first, options);
        AssertPacket(first.ToArray(), length, packetHash); Assert.Equal(original.Length, root.Bytes);
        if ((sampling == JpegSampling.YCbCr444 && depth == 8) ||
            (sampling == JpegSampling.YCbCr420 && depth == 16) || sampling == JpegSampling.Grayscale)
        {
            // Preserve E9's successful live-token and BaselineOnly packet proof.
            using var live = new CancellationTokenSource(); using var baseline = new MemoryStream();
            ImageCodec.EncodeJpeg(image, baseline, options with { BaselineOnly = true }, live.Token);
            AssertPacket(baseline.ToArray(), length, packetHash);
        }
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
        if (sampling == JpegSampling.Grayscale)
        {
            var gray = GraySamples(normalized);
            Assert.Equal(depth == 8 ? "6b74f1d2c127f3bcccba2b377f4474b9fc7464890853d05bf058bbb38337df8d"
                : "47a685cb7e759840f7e50cff59e4254ae172aa9807b17b3f18551b9883d846a6", Hash(gray));
            const int grayStride = 518;
            var padded = Enumerable.Repeat((byte)165, grayStride * 259 + 7).ToArray();
            for (var y = 0; y < 259; y++) gray.AsSpan(y * 513, 513).CopyTo(padded.AsSpan(y * grayStride, 513));
            var beforeGray = padded.ToArray();
            // Gray input keeps the established override even when Sampling
            // is left at its default420 value. No RGBA source is needed.
            var grayOptions = options with { Sampling = JpegSampling.YCbCr420 };
            using var grayOutput = new MemoryStream();
            ImageCodec.EncodeJpeg(padded, 513, 259, JpegSampleFormat.Grayscale, grayOutput, grayStride, grayOptions);
            AssertPacket(grayOutput.ToArray(), length, packetHash);
            using var grayAsync = new ForwardOutput();
            await ImageCodec.EncodeJpegAsync(padded, 513, 259, JpegSampleFormat.Grayscale, grayAsync, grayStride, grayOptions);
            AssertPacket(grayAsync.ToArray(), length, packetHash);
            Assert.Equal(beforeGray, padded); Assert.Equal(original.Length, root.Bytes);
        }
        Assert.Equal(beforeRgb, rgb); Assert.Equal(original, OriginalPixels.Bytes(image));
        Assert.Equal(original.Length, root.Bytes); image.Dispose(); Assert.Equal(0, root.Bytes);
        AssertPacket(first.ToArray(), length, packetHash); AssertPacket(samples.ToArray(), length, packetHash);
        Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(JpegSampling.YCbCr444, 1)]
    [InlineData(JpegSampling.YCbCr444, 85)]
    [InlineData(JpegSampling.YCbCr444, 100)]
    [InlineData(JpegSampling.YCbCr422, 1)]
    [InlineData(JpegSampling.YCbCr422, 85)]
    [InlineData(JpegSampling.YCbCr422, 100)]
    [InlineData(JpegSampling.YCbCr420, 1)]
    [InlineData(JpegSampling.YCbCr420, 85)]
    [InlineData(JpegSampling.YCbCr420, 100)]
    [InlineData(JpegSampling.Grayscale, 1)]
    [InlineData(JpegSampling.Grayscale, 85)]
    [InlineData(JpegSampling.Grayscale, 100)]
    public void SmallImagesAndQualityEndpointsRemainValid(JpegSampling sampling, int quality)
    {
        using var image = OriginalPixels.Buffer(1, 1, 16, opaque: true);
        using var output = new MemoryStream();
        ImageCodec.EncodeJpeg(image, output, Options with { Quality = quality, Sampling = sampling });
        using var decoded = ImageCodec.Decode(output.ToArray());
        Assert.Equal((1, 1, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
        Assert.Equal((byte)255, decoded.GetRow8(0)[3]);
    }

    [Theory]
    [InlineData(JpegSampling.YCbCr444, 1)]
    [InlineData(JpegSampling.YCbCr444, 2)]
    [InlineData(JpegSampling.YCbCr444, 3)]
    [InlineData(JpegSampling.YCbCr444, 4)]
    [InlineData(JpegSampling.YCbCr422, 1)]
    [InlineData(JpegSampling.YCbCr422, 2)]
    [InlineData(JpegSampling.YCbCr422, 3)]
    [InlineData(JpegSampling.YCbCr422, 4)]
    [InlineData(JpegSampling.YCbCr420, 1)]
    [InlineData(JpegSampling.YCbCr420, 2)]
    [InlineData(JpegSampling.YCbCr420, 3)]
    [InlineData(JpegSampling.YCbCr420, 4)]
    [InlineData(JpegSampling.Grayscale, 1)]
    [InlineData(JpegSampling.Grayscale, 2)]
    [InlineData(JpegSampling.Grayscale, 3)]
    [InlineData(JpegSampling.Grayscale, 4)]
    public void RefusalIsTerminalAndReleasesPriorReservations(JpegSampling sampling, long atGrowth)
    {
        using var image = OriginalPixels.Buffer(17, 11, 8);
        var before = OriginalPixels.Bytes(image);
        using var root = new TestMemoryBudget(1024 * 1024) { RefuseAtGrowth = atGrowth };
        var options = Options with { Sampling = sampling, Limits = ImageLimits.Default with { MemoryBudget = root } };
        using var output = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, output, options));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length); Assert.Equal(before, OriginalPixels.Bytes(image));
        root.RefuseAtGrowth = 0; ImageCodec.EncodeJpeg(image, output, options);
        Assert.True(output.Length > 0); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(JpegSampling.YCbCr444, 0)]
    [InlineData(JpegSampling.YCbCr422, 0)]
    [InlineData(JpegSampling.YCbCr420, 0)]
    [InlineData(JpegSampling.Grayscale, 0)]
    [InlineData(JpegSampling.YCbCr420, 33)]
    public void WorkCancellationAndFailedWritesCleanUpWithoutDisposingTheCaller(JpegSampling sampling, int restart)
    {
        using var image = OriginalPixels.Buffer(17, 11, 8);
        using var root = new TestMemoryBudget(1024 * 1024);
        var options = Options with { Sampling = sampling, RestartInterval = restart, Limits = ImageLimits.Default with { MemoryBudget = root } };
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
    [InlineData(false, false, JpegSampling.YCbCr444, 0)]
    [InlineData(true, false, JpegSampling.YCbCr444, 0)]
    [InlineData(false, true, JpegSampling.YCbCr444, 0)]
    [InlineData(false, false, JpegSampling.YCbCr422, 0)]
    [InlineData(true, false, JpegSampling.YCbCr422, 0)]
    [InlineData(false, true, JpegSampling.YCbCr422, 0)]
    [InlineData(false, false, JpegSampling.YCbCr420, 0)]
    [InlineData(true, false, JpegSampling.YCbCr420, 0)]
    [InlineData(false, true, JpegSampling.YCbCr420, 0)]
    [InlineData(false, false, JpegSampling.Grayscale, 0)]
    [InlineData(true, false, JpegSampling.Grayscale, 0)]
    [InlineData(false, true, JpegSampling.Grayscale, 0)]
    [InlineData(false, false, JpegSampling.YCbCr420, 33)]
    [InlineData(true, false, JpegSampling.YCbCr420, 33)]
    [InlineData(false, true, JpegSampling.YCbCr420, 33)]
    public async Task SharedRootIncludesPendingActualAsyncUsersAndRecovers(bool cancel, bool fail, JpegSampling sampling, int restart)
    {
        using var image = OriginalPixels.Buffer(17, 11, 8);
        long required;
        using (var measure = new TestMemoryBudget(1024 * 1024))
        using (var probe = new MemoryStream())
        {
            await ImageCodec.EncodeJpegAsync(image, probe, Options with { Sampling = sampling, RestartInterval = restart, Limits = ImageLimits.Default with { MemoryBudget = measure } });
            required = measure.Peak;
        }
        using var root = new TestMemoryBudget(required);
        var options = Options with { Sampling = sampling, RestartInterval = restart, Limits = ImageLimits.Default with { MemoryBudget = root } };
        using var token = new CancellationTokenSource(); using var held = new HeldOutput(root, fail);
        var pending = ImageCodec.EncodeJpegAsync(image, held, options, token.Token).AsTask();
        await held.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            var charged = root.Bytes; Assert.True(charged > 0);
            using var competing = new MemoryStream();
            Assert.Equal(ImageLimitScope.SharedBudget,
                (await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.EncodeJpegAsync(image, competing, options).AsTask())).Scope);
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
    [InlineData(JpegSampling.Grayscale, 1, UnsupportedImageFeature.ControlMarker)]
    public void PendingReferenceFeaturesAreStructuredFailures(JpegSampling sampling, int restart, UnsupportedImageFeature feature)
    {
        using var image = OriginalPixels.Buffer(1, 1, 8, opaque: true);
        using var output = new MemoryStream();
        var error = Assert.Throws<UnsupportedImageException>(() => ImageCodec.EncodeJpeg(image, output,
            Options with { Sampling = sampling, RestartInterval = restart }));
        Assert.Equal(feature, error.Feature); Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData(18, 19, JpegSampling.YCbCr420, 556,
        "bb425e04fc82c4bf6df02bbdc9950e61cde69259a7de8e169b9bf2df1aab46dc",
        "ef83424661e0e90b54cb5d166af4dadfa851a57793f25275abc97a2ccbdaddb8")]
    [InlineData(18, 19, JpegSampling.YCbCr422, 637,
        "bb425e04fc82c4bf6df02bbdc9950e61cde69259a7de8e169b9bf2df1aab46dc",
        "41ae5d88e2c83d34fd6df06a11d0498908abd5d2cadbb5daea82c3e25f25305d")]
    [InlineData(1, 1, JpegSampling.YCbCr420, 203,
        "d1702632ae14f252a701dd314241b757eb1ef76f80a0298190be8bb130cbbe55",
        "0463f3b09d4a3ce44ee2f4be32d421657bbf4607476370d0cac90cc168fa2749")]
    [InlineData(2, 2, JpegSampling.YCbCr420, 225,
        "f892d406b3f44430aa29892a0c16055c5ad42b1ca06ab7d092512c77e0dc3aed",
        "f64cc5a933f842ca144709b336dcd358a3ad6d2e94ac4091bb67b55404d9ca34")]
    [InlineData(3, 3, JpegSampling.YCbCr420, 263,
        "38cfa8ec37dd9d8a3019fb905c99fb2d6b7339076611e0285ab765045d386f6b",
        "c45bd5b142ef0c9b73b29a6eba9d312ffb988737f5ca4248fc73dff291a1d712")]
    [InlineData(4, 4, JpegSampling.YCbCr420, 282,
        "d50ece5f42692215f80761df9533b3fc7fe27a061e5e3d20a95d95772e8a3631",
        "550236ed436bd6e35b1e7b090f5dc381682272b06828f3c8ca3a6dbaaf6ac86d")]
    public async Task PairedInputAndTinyFilterBoundariesMatchNativePackets(int width, int height, JpegSampling sampling,
        int length, string sourceHash, string packetHash)
    {
        using var image = OriginalPixels.Buffer(width, height, 8);
        var before = OriginalPixels.Bytes(image); Assert.Equal(sourceHash, Hash(before));
        using var root = new TestMemoryBudget(1024 * 1024);
        var options = Options with { Sampling = sampling, Limits = ImageLimits.Default with { MemoryBudget = root } };
        using var output = new ForwardOutput(); ImageCodec.EncodeJpeg(image, output, options);
        AssertPacket(output.ToArray(), length, packetHash); Assert.Equal(0, root.Bytes);
        var normalized = JpegQualityCorpus.Reference(before, 8, 0x204060);
        var stride = width * 3 + 5; var rgb = Enumerable.Repeat((byte)165, stride * height + 7).ToArray();
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            normalized.AsSpan((y * width + x) * 4, 3).CopyTo(rgb.AsSpan(y * stride + x * 3, 3));
        var originalRgb = rgb.ToArray(); using var callerOutput = new MemoryStream();
        await ImageCodec.EncodeJpegAsync(rgb, width, height, JpegSampleFormat.Rgb, callerOutput, stride, options);
        AssertPacket(callerOutput.ToArray(), length, packetHash); Assert.Equal(0, root.Bytes);
        Assert.Equal(originalRgb, rgb); Assert.Equal(before, OriginalPixels.Bytes(image));
        AssertPacket(output.ToArray(), length, packetHash); Assert.False(root.IsDisposed);
        using var decoded = ImageCodec.Decode(output.ToArray());
        Assert.Equal((width, height, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
    }

    [Fact]
    public async Task Gray16NormalizationMatchesIndependentBorrowedSamples()
    {
        using var image = OriginalPixels.Buffer(17, 11, 16);
        var before = OriginalPixels.Bytes(image);
        var normalized = JpegQualityCorpus.Reference(before, 16, 0x204060);
        var gray = GraySamples(normalized); var beforeGray = gray.ToArray();
        using var root = new TestMemoryBudget(1024 * 1024);
        var options = Options with { Sampling = JpegSampling.Grayscale, Quality = 69,
            Limits = ImageLimits.Default with { MemoryBudget = root } };
        using var fromImage = new MemoryStream(); ImageCodec.EncodeJpeg(image, fromImage, options);
        using var fromSamples = new MemoryStream();
        await ImageCodec.EncodeJpegAsync(gray, 17, 11, JpegSampleFormat.Grayscale, fromSamples, options: options);
        Assert.Equal(fromImage.ToArray(), fromSamples.ToArray()); Assert.Equal(0, root.Bytes);
        Assert.Equal(before, OriginalPixels.Bytes(image)); Assert.Equal(beforeGray, gray); Assert.False(root.IsDisposed);
        using var decoded = ImageCodec.Decode(fromImage.ToArray());
        Assert.Equal((17, 11, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
    }

    private static byte[] GraySamples(byte[] rgba)
    {
        var gray = new byte[rgba.Length / 4];
        for (var i = 0; i < gray.Length; i++)
            gray[i] = (byte)Math.Floor((0.299 * rgba[i * 4] + 0.587 * rgba[i * 4 + 1]) + 0.114 * rgba[i * 4 + 2] + 0.5);
        return gray;
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
