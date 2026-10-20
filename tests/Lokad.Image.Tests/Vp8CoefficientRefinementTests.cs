using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8CoefficientRefinementTests
{
    private delegate bool Refiner(ReadOnlySpan<short> residual, Span<short> coefficients,
        int dc, int ac, int plane, int predictor, ReadOnlySpan<byte> probabilities,
        ReadOnlySpan<int> costs, int lambda, OperationContext context);
    private static readonly Refiner Refine = typeof(Vp8Encoder)
        .GetMethod("RefineCoefficients", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Refiner>();
    private static int[] Costs() => Enumerable.Range(0, 257).Select(p =>
        (int)Math.Round(-Math.Log2(Math.Max(1, p) / 256d) * 256, MidpointRounding.AwayFromZero)).ToArray();
    private static short[] Residual() => Enumerable.Repeat((short)3, 16).ToArray();
    private static short[] Seed(short[] residual)
    { var values = new short[16]; Vp8ForwardDct.Quantize(residual, values, 16, 16); return values; }

    [Fact]
    public void ZeroRateWeightRetainsTheNearestSeedIncludingItsExactHalfTie()
    {
        var residual = Residual(); var values = Seed(residual); var unchanged = values.ToArray();
        Assert.Equal(2, values[0]); using var context = new OperationContext(ImageLimits.Default, default);
        Assert.False(Refine(residual, values, 16, 16, 3, 0, Vp8Tables.CoefficientProbabilities, Costs(), 0, context));
        Assert.Equal(unchanged, values); Assert.Equal(4736, context.ConsumedWork);
    }

    [Fact]
    public void RateWeightedTerminalEobCanRemoveTheWholeBlockWithoutHeapAdmission()
    {
        var residual = Residual(); var unchanged = residual.ToArray(); var values = Seed(residual);
        using var root = new TestMemoryBudget(10000);
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        {
            Assert.True(Refine(residual, values, 16, 16, 3, 0, Vp8Tables.CoefficientProbabilities, Costs(), 1000000, context));
            Assert.All(values, value => Assert.Equal(0, value)); Assert.Equal(4736, context.ConsumedWork);
        }
        Assert.Equal(unchanged, residual); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ActualWorkExhaustionPrecedesMutationAndDoesNotPoisonTheBorrowedRoot()
    {
        var residual = Residual(); var values = Seed(residual); var unchanged = values.ToArray();
        using var root = new TestMemoryBudget(10000);
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWork = 4735 }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Refine(residual, values, 16, 16, 3, 0,
                Vp8Tables.CoefficientProbabilities, Costs(), 1000000, context));
            Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
            Assert.Equal(unchanged, values);
        }
        using (var next = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
            Assert.True(Refine(residual, values, 16, 16, 3, 0, Vp8Tables.CoefficientProbabilities, Costs(), 1000000, next));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationPrecedesCoefficientMutation()
    {
        var residual = Residual(); var values = Seed(residual); var unchanged = values.ToArray();
        using var cancellation = new CancellationTokenSource(); using var root = new TestMemoryBudget(10000);
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, cancellation.Token))
        {
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => Refine(residual, values, 16, 16, 3, 0,
                Vp8Tables.CoefficientProbabilities, Costs(), 1000000, context));
        }
        Assert.Equal(unchanged, values); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void IndependentWorkAllowanceSelectsRefinementWithoutChangingTheCacheOwner(bool admitted)
    {
        const int blocks = 81, columns = 9, capacity = blocks * 136 + 16384 + columns + 64;
        var source = new Vp8Planes(129, 129, 144, new byte[blocks * 256], new byte[blocks * 64], new byte[blocks * 64]);
        using var root = new TestMemoryBudget(capacity);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(source, blocks, 6, 4, context,
            new byte[blocks * 16], fullFrame: true, segmented: true, refinementWork: admitted ? 1 : 2000000000))
        {
            Assert.NotNull(model); Assert.True(model.Intra16); Assert.Equal(admitted, model.Refine);
            Assert.Equal(blocks, model.CacheMacroblocks); Assert.Equal(capacity, root.Bytes);
            Assert.Equal(ImageLimits.Default.MaxWork, context.RemainingWork); Assert.Equal(1, root.Attempts);
        }
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RefinementAdmissionCannotRecoverFromSharedRefusal()
    {
        const int blocks = 81, columns = 9, capacity = blocks * 136 + 16384 + columns + 64;
        var source = new Vp8Planes(129, 129, 144, new byte[blocks * 256], new byte[blocks * 64], new byte[blocks * 64]);
        var modes = new byte[blocks * 16]; using var root = new TestMemoryBudget(capacity);
        Assert.True(root.TryReserve(1, out var retained));
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8Encoder.CoefficientModel.TryCreate(source, blocks, 6, 4,
                context, modes, fullFrame: true, segmented: true, refinementWork: 1));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(1, root.Refusals); Assert.Equal(1, root.Bytes); retained.Dispose();
        using (var next = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(source, blocks, 6, 4, next, modes,
            fullFrame: true, segmented: true, refinementWork: 1))
        { Assert.NotNull(model); Assert.True(model.Refine); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
