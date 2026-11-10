using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class BicubicResizePlanTests
{
    private static bool PreparedSupported => Avx512F.IsSupported && Avx512DQ.IsSupported
        && Avx2.IsSupported && Sse41.IsSupported;

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void PreparedReusePreservesExactAlphaAndColorHalves(int depth)
    {
        using var source = HalfSource(depth);
        var input = OriginalPixels.Bytes(source);
        Assert.True(ExactResizeReference.AlphaIsHalf(input, 4, 4, depth, 2, 2, 0, 0, ResizeFilter.Bicubic));
        Assert.True(ExactResizeReference.SampleIsHalf(input, 4, 4, depth, 2, 2, 0, 0, 0, ResizeFilter.Bicubic));
        using var plan = BicubicResizePlan.Create(4, 4, 2, 2, depth);
        if (PreparedSupported) Assert.True(plan.StorageBytes > 0);
        var expected = ExactResizeReference.Resize(input, 4, 4, depth, 2, 2, ResizeFilter.Bicubic);
        for (var i = 0; i < 2; i++)
        {
            using var result = plan.Resize(source);
            Assert.Equal(expected, OriginalPixels.Bytes(result));
            if (depth == 8) Assert.Equal(new byte[] { 1, 255, 128, 1 }, result.GetRow8(0)[..4].ToArray());
            else Assert.Equal(new ushort[] { 1, 65535, 32768, 1 }, result.GetRow16(0)[..4].ToArray());
        }
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void CancellationDuringExactRepairDoesNotReuseFailedRows(int depth)
    {
        using var source = HalfSource(depth);
        using var root = new TestMemoryBudget(1 << 20); var limits = new ImageLimits { MemoryBudget = root };
        using var plan = BicubicResizePlan.Create(4, 4, 2, 2, depth, limits);
        var retained = root.Bytes; var outputBytes = 2 * 2 * 4 * (depth / 8);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes > retained + outputBytes) cancellation.Cancel(); };
        try { Assert.Throws<OperationCanceledException>(() => plan.Resize(source, cancellation.Token)); }
        finally { root.Admitted = null; }
        Assert.True(cancellation.IsCancellationRequested); Assert.Equal(retained, root.Bytes);
        for (var y = 0; y < source.Height; y++) Row(source, y).Fill(255);
        using var recovered = plan.Resize(source);
        Assert.All(OriginalPixels.Bytes(recovered), value => Assert.Equal(255, value));
        recovered.Dispose(); plan.Dispose();
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8, 19, 13, 11, 7)] [InlineData(16, 19, 13, 11, 7)]
    [InlineData(8, 37, 23, 7, 5)] [InlineData(16, 37, 23, 7, 5)]
    [InlineData(8, 257, 17, 3, 2)] [InlineData(16, 257, 17, 3, 2)]
    [InlineData(8, 7, 5, 13, 11)] [InlineData(16, 7, 5, 13, 11)]
    [InlineData(8, 7, 11, 13, 5)] [InlineData(16, 7, 11, 13, 5)]
    [InlineData(8, 9, 5, 9, 5)] [InlineData(16, 9, 5, 9, 5)]
    [InlineData(8, 3, 5, 1, 1)] [InlineData(16, 3, 5, 1, 1)]
    public void ReuseMatchesRationalPixelsAndReadsNewSources(int depth, int sw, int sh, int width, int height)
    {
        using var source = OriginalPixels.Buffer(sw, sh, depth);
        using var different = OriginalPixels.Buffer(sw, sh, depth, phase: 3);
        using var plan = BicubicResizePlan.Create(sw, sh, width, height, depth);
        using var first = plan.Resize(source);
        Assert.Equal(ExactResizeReference.Resize(OriginalPixels.Bytes(source), sw, sh, depth,
            width, height, ResizeFilter.Bicubic), OriginalPixels.Bytes(first));
        Assert.Equal(source.Orientation, first.Orientation);
        for (var y = 0; y < sh; y++) Row(different, y).CopyTo(Row(source, y));
        using var second = plan.Resize(source);
        var expected = ExactResizeReference.Resize(OriginalPixels.Bytes(different), sw, sh, depth,
            width, height, ResizeFilter.Bicubic);
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        using var ordinary = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        Assert.Equal(expected, OriginalPixels.Bytes(ordinary));
        Row(first, 0)[0] ^= 127; first.Dispose(); source.Dispose(); plan.Dispose();
        Assert.Equal(expected, OriginalPixels.Bytes(second)); Assert.Equal(0, plan.StorageBytes);
        Assert.Throws<ObjectDisposedException>(() => plan.Resize(different)); plan.Dispose();
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.Png16)]
    public void ResizePreservesEncodedOrientationWithoutApplyingIt(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 19, 13);
        using var source = ImageCodec.Decode(OriginalExif.Attach(fixture, ImageOrientation.MirrorVertical, bigEndian: true));
        using var plan = BicubicResizePlan.Create(source.Width, source.Height, 11, 7, source.BitDepth);
        using var result = plan.Resize(source);
        Assert.Equal(ImageOrientation.MirrorVertical, result.Orientation);
        Assert.Equal(ExactResizeReference.Resize(OriginalPixels.Bytes(source), 19, 13, source.BitDepth,
            11, 7, ResizeFilter.Bicubic), OriginalPixels.Bytes(result));
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void RetainedStorageAggregateRefusalAndCancellationPreserveSurvivors(int depth)
    {
        const long capacity = 1 << 20;
        using var budget = new TestMemoryBudget(capacity); var limits = new ImageLimits { MemoryBudget = budget };
        using var original = OriginalPixels.Buffer(37, 23, depth);
        using var source = original.Clone(limits); var sourceBytes = budget.Bytes;
        using var creation = new CancellationTokenSource();
        using var plan = BicubicResizePlan.Create(37, 23, 19, 11, depth, limits, creation.Token);
        creation.Cancel();
        if (PreparedSupported) Assert.True(plan.StorageBytes > 0);
        Assert.Equal(sourceBytes + plan.StorageBytes, budget.Bytes); var retained = budget.Bytes;
        using (var caller = budget.CreateScope())
        {
            Assert.True(caller.TryReserve(capacity - retained - 1, out var held));
            using (held)
            {
                var attempts = budget.GrowthAttempts;
                var refusal = Assert.Throws<ImageLimitException>(() => plan.Resize(source));
                Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope);
                Assert.Equal(attempts + 1, budget.GrowthAttempts);
                Assert.Equal(retained + held.Bytes, budget.Bytes);
            }
        }
        using (var canceled = new CancellationTokenSource())
        {
            budget.Admitted = _ => canceled.Cancel();
            try { Assert.Throws<OperationCanceledException>(() => plan.Resize(source, canceled.Token)); }
            finally { budget.Admitted = null; }
            Assert.Equal(retained, budget.Bytes);
        }
        using var first = plan.Resize(source); using var second = plan.Resize(source);
        var expected = OriginalPixels.Bytes(second); first.Dispose(); source.Dispose(); plan.Dispose();
        Assert.Equal(expected.Length, budget.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
        second.Dispose(); Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }

    [Fact]
    public void CreationRefusalIsTerminalAndLeavesTheRootUsable()
    {
        if (!PreparedSupported) return;
        using var budget = new TestMemoryBudget(128); var limits = new ImageLimits { MemoryBudget = budget };
        var refusal = Assert.Throws<ImageLimitException>(() => BicubicResizePlan.Create(37, 23, 19, 11, 16, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope);
        Assert.Equal(1, budget.GrowthAttempts); Assert.Equal(1, budget.Refusals); Assert.Equal(0, budget.Bytes);
        using var source = ImageBuffer.Create(1, 1, 8);
        using var identity = BicubicResizePlan.Create(1, 1, 1, 1, 8, limits);
        using (var result = identity.Resize(source)) Assert.Equal(4, budget.Bytes);
        Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }

    [Fact]
    public void CreationCancellationReleasesCapacityBeforeRecovery()
    {
        if (!PreparedSupported) return;
        using var budget = new TestMemoryBudget(1 << 20); var limits = new ImageLimits { MemoryBudget = budget };
        using var cancellation = new CancellationTokenSource(); budget.Admitted = _ => cancellation.Cancel();
        try { Assert.Throws<OperationCanceledException>(() => BicubicResizePlan.Create(37, 23, 19, 11, 16, limits, cancellation.Token)); }
        finally { budget.Admitted = null; }
        Assert.Equal(0, budget.Bytes);
        using var plan = BicubicResizePlan.Create(37, 23, 19, 11, 16, limits);
        using var source = OriginalPixels.Buffer(37, 23, 16); using var result = plan.Resize(source);
        Assert.Equal(plan.StorageBytes + 19 * 11 * 8, budget.Bytes);
    }

    [Fact]
    public void TightLocalLimitsChooseOrdinaryExecutionBeforeAdmission()
    {
        using var source = OriginalPixels.Buffer(37, 23, 16, opaque: true);
        var input = OriginalPixels.Bytes(source);
        using var budget = new TestMemoryBudget(1 << 20);
        var limits = new ImageLimits { MemoryBudget = budget, MaxWorkingBytes = input.Length + 19L * 11 * 8 + 4096 };
        using var plan = BicubicResizePlan.Create(37, 23, 19, 11, 16, limits);
        Assert.Equal(0, plan.StorageBytes); Assert.Equal(0, budget.GrowthAttempts);
        using (var result = plan.Resize(source))
            Assert.Equal(ExactResizeReference.Resize(input, 37, 23, 16, 19, 11, ResizeFilter.Bicubic), OriginalPixels.Bytes(result));
        Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }

    [Fact]
    public void ActiveExecutionRejectsReentranceAndDisposalThenRecovers()
    {
        using var budget = new TestMemoryBudget(1 << 20); var limits = new ImageLimits { MemoryBudget = budget };
        using var source = OriginalPixels.Buffer(37, 23, 16);
        using var plan = BicubicResizePlan.Create(37, 23, 19, 11, 16, limits);
        var checkedBusy = false;
        budget.Admitted = _ =>
        {
            Assert.Throws<InvalidOperationException>(() => plan.Resize(source));
            Assert.Throws<InvalidOperationException>(() => plan.Dispose()); checkedBusy = true;
        };
        try { using var result = plan.Resize(source); Assert.True(checkedBusy); }
        finally { budget.Admitted = null; }
        using var next = plan.Resize(source);
        using var ordinary = ImageTransforms.Resize(source, 19, 11, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(ordinary), OriginalPixels.Bytes(next));
    }

    [Fact]
    public async Task SurvivingOutputStaysAdmittedThroughAsynchronousEncoding()
    {
        using var budget = new TestMemoryBudget(2 << 20); var limits = new ImageLimits { MemoryBudget = budget };
        using var source = OriginalPixels.Buffer(37, 23, 16);
        using var plan = BicubicResizePlan.Create(37, 23, 19, 11, 16, limits);
        using var result = plan.Resize(source); var expected = OriginalPixels.Bytes(result);
        source.Dispose(); plan.Dispose(); Assert.Equal(expected.Length, budget.Bytes);
        using var output = new MemoryStream();
        await ImageCodec.EncodePngAsync(result, output, new PngEncodeOptions { Limits = limits });
        Assert.Equal(expected.Length, budget.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(result));
        using var decoded = ImageCodec.Decode(output.ToArray()); Assert.Equal(expected, OriginalPixels.Bytes(decoded));
        result.Dispose(); Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }

    [Fact]
    public void MismatchedOrDisposedInputDoesNotDamagePreparation()
    {
        using var budget = new TestMemoryBudget(1 << 20); var limits = new ImageLimits { MemoryBudget = budget };
        using var plan = BicubicResizePlan.Create(19, 13, 11, 7, 8, limits); var retained = budget.Bytes;
        using var wrongShape = ImageBuffer.Create(18, 13, 8); using var wrongDepth = ImageBuffer.Create(19, 13, 16);
        using var disposed = ImageBuffer.Create(19, 13, 8); disposed.Dispose();
        Assert.Throws<ArgumentException>(() => plan.Resize(wrongShape));
        Assert.Throws<ArgumentException>(() => plan.Resize(wrongDepth));
        Assert.Throws<ObjectDisposedException>(() => plan.Resize(disposed));
        Assert.Equal(retained, budget.Bytes);
        using var source = OriginalPixels.Buffer(19, 13, 8); using var result = plan.Resize(source);
    }

    [Fact]
    public void InvalidShapeDepthAndLimitsFailBeforeSharedAdmission()
    {
        using var budget = new TestMemoryBudget(0); var limits = new ImageLimits { MemoryBudget = budget };
        Assert.Throws<ArgumentOutOfRangeException>(() => BicubicResizePlan.Create(19, 13, 0, 7, 8, limits));
        Assert.Throws<ArgumentOutOfRangeException>(() => BicubicResizePlan.Create(0, 13, 11, 7, 8, limits));
        Assert.Throws<ArgumentOutOfRangeException>(() => BicubicResizePlan.Create(19, 13, 11, 7, 12, limits));
        Assert.Equal(ImageLimitKind.Dimension, Assert.Throws<ImageLimitException>(() =>
            BicubicResizePlan.Create(19, 13, 11, 7, 8, limits with { MaxDimension = 10 })).Limit);
        Assert.Equal(ImageLimitKind.PixelCount, Assert.Throws<ImageLimitException>(() =>
            BicubicResizePlan.Create(19, 13, 11, 7, 8, limits with { MaxPixels = 10 })).Limit);
        Assert.Equal(ImageLimitKind.BufferLength, Assert.Throws<ImageLimitException>(() =>
            BicubicResizePlan.Create(int.MaxValue, 2, 11, 7, 16,
                limits with { MaxDimension = int.MaxValue, MaxPixels = long.MaxValue })).Limit);
        Assert.Equal(ImageLimitScope.Operation, Assert.Throws<ImageLimitException>(() =>
            BicubicResizePlan.Create(19, 13, 11, 7, 8, limits with { MaxWorkingBytes = 128 })).Scope);
        Assert.Equal(0, budget.Attempts); Assert.Equal(0, budget.Bytes);
    }

    private static Span<byte> Row(ImageBuffer image, int y)
        => image.BitDepth == 8 ? image.GetRow8(y) : MemoryMarshal.AsBytes(image.GetRow16(y));

    private static ImageBuffer HalfSource(int depth)
    {
        var source = ImageBuffer.Create(4, 4, depth);
        for (var y = 0; y < 4; y++) for (var x = 0; x < 4; x++)
        {
            if (depth == 8)
            {
                var row = source.GetRow8(y); row[x * 4] = (byte)(y == 0 ? 1 : 0);
                row[x * 4 + 1] = 255; row[x * 4 + 2] = (byte)(y == 0 ? 255 : 0); row[x * 4 + 3] = (byte)(x == 0 ? 1 : 0);
            }
            else
            {
                var row = source.GetRow16(y); row[x * 4] = (ushort)(y == 0 ? 1 : 0);
                row[x * 4 + 1] = 65535; row[x * 4 + 2] = (ushort)(y == 0 ? 65535 : 0); row[x * 4 + 3] = (ushort)(x == 0 ? 1 : 0);
            }
        }
        return source;
    }
}
