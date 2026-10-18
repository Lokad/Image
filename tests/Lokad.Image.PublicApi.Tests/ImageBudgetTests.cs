using System.Runtime.CompilerServices;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ImageBudgetTests
{
    private static readonly ImageLimits Limits = new() { MaxWorkingBytes = 4 * 1024 * 1024, MaxWork = 2_000_000 };

    public static IEnumerable<object[]> DecodeProfiles() => Enum.GetValues<MetadataProfile>().Select(p => new object[] { p });

    [Theory]
    [MemberData(nameof(DecodeProfiles))]
    public void CodecResultsAndOrientationKeepExactPixelsAndIndependentReservations(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 9, 7);
        var data = OriginalExif.Attach(fixture, ImageOrientation.Rotate90, false);
        var original = data.ToArray();
        var options = new DecodeOptions { Limits = Limits, ApplyOrientation = true };
        using var expected = ImageCodec.Decode(data, options);
        var expectedBytes = OriginalPixels.Bytes(expected);
        var budget = new ImageBudget(Limits.MaxWorkingBytes + expectedBytes.Length, Limits.MaxWork * 10);
        var first = budget.Decode(data, options);
        try
        {
            using var input = new MemoryStream(data);
            using var second = budget.Decode(input, options);
            Assert.True(input.CanRead);
            Assert.Equal(ImageOrientation.Normal, second.Image.Orientation);
            Assert.Equal(expectedBytes, OriginalPixels.Bytes(first.Image));
            Assert.Equal(expectedBytes, OriginalPixels.Bytes(second.Image));
            Assert.Equal(budget.MaxWorkingBytes - 2 * expectedBytes.Length, budget.RemainingWorkingBytes);
            var failure = Assert.Throws<ImageLimitException>(() => budget.Decode(data, options));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (failure.Limit, failure.Scope));
            first.Dispose(); first.Dispose();
            Assert.Throws<ObjectDisposedException>(() => first.Image);
            Assert.Equal(expectedBytes, OriginalPixels.Bytes(second.Image));
            using var third = budget.Decode(data, options);
            Assert.NotSame(second.Image, third.Image);
            Assert.Equal(expectedBytes, OriginalPixels.Bytes(third.Image));
            if (third.Image.BitDepth == 8) third.Image.GetRow8(0)[0] ^= 1;
            else third.Image.GetRow16(0)[0] ^= 1;
            Assert.Equal(expectedBytes, OriginalPixels.Bytes(second.Image));
        }
        finally { first.Dispose(); }
        Assert.Equal(original, data);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork);
    }

    [Fact]
    public void StreamChargesAddOnlyAcquisitionWorkAndIdentifyReleasesAllMemory()
    {
        var data = AuthoredJpeg.GrayscaleDcGrid(8, 8, false);
        var spanBudget = new ImageBudget(Limits.MaxWorkingBytes, Limits.MaxWork);
        var streamBudget = new ImageBudget(Limits.MaxWorkingBytes, Limits.MaxWork);
        Assert.Equal(ImageCodec.Identify(data), spanBudget.Identify(data, Limits));
        using var input = new MemoryStream(data);
        Assert.Equal(ImageCodec.Identify(data), streamBudget.Identify(input, Limits));
        Assert.Equal(data.Length, streamBudget.ConsumedWork - spanBudget.ConsumedWork);
        Assert.Equal(spanBudget.MaxWorkingBytes, spanBudget.RemainingWorkingBytes);
        Assert.Equal(streamBudget.MaxWorkingBytes, streamBudget.RemainingWorkingBytes);
        Assert.True(input.CanRead);
    }

    [Theory]
    [InlineData(ImageFormat.Png, false)]
    [InlineData(ImageFormat.Jpeg, false)]
    [InlineData(ImageFormat.WebP, false)]
    [InlineData(ImageFormat.WebP, true)]
    public void EncodingReusesTheSameCompleteCodecAndLeavesCallerOwnersAlive(ImageFormat format, bool lossless)
    {
        using var image = ImageBuffer.Create(9, 7, 16);
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
            new ushort[] { (ushort)(x * 8000), (ushort)(y * 10000), 50123, 65535 }.CopyTo(image.GetRow16(y).Slice(x * 4));
        var original = OriginalPixels.Bytes(image);
        var budget = new ImageBudget(Limits.MaxWorkingBytes, Limits.MaxWork * 2);
        using var expected = new MemoryStream(); using var actual = new MemoryStream();
        Encode(format, null, image, expected, lossless); Encode(format, budget, image, actual, lossless);
        Assert.Equal(expected.ToArray(), actual.ToArray());
        Assert.True(actual.CanWrite);
        Assert.Equal(original, OriginalPixels.Bytes(image));
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.True(budget.ConsumedWork > 0);
        Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork);

        using var failing = new FailingWriteStream();
        var failure = Assert.Throws<IOException>(() => Encode(format, budget, image, failing, lossless));
        Assert.Same(failing.Failure, failure); Assert.False(failing.Disposed);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork);
        Assert.Equal(original, OriginalPixels.Bytes(image));
    }

    [Fact]
    public void ExhaustionIsAtomicAndCanBeDistinguishedFromPerOperationLimits()
    {
        var data = AuthoredJpeg.GrayscaleDcGrid(8, 8, false);
        var options = new DecodeOptions { Limits = Limits with { MaxWork = 1 } };
        var budget = new ImageBudget(Limits.MaxWorkingBytes, 1);
        var failure = Assert.Throws<ImageLimitException>(() => budget.Decode(data, options));
        Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (failure.Limit, failure.Scope));
        Assert.Equal(1, budget.ConsumedWork); Assert.Equal(0, budget.RemainingWork);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        var shared = Assert.Throws<ImageLimitException>(() => budget.Decode(data, options));
        Assert.Equal((ImageLimitKind.Work, ImageLimitScope.SharedBudget), (shared.Limit, shared.Scope));
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);

        var noMemory = new ImageBudget(Limits.MaxWorkingBytes - 1, Limits.MaxWork);
        using var input = new MemoryStream(data);
        Assert.Equal(ImageLimitScope.SharedBudget, Assert.Throws<ImageLimitException>(() => noMemory.Identify(input, Limits)).Scope);
        Assert.Equal(0, input.Position); Assert.Equal(0, noMemory.ConsumedWork);
        Assert.Equal(Limits.MaxWork, noMemory.RemainingWork);
        var unsupported = new ImageBudget(Limits.MaxWorkingBytes, Limits.MaxWork);
        var feature = Assert.Throws<UnsupportedImageException>(() => unsupported.Decode("GIF89a"u8, new() { Limits = Limits }));
        Assert.Equal(UnsupportedImageFeature.Format, feature.Feature);
        Assert.Equal(unsupported.MaxWorkingBytes, unsupported.RemainingWorkingBytes);
        Assert.Equal(unsupported.MaxWork, unsupported.RemainingWork);
    }

    [Fact]
    public void MalformedAndFailingStreamInputsReleaseAdmissionWithoutReplacingTheFailure()
    {
        var data = AuthoredJpeg.GrayscaleDcGrid(8, 8, false)[..^2];
        var original = data.ToArray();
        var budget = new ImageBudget(Limits.MaxWorkingBytes, Limits.MaxWork * 2);
        var expected = Assert.Throws<ImageDataException>(() => ImageCodec.Decode(data, new() { Limits = Limits }));
        var actual = Assert.Throws<ImageDataException>(() => budget.Decode(data, new() { Limits = Limits }));
        Assert.Equal(expected.Message, actual.Message); Assert.True(budget.ConsumedWork > 0);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork);
        using var input = new FailingReadStream();
        Assert.Same(input.Failure, Assert.Throws<IOException>(() => budget.Decode(input, new() { Limits = Limits })));
        Assert.True(input.CanRead);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork);
        Assert.Equal(original, data);
    }

    [Fact]
    public void CallerReservationsComposeWithCodecsWithoutOverflowOrDoubleRelease()
    {
        var data = AuthoredJpeg.GrayscaleDcGrid(8, 8, false);
        var budget = new ImageBudget(Limits.MaxWorkingBytes + 7, Limits.MaxWork + 3);
        var caller = budget.Reserve(7, 3);
        Assert.Equal(Limits.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.Equal(Limits.MaxWork, budget.RemainingWork);
        budget.Identify(data, Limits);
        caller.Dispose(); caller.Dispose();
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.True(budget.ConsumedWork > 3);
        var large = new ImageBudget(long.MaxValue, long.MaxValue);
        using (large.Reserve(long.MaxValue, long.MaxValue))
        {
            Assert.Equal(0, large.RemainingWorkingBytes); Assert.Equal(0, large.RemainingWork);
            Assert.Equal(ImageLimitKind.WorkingBytes, Assert.Throws<ImageLimitException>(() => large.Reserve(1)).Limit);
            Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => large.Reserve(0, 1)).Limit);
        }
        Assert.Equal(long.MaxValue, large.RemainingWorkingBytes);
        Assert.Equal(long.MaxValue, large.ConsumedWork);
        Assert.Throws<ArgumentOutOfRangeException>(() => large.Reserve(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => large.Reserve(0, -1));
    }

    [Fact]
    public async Task InFlightCallsReserveMemoryAndWorkBeforeReadingAndRefundAfterCancellation()
    {
        var data = AuthoredJpeg.GrayscaleDcGrid(8, 8, false);
        var budget = new ImageBudget(Limits.MaxWorkingBytes, Limits.MaxWork);
        using var cancellation = new CancellationTokenSource();
        using var input = new GatedReadStream(data);
        var call = Task.Run(() => budget.Decode(input, new() { Limits = Limits }, cancellation.Token));
        try
        {
            Assert.True(input.Entered.Wait(TimeSpan.FromSeconds(10)));
            Assert.Equal(0, budget.RemainingWorkingBytes); Assert.Equal(0, budget.RemainingWork);
            Assert.Equal(ImageLimitKind.WorkingBytes, Assert.Throws<ImageLimitException>(() => budget.Identify(data, Limits)).Limit);
            Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => budget.Reserve(0, 1)).Limit);
            cancellation.Cancel();
        }
        finally { input.Continue.Set(); }
        var failure = await Assert.ThrowsAsync<OperationCanceledException>(async () => { using var result = await call; });
        Assert.Equal(cancellation.Token, failure.CancellationToken);
        Assert.True(input.CanRead); Assert.Equal(0, budget.ConsumedWork);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        Assert.Equal(budget.MaxWork, budget.RemainingWork);
    }

    [Fact]
    public void PreCanceledAndInvalidInputsNeverClaimSharedAdmission()
    {
        var budget = new ImageBudget(0, 0);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var failure = Assert.Throws<OperationCanceledException>(() => budget.Decode("GIF89a"u8,
            new() { Limits = Limits }, cancellation.Token));
        Assert.Equal(cancellation.Token, failure.CancellationToken);
        Assert.Throws<OperationCanceledException>(() => budget.Reserve(1, 1, cancellation.Token));
        Assert.Throws<ArgumentOutOfRangeException>(() => budget.Identify("GIF89a"u8, Limits with { MaxWork = 0 }));
        Assert.Throws<ArgumentNullException>(() => budget.Identify((Stream)null!, Limits));
        Assert.Equal(0, budget.ConsumedWork);
    }

    [Fact]
    public void ForgottenWrapperKeepsChargeWhileItsImageSurvivesAndReleasesAfterCollection()
    {
        var budget = new ImageBudget(Limits.MaxWorkingBytes, Limits.MaxWork);
        KeepEscapedImageAlive(budget);
        for (var i = 0; i < 4 && budget.RemainingWorkingBytes != budget.MaxWorkingBytes; i++)
        {
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        }
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ImageBuffer ForgetWrapper(ImageBudget budget)
        => budget.Decode(AuthoredJpeg.GrayscaleDcGrid(8, 8, false), new() { Limits = Limits }).Image;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void KeepEscapedImageAlive(ImageBudget budget)
    {
        var image = ForgetWrapper(budget);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Assert.Equal(budget.MaxWorkingBytes - 8 * 8 * 4, budget.RemainingWorkingBytes);
        image.Dispose();
        Assert.Equal(budget.MaxWorkingBytes - 8 * 8 * 4, budget.RemainingWorkingBytes);
        GC.KeepAlive(image);
    }

    private static void Encode(ImageFormat format, ImageBudget? budget, ImageBuffer image, Stream output, bool lossless)
    {
        switch (format)
        {
            case ImageFormat.Png:
                var png = new PngEncodeOptions { Limits = Limits };
                if (budget is null) ImageCodec.EncodePng(image, output, png); else budget.EncodePng(image, output, png); break;
            case ImageFormat.Jpeg:
                var jpeg = new JpegEncodeOptions { Limits = Limits, Quality = 86, Sampling = JpegSampling.YCbCr420 };
                if (budget is null) ImageCodec.EncodeJpeg(image, output, jpeg); else budget.EncodeJpeg(image, output, jpeg); break;
            case ImageFormat.WebP:
                var webp = new WebpEncodeOptions { Limits = Limits, Lossless = lossless, Quality = 86, Effort = 4 };
                if (budget is null) ImageCodec.EncodeWebp(image, output, webp); else budget.EncodeWebp(image, output, webp); break;
        }
    }

    private sealed class GatedReadStream(byte[] input) : MemoryStream(input)
    {
        internal readonly ManualResetEventSlim Entered = new(), Continue = new();
        public override int Read(Span<byte> buffer)
        {
            Entered.Set();
            if (!Continue.Wait(TimeSpan.FromSeconds(10))) throw new IOException("Test reader was not released.");
            return base.Read(buffer);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { Entered.Dispose(); Continue.Dispose(); }
            base.Dispose(disposing);
        }
    }

    private sealed class FailingWriteStream : MemoryStream
    {
        internal readonly IOException Failure = new("Authored output failure");
        internal bool Disposed;
        public override void Write(ReadOnlySpan<byte> buffer) => throw Failure;
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }

    private sealed class FailingReadStream : MemoryStream
    {
        internal readonly IOException Failure = new("Authored input failure");
        public override int Read(Span<byte> buffer) => throw Failure;
    }
}
