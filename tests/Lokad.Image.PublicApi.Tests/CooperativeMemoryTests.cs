using System.Text;
using Lokad.Image.Tooling;
using TestMemory = Lokad.Image.Tooling.TestMemoryBudget;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CooperativeMemoryTests
{
    [Fact]
    public void ImagesKeepSiblingOwnersAndCloningAdmitsTheReplacementPeak()
    {
        using var root = new TestMemory(8192);
        var limits = new ImageLimits { MemoryBudget = root };
        var first = ImageBuffer.Create(32, 32, 8, limits);
        first.GetRow8(0)[0] = 123;
        using var second = first.Clone(limits);
        Assert.Equal(8192, root.Bytes);
        var attempts = root.Attempts;
        var error = Assert.Throws<ImageLimitException>(() => first.Clone(limits));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(attempts + 1, root.Attempts); // Refusal is terminal; no retry/fallback.
        Assert.Equal(8192, root.Bytes);
        first.Dispose(); first.Dispose();
        Assert.Equal(4096, root.Bytes);
        using (var third = second.Clone(limits))
        {
            Assert.Equal(8192, root.Bytes);
            third.GetRow8(0)[0] = 42;
            Assert.Equal(123, second.GetRow8(0)[0]);
        }
        second.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.Equal(8192, root.Peak);
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public async Task ConcurrentOperationsShareOneAllowance()
    {
        using var root = new TestMemory(1024);
        var limits = new ImageLimits { MemoryBudget = root };
        var outcomes = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        {
            try { return ImageBuffer.Create(16, 16, 8, limits); }
            catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget) { return null; }
        })));
        var survivor = Assert.Single(outcomes, i => i is not null)!;
        Assert.Equal(1024, root.Bytes);
        survivor.Dispose();
        using var subsequent = ImageBuffer.Create(16, 16, 8, limits);
        Assert.Equal(1024, root.Bytes);
    }

    [Fact]
    public void SuppliedScopesAndReservationsAreBorrowedAndHostSemanticsArePreserved()
    {
        using var root = new TestMemory(1000);
        using var parent = root.CreateScope();
        Assert.Equal(0, root.Bytes);
        Assert.True(parent.TryReserve(0, out var external));
        using (external)
        {
            Assert.True(external.TryResize(600));
            Assert.False(external.TryResize(1001));
            Assert.Equal(600, external.Bytes);
            Assert.Throws<ArgumentOutOfRangeException>(() => external.TryResize(-1));
            var limits = new ImageLimits { MemoryBudget = parent };
            using (var image = ImageBuffer.Create(10, 10, 8, limits)) Assert.Equal(1000, root.Bytes);
            Assert.Equal(600, root.Bytes);
            Assert.True(external.TryResize(0));
            using var second = ImageBuffer.Create(10, 10, 8, limits);
            Assert.Equal(400, root.Bytes);
        }
        Assert.Equal(0, root.Bytes);
        using var retained = ImageBuffer.Create(1, 1, 8, new() { MemoryBudget = parent });
        retained.Dispose();
        parent.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.Equal(0, external.Bytes);
        Assert.Throws<ObjectDisposedException>(() => ImageBuffer.Create(1, 1, 8, new() { MemoryBudget = parent }));
        Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.Png16)]
    [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.JpegProgressive420)]
    [InlineData(MetadataProfile.WebpLosslessAlpha)] [InlineData(MetadataProfile.WebpLossyAlpha)]
    public async Task CodecsReleaseTemporaryCapacityAndRetainOnlyIndependentPixels(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 19, 13);
        using var expected = ImageCodec.Decode(fixture.Encoded);
        using var root = new TestMemory(32 * 1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        var options = new DecodeOptions { Limits = limits };
        using (var image = ImageCodec.Decode(fixture.Encoded, options))
        {
            Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(image));
            Assert.Equal(19 * 13 * 4 * (image.BitDepth / 8), root.Bytes);
            Assert.Equal(ImageCodec.Identify(fixture.Encoded), ImageCodec.Identify(fixture.Encoded, limits));
            using var stream = new MemoryStream(fixture.Encoded);
            using var second = await ImageCodec.DecodeAsync(stream, options);
            Assert.Equal(2 * 19 * 13 * 4 * (image.BitDepth / 8), root.Bytes);
            Assert.Equal(OriginalPixels.Bytes(image), OriginalPixels.Bytes(second));
            Assert.True(stream.CanRead);
        }
        Assert.Equal(0, root.Bytes);
        Assert.True(root.Peak > 19 * 13 * 8);
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CallerOwnedJpegSamplesAreNotChargedAsCopies()
    {
        var packet = AuthoredJpeg.GrayscaleDcGrid(8, 8, false);
        var destination = new byte[1024 * 1024];
        using var root = new TestMemory(256 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        var info = ImageCodec.DecodeJpeg(packet, destination, JpegSampleFormat.Grayscale,
            options: new() { Limits = limits });
        Assert.Equal((8, 8), (info.Width, info.Height));
        Assert.Equal(0, root.Bytes);
        using var output = new MemoryStream();
        ImageCodec.EncodeJpeg(destination, 8, 8, JpegSampleFormat.Grayscale, output,
            options: new() { Limits = limits });
        Assert.Equal(0, root.Bytes);
        Assert.True(root.Peak < destination.Length);
    }

    [Fact]
    public void FontAndPreparedLayoutHaveIndependentExplicitStorageLifetimes()
    {
        var data = AuthoredTrueType.Build();
        using var root = new TestMemory(1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        var font = TrueTypeFont.Load(data, limits);
        Assert.Equal(font.StorageBytes, root.Bytes);
        var fontBytes = root.Bytes;
        var layout = TextLayout.Create(font, "AV A\u0301\ng", new() { Limits = limits });
        var layoutBytes = root.Bytes - fontBytes;
        Assert.True(layoutBytes > 0);
        font.Dispose(); font.Dispose();
        Assert.Equal(layoutBytes, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => font.GetGlyphIndex(new Rune('A')));
        using (var canvas = ImageBuffer.Create(80, 50, 8, limits))
            ImageDrawing.DrawText(canvas, layout, 0, 0, RgbaColor.From8(12, 34, 56), limits);
        Assert.Equal(layoutBytes, root.Bytes);
        layout.Dispose(); layout.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => layout.GetGlyph(0));
        using var next = TrueTypeFont.Load(data, limits);
        Assert.Equal(next.StorageBytes, root.Bytes);
    }

    [Theory]
    [InlineData(ResizeFilter.Nearest)] [InlineData(ResizeFilter.Bilinear)] [InlineData(ResizeFilter.Bicubic)]
    public void TransformFailureReleasesUnpublishedResultAndAllowsReuse(ResizeFilter filter)
    {
        using var root = new TestMemory(1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        using var input = ImageBuffer.Create(31, 19, 16, limits);
        var retained = root.Bytes;
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(input, 41, 27, filter,
            limits with { MaxWork = 1 }));
        Assert.Equal(retained, root.Bytes);
        using (var result = ImageTransforms.Resize(input, 41, 27, filter, limits))
            Assert.Equal(retained + 41 * 27 * 8, root.Bytes);
        Assert.Equal(retained, root.Bytes);
        using var cropped = ImageTransforms.Crop(input, 0, 0, 11, 7, limits);
        Assert.Equal(retained + 11 * 7 * 8, root.Bytes);
    }

    [Fact]
    public void RefusalDuringTemporaryGrowthDoesNotRetryAndReleasesEveryPrivateOwner()
    {
        var packet = OriginalExif.Create(MetadataProfile.Png8, 53, 31).Encoded;
        using var root = new TestMemory(9000);
        var limits = new ImageLimits { MemoryBudget = root };
        using var input = new MemoryStream(packet);
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(input, new() { Limits = limits }));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(1, root.Refusals);
        Assert.Equal(0, root.Bytes);
        Assert.True(input.CanRead);
        using var subsequent = ImageBuffer.Create(10, 10, 8, limits);
        Assert.Equal(400, root.Bytes);
    }

    [Fact]
    public async Task InputGrowthChargesOldAndNewCapacityTogetherBeforeReplacing()
    {
        using var root = new TestMemory(8192 + 256 + 512 - 1);
        var limits = new ImageLimits { MemoryBudget = root };
        using var input = new ChunkInput(new byte[257]);
        var error = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.IdentifyAsync(input, limits).AsTask());
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(8192 + 256, root.Peak);
        Assert.Equal(1, root.Refusals);
        Assert.Equal(0, root.Bytes);
        Assert.True(input.CanRead);
    }

    [Fact]
    public void CancellationAfterResultAdmissionReleasesUnpublishedPixels()
    {
        using var root = new TestMemory(8192);
        var limits = new ImageLimits { MemoryBudget = root };
        using var source = ImageBuffer.Create(32, 32, 8, limits);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes > 4096) cancellation.Cancel(); };
        var error = Assert.ThrowsAny<OperationCanceledException>(() => source.Clone(limits, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(4096, root.Bytes);
        root.Admitted = null;
        using var subsequent = source.Clone(limits);
        Assert.Equal(8192, root.Bytes);
    }

    [Fact]
    public void FontValidationAndLayoutRefusalReleaseUnpublishedStorage()
    {
        var data = AuthoredTrueType.Build();
        using var root = new TestMemory(data.Length + 256 + 1000);
        var limits = new ImageLimits { MemoryBudget = root };
        Assert.Throws<UnsupportedImageException>(() => TrueTypeFont.Load(new byte[100], limits));
        Assert.Equal(0, root.Bytes);
        using (var font = TrueTypeFont.Load(data, limits))
        {
            Assert.Throws<ImageLimitException>(() => TextLayout.Create(font, "AV", new() { Limits = limits }));
            Assert.Equal(font.StorageBytes, root.Bytes);
        }
        Assert.Equal(0, root.Bytes);
        using var subsequent = ImageBuffer.Create(1, 1, 8, limits);
        Assert.Equal(4, root.Bytes);
    }

    [Theory]
    [InlineData(ResizeFilter.Bilinear)] [InlineData(ResizeFilter.Bicubic)]
    public void WorkspaceRefusalReleasesTheAlreadyAllocatedResizeResult(ResizeFilter filter)
    {
        const int sourceBytes = 31 * 19 * 8, resultBytes = 41 * 27 * 8;
        using var root = new TestMemory(sourceBytes + resultBytes);
        var limits = new ImageLimits { MemoryBudget = root };
        using var input = ImageBuffer.Create(31, 19, 16, limits);
        var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(input, 41, 27, filter, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(sourceBytes + resultBytes, root.Peak);
        Assert.Equal(sourceBytes, root.Bytes);
        Assert.Equal(1, root.Refusals);
        using var subsequent = input.Clone(limits);
        Assert.Equal(2 * sourceBytes, root.Bytes);
    }

    [Fact]
    public void NestedOutputCallbacksShareTheLiveRootAndDoNotDisposeCallerOwners()
    {
        using var root = new TestMemory(16 * 1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        using var input = ImageBuffer.Create(19, 13, 8, limits);
        var retained = root.Bytes;
        long nestedBytes = 0;
        using var output = new CallbackOutput(() =>
        {
            var before = root.Bytes;
            Assert.True(before > retained);
            using var child = ImageBuffer.Create(16, 16, 8, limits);
            nestedBytes = root.Bytes;
            Assert.Equal(before + 1024, nestedBytes);
        });
        ImageCodec.EncodePng(input, output, new() { Limits = limits });
        Assert.True(nestedBytes > retained + 1024);
        Assert.Equal(retained, root.Bytes);
        Assert.True(output.CanWrite);
        using var result = ImageCodec.Decode(output.ToArray(), new() { Limits = limits });
        Assert.Equal(OriginalPixels.Bytes(input), OriginalPixels.Bytes(result));
    }

    [Fact]
    public void NestedOrientationAndLegacyWorkAdmissionPreserveBothIndependentBudgets()
    {
        var fixture = OriginalExif.Create(MetadataProfile.Png16, 19, 13);
        var packet = OriginalExif.Attach(fixture, ImageOrientation.Rotate90, false);
        using var root = new TestMemory(32 * 1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root, MaxWork = 2_000_000, MaxWorkingBytes = 4 * 1024 * 1024 };
        var legacy = new ImageBudget(8 * 1024 * 1024, 10_000_000);
        using (var result = legacy.Decode(packet, new() { Limits = limits, ApplyOrientation = true }))
        {
            Assert.Equal((13, 19), (result.Image.Width, result.Image.Height));
            Assert.Equal(19 * 13 * 8, root.Bytes);
            Assert.Equal(legacy.MaxWorkingBytes - root.Bytes, legacy.RemainingWorkingBytes);
        }
        Assert.Equal(0, root.Bytes);
        Assert.Equal(legacy.MaxWorkingBytes, legacy.RemainingWorkingBytes);
        Assert.True(legacy.ConsumedWork > 0);
    }

    [Theory]
    [InlineData("png-decode")] [InlineData("jpeg-decode")]
    [InlineData("vp8l-decode")] [InlineData("vp8-alpha-decode")]
    [InlineData("png-encode")] [InlineData("jpeg-encode")]
    [InlineData("vp8l-encode")] [InlineData("vp8-encode")]
    [InlineData("font")] [InlineData("layout")] [InlineData("resize")] [InlineData("headers")]
    public void EverySubstantialAdmissionBoundaryHasTerminalRefusalAndCompleteCleanup(string operation)
    {
        using var source = ImageBuffer.Create(19, 13, 8);
        using var font = TrueTypeFont.Load(AuthoredTrueType.Build());
        var png = OriginalExif.Create(MetadataProfile.Png8, 19, 13).Encoded;
        var jpeg = AuthoredJpeg.GrayscaleDcGrid(8, 8, false);
        var vp8l = OriginalExif.Create(MetadataProfile.WebpLosslessAlpha, 19, 13).Encoded;
        var vp8 = OriginalExif.Create(MetadataProfile.WebpLossyAlpha, 19, 13).Encoded;
        void Run(ImageLimits limits)
        {
            using var output = new MemoryStream();
            switch (operation)
            {
                case "png-decode": using (ImageCodec.Decode(png, new() { Limits = limits })) { } break;
                case "jpeg-decode": using (ImageCodec.Decode(jpeg, new() { Limits = limits })) { } break;
                case "vp8l-decode": using (ImageCodec.Decode(vp8l, new() { Limits = limits })) { } break;
                case "vp8-alpha-decode": using (ImageCodec.Decode(vp8, new() { Limits = limits })) { } break;
                case "png-encode": ImageCodec.EncodePng(source, output, new() { Limits = limits }); break;
                case "jpeg-encode": ImageCodec.EncodeJpeg(source, output, new() { Limits = limits, BackgroundRgb = 0 }); break;
                case "vp8l-encode": ImageCodec.EncodeWebp(source, output, new() { Limits = limits }); break;
                case "vp8-encode": ImageCodec.EncodeWebp(source, output, new() { Limits = limits, Lossless = false, Effort = 0 }); break;
                case "font": using (TrueTypeFont.Load(AuthoredTrueType.Build(), limits)) { } break;
                case "layout": using (TextLayout.Create(font, "AV A\u0301\ng", new() { Limits = limits })) { } break;
                case "resize": using (ImageTransforms.Resize(source, 31, 21, ResizeFilter.Bicubic, limits)) { } break;
                case "headers": using (var input = new MemoryStream(png)) ImageCodec.InspectHeaders(input, limits); break;
                default: throw new InvalidOperationException();
            }
        }
        using var baseline = new TestMemory(32 * 1024 * 1024);
        Run(new() { MemoryBudget = baseline });
        Assert.Equal(0, baseline.Bytes);
        Assert.InRange(baseline.GrowthAttempts, 1, 200); // Fixed tiny fixtures; no workload grid.
        for (var failure = 1; failure <= baseline.GrowthAttempts; failure++)
        {
            using var root = new TestMemory(32 * 1024 * 1024) { RefuseAtGrowth = failure };
            var limits = new ImageLimits { MemoryBudget = root };
            var error = Assert.Throws<ImageLimitException>(() => Run(limits));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
            Assert.Equal(1, root.Refusals);
            Assert.Equal(failure, root.GrowthAttempts);
            Assert.Equal(0, root.Bytes);
            root.RefuseAtGrowth = 0;
            Run(limits);
            Assert.Equal(0, root.Bytes);
            Assert.False(root.IsDisposed);
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task CanceledIoStaysChargedUntilTheActualBorrowerFinishes(bool writing)
    {
        using var root = new TestMemory(32 * 1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        using var cancellation = new CancellationTokenSource();
        using var stream = new HeldIo(root);
        using var image = ImageBuffer.Create(31, 19, 8, limits);
        var retained = root.Bytes;
        var pending = writing
            ? ImageCodec.EncodePngAsync(image, stream, new() { Limits = limits }, cancellation.Token).AsTask()
            : ImageCodec.IdentifyAsync(stream, limits, cancellation.Token).AsTask();
        await stream.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var duringIo = root.Bytes;
        Assert.True(duringIo > retained);
        cancellation.Cancel();
        Assert.False(pending.IsCompleted);
        Assert.Equal(duringIo, root.Bytes);
        stream.Resume.SetResult();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.True(stream.SawLiveCharge);
        Assert.Equal(retained, root.Bytes);
        Assert.False(stream.WasDisposed);
        using var subsequent = image.Clone(limits);
        Assert.Equal(2 * retained, root.Bytes);
    }

    private sealed class HeldIo(TestMemory root) : Stream
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool SawLiveCharge, WasDisposed;
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Entered.SetResult(); await Resume.Task.ConfigureAwait(false);
            SawLiveCharge = root.Bytes >= buffer.Length;
            cancellationToken.ThrowIfCancellationRequested(); return 0;
        }
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Entered.SetResult(); await Resume.Task.ConfigureAwait(false);
            SawLiveCharge = root.Bytes >= buffer.Length;
            cancellationToken.ThrowIfCancellationRequested();
        }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] b, int o, int c) => throw new InvalidOperationException();
        public override void Write(byte[] b, int o, int c) => throw new InvalidOperationException();
        public override void Flush() => throw new InvalidOperationException();
        public override long Seek(long o, SeekOrigin s) => throw new NotSupportedException();
        public override void SetLength(long v) => throw new NotSupportedException();
    }

    private sealed class ChunkInput(byte[] data) : MemoryStream(data)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => base.ReadAsync(buffer[..Math.Min(1, buffer.Length)], cancellationToken);
    }

    private sealed class CallbackOutput(Action callback) : MemoryStream
    {
        private bool invoked;
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (!invoked) { invoked = true; callback(); }
            base.Write(buffer);
        }
    }

}
