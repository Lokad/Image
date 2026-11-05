using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPredictionScratchTests
{
    private static uint[] Pixels(bool greenOnly)
    {
        var rgba = Vp8lReferencePredictorTests.Asymmetric();
        var result = new uint[rgba.Length / 4];
        for (var i = 0; i < result.Length; i++)
            result[i] = greenOnly ? (uint)rgba[i * 4 + 3] << 8
                : (uint)rgba[i * 4 + 3] << 24 | (uint)rgba[i * 4] << 16
                    | (uint)rgba[i * 4 + 1] << 8 | rgba[i * 4 + 2];
        return result;
    }

    [Theory]
    [InlineData(false, false, 132867)]
    [InlineData(true, false, 132867)]
    [InlineData(true, true, 132867)]
    [InlineData(true, false, 1024)] // Rows fit; adaptive counts still need ownership.
    public void DirtyScratchPreservesEveryResidualAndTheSelectedMapOwnsItsLifetime(
        bool greenOnly, bool lowEffort, int capacity)
    {
        var original = Pixels(greenOnly); var expected = original.ToArray();
        byte[] expectedMap; long expectedCost; int selected, stored;
        using (var referenceRoot = new TestMemoryBudget(8_000_000))
        using (var referenceContext = new OperationContext(new() { MemoryBudget = referenceRoot }, default))
        {
            using var reference = Vp8lReferencePredictor.SelectAndApply(expected, 513, 2, 3, lowEffort, referenceContext, greenOnly);
            expectedMap = reference.Modes.ToArray(); expectedCost = reference.EntropyCost;
            selected = reference.SelectedBits; stored = reference.StoredBits;
        }
        using var root = new TestMemoryBudget(8_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var tokens = new Vp8lTokenBuffer(context, capacity);
        var scratch = tokens.BorrowScratchWords(context, capacity * 2); scratch.Fill(-1);
        var actual = original.ToArray();
        using var plan = Vp8lReferencePredictor.SelectAndApply(actual, 513, 2, 3, lowEffort, context, greenOnly, scratch);
        Assert.Equal(expected, actual); Assert.Equal(expectedMap, plan.Modes.ToArray());
        Assert.Equal((selected, stored, expectedCost), (plan.SelectedBits, plan.StoredBits, plan.EntropyCost));
        Assert.Equal(capacity * 8L + plan.Modes.Length, root.Bytes);
        Assert.Equal(0, tokens.Count); Assert.Equal(0, tokens.CacheBits);
        // Later matching may replace every byte. The selected map cannot alias it.
        scratch.Fill(0x12345678); tokens.Add(Vp8lToken.Literal(0xabcdef01)); tokens.Dispose();
        Assert.Equal(expectedMap, plan.Modes.ToArray()); Assert.Equal(plan.Modes.Length, root.Bytes);
        plan.Dispose(); plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(false, 2)]
    [InlineData(true, 1)] [InlineData(true, 2)]
    public void RefusalOrCancellationLeavesBorrowedCapacityChargedWithoutRetryAndTheRootRecovers(bool cancellation, int phase)
    {
        var original = Pixels(true);
        using var root = new TestMemoryBudget(8_000_000);
        using var cancel = new CancellationTokenSource();
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token))
        using (var tokens = new Vp8lTokenBuffer(context, original.Length))
        {
            var target = root.GrowthAttempts + phase;
            if (cancellation) root.Admitted = _ => { if (root.GrowthAttempts == target) cancel.Cancel(); };
            else root.RefuseAtGrowth = target;
            if (cancellation)
                Assert.Equal(cancel.Token, Assert.Throws<OperationCanceledException>(() =>
                    Vp8lReferencePredictor.SelectAndApply(original.ToArray(), 513, 2, 3, false, context, true,
                        tokens.BorrowScratchWords(context, original.Length * 2))).CancellationToken);
            else
            {
                var error = Assert.Throws<ImageLimitException>(() =>
                    Vp8lReferencePredictor.SelectAndApply(original.ToArray(), 513, 2, 3, false, context, true,
                        tokens.BorrowScratchWords(context, original.Length * 2)));
                Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            }
            Assert.Equal(target, root.GrowthAttempts); Assert.Equal(cancellation ? 0 : 1, root.Refusals);
            Assert.Equal(original.Length * 8L, root.Bytes); Assert.Equal(0, tokens.Count);
            root.Admitted = null; root.RefuseAtGrowth = 0;
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var nextTokens = new Vp8lTokenBuffer(nextContext, original.Length);
        using var next = Vp8lReferencePredictor.SelectAndApply(original, 513, 2, 3, false, nextContext, true,
            nextTokens.BorrowScratchWords(nextContext, original.Length * 2));
        Assert.NotEmpty(next.Modes.ToArray()); next.Dispose(); nextTokens.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void SourceAliasingIsRejectedBeforeAdmissionOrMutation()
    {
        var original = Pixels(true); var copy = original.ToArray();
        using var root = new TestMemoryBudget(8_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Throws<ArgumentException>(() => Vp8lReferencePredictor.SelectAndApply(original, 513, 2, 3, false, context,
            true, MemoryMarshal.Cast<uint, int>(original.AsSpan())));
        Assert.Equal(copy, original); Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes);
    }
}
