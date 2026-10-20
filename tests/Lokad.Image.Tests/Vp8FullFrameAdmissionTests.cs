using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8FullFrameAdmissionTests
{
    private const int Blocks = 65;
    private static Vp8Planes Planes() => new(13 * 16, 5 * 16, 13 * 16,
        new byte[Blocks * 256], new byte[Blocks * 64], new byte[Blocks * 64]);

    [Fact]
    public void CompleteQ0CacheReservesItsRetainedCapacityThroughTheBorrowedRoot()
    {
        using var root = new TestMemoryBudget(Blocks * 120 + 16384);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(Planes(), Blocks, 6, 4, context, fullFrame: true))
        {
            Assert.NotNull(model); Assert.Equal(Blocks, model.CacheMacroblocks); Assert.False(model.Intra16);
            Assert.Equal(Blocks * 120 + 16384, root.Bytes);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void InsufficientLocalWorkKeepsThePrefixPolicyBeforeSharedAdmission()
    {
        var samples = Blocks * 384;
        var original = Blocks * (16L * 6 * 192 + 4 * 512L + 24L * 512 + 64) + samples * 128L + 65536;
        var additional = (Blocks - 64) * (24L * 256 + 256) + 1056 * 16;
        using var root = new TestMemoryBudget(65536);
        using var context = new OperationContext(new ImageLimits { MaxWork = original + additional - 1, MemoryBudget = root }, default);
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(Planes(), Blocks, 6, 4, context, fullFrame: true))
        { Assert.NotNull(model); Assert.Equal(64, model.CacheMacroblocks); Assert.False(model.Intra16); }
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
    }

    [Fact]
    public void SelectedMixedCacheCannotRetryAnOtherwiseAdmissiblePrefixAfterSharedRefusal()
    {
        const int blocks = 81, columns = 9;
        const long cacheBytes = blocks * 136 + 16384 + columns + 64;
        var source = new Vp8Planes(129, 129, 144, new byte[blocks * 256], new byte[blocks * 64], new byte[blocks * 64]);
        var modes = new byte[blocks * 16]; using var root = new TestMemoryBudget(cacheBytes);
        Assert.True(root.TryReserve(1, out var retained)); Assert.True(24064 < cacheBytes - 1);
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8Encoder.CoefficientModel.TryCreate(
                source, blocks, 6, 4, context, modes, fullFrame: true, segmented: true));
            Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        }
        Assert.Equal(1, root.Bytes); Assert.Equal(1, root.Refusals); retained.Dispose();
        using (var next = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(source, blocks, 6, 4, next, modes, fullFrame: true, segmented: true))
        { Assert.NotNull(model); Assert.True(model.Intra16); Assert.Equal(blocks, model.CacheMacroblocks); Assert.Equal(cacheBytes, root.Bytes); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SharedRefusalDoesNotRetryTheSmallerPrefixAndReleaseEnablesSubsequentWork()
    {
        using var root = new TestMemoryBudget(Blocks * 120 + 16384);
        Assert.True(root.TryReserve(1, out var retained));
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        {
            var failure = Assert.Throws<ImageLimitException>(() =>
                Vp8Encoder.CoefficientModel.TryCreate(Planes(), Blocks, 6, 4, context, fullFrame: true));
            Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope); Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
        }
        Assert.Equal(1, root.Refusals); Assert.Equal(1, root.Bytes); retained.Dispose();
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(Planes(), Blocks, 6, 4, context, fullFrame: true))
        { Assert.NotNull(model); Assert.Equal(Blocks, model.CacheMacroblocks); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
