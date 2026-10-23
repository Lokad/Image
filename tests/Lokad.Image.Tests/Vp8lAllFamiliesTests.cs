using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lAllFamiliesTests
{
    private static uint[] Fragment(OperationContext context)
    {
        context.Reserve(24);
        return [10u << 8, 20u << 8, 30u << 8, 30u << 8, 10u << 8, 40u << 8];
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void LateFamilyFailureReleasesTheEarlierWinnerAndTrialStorageWithoutFallback(int failure)
    {
        long growths, work;
        using (var sizingRoot = new TestMemoryBudget(10_000_000))
        using (var sizing = new OperationContext(new() { MemoryBudget = sizingRoot }, default))
        {
            var pixels = Fragment(sizing); var start = sizingRoot.GrowthAttempts;
            using var prepared = Vp8lPreparedEncoding.Prepare(pixels, 3, 6, sizing, 100);
            growths = sizingRoot.GrowthAttempts - start; work = sizing.ConsumedWork;
            prepared.Dispose(); sizing.Release(24); Assert.Equal(0, sizingRoot.Bytes);
        }
        using var root = new TestMemoryBudget(10_000_000); using var caller = root.CreateScope(); Assert.True(caller.TryReserve(1, out var held));
        using var inputContext = new OperationContext(new() { MemoryBudget = root }, default); var input = Fragment(inputContext); var original = input.ToArray();
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWork = failure == 2 ? work - 1 : 2_000_000_000 }, cancellation.Token);
        var lastGrowth = root.GrowthAttempts + growths;
        if (failure == 0) root.RefuseAtGrowth = lastGrowth;
        if (failure == 1) root.Admitted = _ => { if (root.GrowthAttempts == lastGrowth) cancellation.Cancel(); };
        if (failure == 1) Assert.Throws<OperationCanceledException>(() => Vp8lPreparedEncoding.Prepare(input, 3, 6, context, 100));
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8lPreparedEncoding.Prepare(input, 3, 6, context, 100));
            Assert.Equal((failure == 0 ? ImageLimitKind.WorkingBytes : ImageLimitKind.Work,
                failure == 0 ? ImageLimitScope.SharedBudget : ImageLimitScope.Operation), (error.Limit, error.Scope));
        }
        Assert.Equal(25, root.Bytes); Assert.Equal(original, input); Assert.Equal(failure == 0 ? 1 : 0, root.Refusals);
        if (failure != 2) Assert.Equal(lastGrowth, root.GrowthAttempts);
        root.Admitted = null; root.RefuseAtGrowth = 0; held.Dispose();
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lPreparedEncoding.Prepare(input, 3, 6, nextContext, 100); Assert.Equal(original, input);
        Array.Clear(input); inputContext.Release(24); var retained = root.Bytes; Assert.True(retained > 0);
        using var first = new MemoryStream(); next.WriteAlphaBody(first); using var second = new MemoryStream(); next.WriteAlphaBody(second);
        Assert.Equal(first.ToArray(), second.ToArray()); Assert.Equal(retained, root.Bytes);
        next.Dispose(); next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(first.CanWrite); Assert.True(second.CanWrite);
    }

    [Fact]
    public void DefaultWorkRefusalForCompleteAlphaSearchIsTerminalAndAllowsSameRootRecovery()
    {
        using var root = new TestMemoryBudget(128_000_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var source = OriginalVp8lControls.Buffer("asymmetric", 8, root); using var input = Vp8PreparedInput.Create(source, context);
        var baseline = root.Bytes; var original = SHA256.HashData(input.Alpha);
        var error = Assert.Throws<ImageLimitException>(() => Vp8AlphaEncoding.Create(input.Alpha, input.Width, context, 6));
        Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
        Assert.Equal(baseline, root.Bytes); Assert.Equal(0, root.Refusals); Assert.True(context.ConsumedWork <= context.Limits.MaxWork);
        Assert.Equal(original, SHA256.HashData(input.Alpha));
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8AlphaEncoding.Create([37], 1, nextContext, 6); Assert.Equal(new byte[] { 0, 37 }, next.Bytes.ToArray());
        next.Dispose(); Assert.Equal(baseline, root.Bytes); input.Dispose(); source.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
