using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8PrunedPredictionTests
{
    private delegate int Selector(Vp8Planes source, Vp8Planes reconstructed, int x, int y,
        int macroX, int macroY, int candidates, OperationContext context);
    private static Selector Method(string name) => typeof(Vp8Encoder)
        .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Selector>();
    private static readonly Selector Original = Method("SelectLuma"), Pruned = Method("SelectPrunedLuma");
    private static Vp8Planes Plane(byte[] pixels) => new(64, 64, 64, pixels, new byte[1024], new byte[1024]);

    [Theory] [InlineData(0, 0, 6)] [InlineData(20, 20, 8)] [InlineData(60, 60, 10)]
    public void OrderedSelectionAndWholeDestinationMatchTheOriginalAtEdgesAndInterior(int x, int y, int candidates)
    {
        var samples = Enumerable.Range(0, 4096).Select(i => (byte)((i * 31 + i / 64 * 47 + i % 7) & 255)).ToArray();
        var boundary = Enumerable.Range(0, 4096).Select(i => (byte)((i * 17 + i / 64 * 29) & 255)).ToArray();
        var unchanged = samples.ToArray(); var first = Plane(boundary.ToArray()); var second = Plane(boundary.ToArray());
        using var original = new OperationContext(ImageLimits.Default, default);
        using var pruned = new OperationContext(ImageLimits.Default, default);
        Assert.Equal(Original(Plane(samples), first, x, y, x / 16 * 16, y / 16 * 16, candidates, original),
            Pruned(Plane(samples), second, x, y, x / 16 * 16, y / 16 * 16, candidates, pruned));
        Assert.Equal(first.Y, second.Y); Assert.Equal(unchanged, samples);
        Assert.InRange(pruned.ConsumedWork, 192, original.ConsumedWork);
    }

    [Theory] [InlineData(0)] [InlineData(1)]
    public void ZeroAndNonzeroTiesKeepTheFirstModeAndFullRowsKeepTheirOriginalWork(int difference)
    {
        var source = Plane(Enumerable.Repeat((byte)(17 + difference), 4096).ToArray());
        var first = Plane(Enumerable.Repeat((byte)17, 4096).ToArray());
        var second = Plane(first.Y.ToArray());
        using var original = new OperationContext(ImageLimits.Default, default);
        using var pruned = new OperationContext(ImageLimits.Default, default);
        Assert.Equal(0, Original(source, first, 20, 20, 16, 16, 6, original));
        Assert.Equal(0, Pruned(source, second, 20, 20, 16, 16, 6, pruned));
        Assert.Equal(first.Y, second.Y); Assert.Equal(original.ConsumedWork, pruned.ConsumedWork);
        Assert.Equal(difference == 0 ? 192 : 6 * 192, pruned.ConsumedWork);
    }

    [Fact]
    public void WorkExhaustionAtTheFirstErrorRowStopsAndTheBorrowedRootRemainsUsable()
    {
        var samples = Enumerable.Repeat((byte)18, 4096).ToArray(); var unchanged = samples.ToArray();
        using var root = new TestMemoryBudget(10000);
        using (var context = new OperationContext(new ImageLimits { MaxWork = 119, MemoryBudget = root }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Pruned(Plane(samples),
                Plane(Enumerable.Repeat((byte)17, 4096).ToArray()), 20, 20, 16, 16, 6, context));
            Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
            Assert.Equal(96, context.ConsumedWork);
        }
        using (var next = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
            Assert.Equal(0, Pruned(Plane(samples), Plane(Enumerable.Repeat((byte)17, 4096).ToArray()), 20, 20, 16, 16, 6, next));
        Assert.Equal(unchanged, samples); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private const int Blocks = 129, Columns = 43, Capacity = 128 * 120 + 16384;
    private const long Prediction = Blocks * (16L * 6 * 192 + 4L * 512 + 24L * 512 + 64) + Blocks * 32L + 2048;
    private const long Eligibility = Prediction + 128 * 24L * 256 + 1056 * 16 + Blocks * 24L;
    private const long Growth = 3 * (Blocks * 384L * 21 + 32) + 3L * 0x7FFFF + 384;
    private static Vp8Planes CacheSource() => new(Columns * 16, 48, Columns * 16,
        new byte[Blocks * 256], new byte[Blocks * 64], new byte[Blocks * 64]);

    [Fact]
    public void ExtendedSampleCapacityIsReservedBeforeAllocationAndEndsWithItsOwner()
    {
        using var root = new TestMemoryBudget(Capacity);
        using var context = new OperationContext(new ImageLimits { MaxWork = Eligibility, MemoryBudget = root }, default);
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(CacheSource(), Blocks, 6, 4, context,
            new byte[Blocks * 16], fullFrame: true, segmented: true))
        {
            Assert.NotNull(model); Assert.Equal(128, model.PrefixMacroblocks); Assert.Equal(128, model.CacheMacroblocks);
            Assert.False(model.Intra16); Assert.Equal(Capacity, root.Bytes); Assert.Equal(1, root.Attempts);
            Assert.Equal(Eligibility, context.RemainingWork); model.Dispose(); model.Dispose(); Assert.Equal(0, root.Bytes);
        }
        Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void IndependentLocalWorkOrMemoryKeepsTheOriginalSampleBeforeSharedAdmission(bool memory)
    {
        using var root = new TestMemoryBudget(Capacity);
        using var context = new OperationContext(new ImageLimits { MaxWork = memory ? Eligibility : Eligibility - 1,
            MaxWorkingBytes = memory ? Growth + Capacity - 1 : ImageLimits.Default.MaxWorkingBytes, MemoryBudget = root }, default);
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(CacheSource(), Blocks, 6, 4, context,
            new byte[Blocks * 16], fullFrame: true, segmented: true))
        {
            Assert.NotNull(model); Assert.Equal(64, model.PrefixMacroblocks); Assert.Equal(64, model.CacheMacroblocks);
            Assert.Equal(24064, root.Bytes); Assert.Equal(1, root.Attempts);
        }
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SharedRefusalCannotRetryTheSmallerSampleAndReleaseEnablesSubsequentSuccess()
    {
        using var root = new TestMemoryBudget(Capacity); Assert.True(root.TryReserve(1, out var retained));
        using (var context = new OperationContext(new ImageLimits { MaxWork = Eligibility, MemoryBudget = root }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8Encoder.CoefficientModel.TryCreate(CacheSource(), Blocks, 6, 4,
                context, new byte[Blocks * 16], fullFrame: true, segmented: true));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(1, root.Bytes); Assert.Equal(1, root.Refusals); retained.Dispose();
        using (var next = new OperationContext(new ImageLimits { MaxWork = Eligibility, MemoryBudget = root }, default))
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(CacheSource(), Blocks, 6, 4, next,
            new byte[Blocks * 16], fullFrame: true, segmented: true))
        { Assert.NotNull(model); Assert.Equal(128, model.CacheMacroblocks); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
