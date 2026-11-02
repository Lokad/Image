using System.Runtime.InteropServices;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Lanczos3ResizePlanTests
{
    [Theory]
    [InlineData(8, 11, 7)] [InlineData(16, 11, 7)]
    [InlineData(8, 29, 23)] [InlineData(16, 29, 23)]
    public void ReuseReadsNewSourcePixelsAndReturnsIndependentResults(int depth, int width, int height)
    {
        using var source = OriginalPixels.Buffer(19, 13, depth);
        using var different = OriginalPixels.Buffer(19, 13, depth, phase: 3);
        using var plan = Lanczos3ResizePlan.Create(19, 13, width, height, depth);
        using var first = plan.Resize(source);
        using var expected = ImageTransforms.Resize(different, width, height, ResizeFilter.Lanczos3);
        for (var y = 0; y < source.Height; y++) Row(different, y).CopyTo(Row(source, y));
        using var second = plan.Resize(source);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(second));
        Assert.NotEqual(OriginalPixels.Bytes(first), OriginalPixels.Bytes(second));
        Assert.Equal(OriginalPixels.Bytes(different), OriginalPixels.Bytes(source));
        var pixels = OriginalPixels.Bytes(second);
        Row(first, 0)[0] ^= 127; first.Dispose(); source.Dispose(); plan.Dispose();
        Assert.Equal(pixels, OriginalPixels.Bytes(second)); Assert.Equal(0, plan.StorageBytes);
        Assert.Throws<ObjectDisposedException>(() => plan.Resize(different)); plan.Dispose();
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void IdentityCopiesHiddenRgbWithIndependentAdmission(int depth)
    {
        using var budget = new TestMemoryBudget(1 << 20); var limits = new ImageLimits { MemoryBudget = budget };
        using var source = Source(19, 13, depth, limits);
        if (depth == 8) { source.GetRow8(0)[0] = 173; source.GetRow8(0)[3] = 0; }
        else { source.GetRow16(0)[0] = 45013; source.GetRow16(0)[3] = 0; }
        var expected = OriginalPixels.Bytes(source); var sourceBytes = budget.Bytes;
        using var plan = Lanczos3ResizePlan.Create(19, 13, 19, 13, depth, limits);
        Assert.Equal(0, plan.StorageBytes); Assert.Equal(sourceBytes, budget.Bytes);
        using var output = plan.Resize(source); Assert.Equal(expected, OriginalPixels.Bytes(output));
        source.Dispose(); plan.Dispose(); Assert.Equal(expected.Length, budget.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(output)); output.Dispose();
        Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void RetainedStorageRefusalCancellationAndSurvivingResultsShareOneRoot(int depth)
    {
        const long capacity = 1 << 20;
        using var budget = new TestMemoryBudget(capacity); var limits = new ImageLimits { MemoryBudget = budget };
        using var source = Source(19, 13, depth, limits); var sourceBytes = budget.Bytes;
        using var creation = new CancellationTokenSource();
        using var plan = Lanczos3ResizePlan.Create(19, 13, 11, 7, depth, limits, creation.Token);
        creation.Cancel(); // Creation cancellation cannot poison future calls.
        Assert.Equal(sourceBytes + plan.StorageBytes, budget.Bytes); var retained = budget.Bytes;
        using (var caller = budget.CreateScope())
        {
            Assert.True(caller.TryReserve(capacity - retained - 1, out var held));
            using (held)
            {
                var refusal = Assert.Throws<ImageLimitException>(() => plan.Resize(source));
                Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope);
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
    public void ReentrantExecutionAndActiveDisposalFailWithoutDamagingThePlan()
    {
        using var budget = new TestMemoryBudget(1 << 20); var limits = new ImageLimits { MemoryBudget = budget };
        using var source = Source(19, 13, 16, limits);
        using var plan = Lanczos3ResizePlan.Create(19, 13, 11, 7, 16, limits);
        var checkedBusy = false;
        budget.Admitted = _ =>
        {
            Assert.Throws<InvalidOperationException>(() => plan.Resize(source));
            Assert.Throws<InvalidOperationException>(() => plan.Dispose()); checkedBusy = true;
        };
        try { using var result = plan.Resize(source); Assert.True(checkedBusy); }
        finally { budget.Admitted = null; }
        using var next = plan.Resize(source);
        using var expected = ImageTransforms.Resize(source, 11, 7, ResizeFilter.Lanczos3);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(next));
    }

    [Theory]
    [InlineData(128)] [InlineData(512)] [InlineData(4096)]
    public void PartialPreparationRefusalReleasesAllOwnedCapacity(long capacity)
    {
        using var budget = new TestMemoryBudget(capacity); var limits = new ImageLimits { MemoryBudget = budget };
        var error = Assert.Throws<ImageLimitException>(() => Lanczos3ResizePlan.Create(37, 23, 19, 11, 16, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(0, budget.Bytes);
        using var source = ImageBuffer.Create(1, 1, 8, limits);
        using var identity = Lanczos3ResizePlan.Create(1, 1, 1, 1, 8, limits);
        using var result = identity.Resize(source); Assert.Equal(8, budget.Bytes);
    }

    [Fact]
    public void PreparationCancellationAndLocalBoundsCleanUpBeforeReuse()
    {
        using var budget = new TestMemoryBudget(1 << 20); var limits = new ImageLimits { MemoryBudget = budget };
        using var canceled = new CancellationTokenSource(); budget.Admitted = _ => canceled.Cancel();
        try { Assert.Throws<OperationCanceledException>(() => Lanczos3ResizePlan.Create(19, 13, 11, 7, 16, limits, canceled.Token)); }
        finally { budget.Admitted = null; }
        Assert.Equal(0, budget.Bytes);
        var error = Assert.Throws<ImageLimitException>(() => Lanczos3ResizePlan.Create(19, 13, 11, 7, 16, limits with { MaxWorkingBytes = 1024 }));
        Assert.Equal(ImageLimitScope.Operation, error.Scope); Assert.Equal(0, budget.Bytes);
        using var plan = Lanczos3ResizePlan.Create(19, 13, 11, 7, 16, limits);
        using var source = Source(19, 13, 16, limits); using var result = plan.Resize(source);
    }

    [Fact]
    public void MismatchedAndDisposedInputsDoNotConsumeOrInvalidateThePlan()
    {
        using var budget = new TestMemoryBudget(1 << 20); var limits = new ImageLimits { MemoryBudget = budget };
        using var plan = Lanczos3ResizePlan.Create(19, 13, 11, 7, 8, limits);
        var retained = budget.Bytes;
        using var wrongShape = ImageBuffer.Create(18, 13, 8);
        using var wrongDepth = ImageBuffer.Create(19, 13, 16);
        using var disposed = ImageBuffer.Create(19, 13, 8); disposed.Dispose();
        Assert.Throws<ArgumentException>(() => plan.Resize(wrongShape));
        Assert.Throws<ArgumentException>(() => plan.Resize(wrongDepth));
        Assert.Throws<ObjectDisposedException>(() => plan.Resize(disposed));
        Assert.Equal(retained, budget.Bytes);
        using var source = OriginalPixels.Buffer(19, 13, 8); using var result = plan.Resize(source);
    }

    [Fact]
    public void InvalidDimensionsDepthAndBufferLengthsFailBeforeAdmission()
    {
        using var budget = new TestMemoryBudget(0); var limits = new ImageLimits { MemoryBudget = budget };
        Assert.Equal(ImageLimitKind.Dimension, Assert.Throws<ImageLimitException>(() =>
            Lanczos3ResizePlan.Create(19, 13, 11, 7, 8, limits with { MaxDimension = 10 })).Limit);
        Assert.Equal(ImageLimitKind.PixelCount, Assert.Throws<ImageLimitException>(() =>
            Lanczos3ResizePlan.Create(19, 13, 11, 7, 8, limits with { MaxPixels = 10 })).Limit);
        Assert.Equal(ImageLimitKind.BufferLength, Assert.Throws<ImageLimitException>(() =>
            Lanczos3ResizePlan.Create(int.MaxValue, 2, 11, 7, 16,
                limits with { MaxDimension = int.MaxValue, MaxPixels = long.MaxValue })).Limit);
        Assert.Throws<ArgumentOutOfRangeException>(() => Lanczos3ResizePlan.Create(19, 13, 11, 7, 12, limits));
        Assert.Equal(0, budget.Attempts); Assert.Equal(0, budget.Bytes);
    }

    private static Span<byte> Row(ImageBuffer image, int y)
        => image.BitDepth == 8 ? image.GetRow8(y) : MemoryMarshal.AsBytes(image.GetRow16(y));
    private static ImageBuffer Source(int width, int height, int depth, ImageLimits limits)
    {
        using var original = OriginalPixels.Buffer(width, height, depth);
        return original.Clone(limits);
    }
}
