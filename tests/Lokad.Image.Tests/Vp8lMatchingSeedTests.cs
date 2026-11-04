using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lMatchingSeedTests
{
    private static uint[] OriginalGreen()
    {
        var input = OriginalVp8lControls.Input("asymmetric");
        var pixels = new uint[input.Width * input.Height];
        for (var i = 0; i < pixels.Length; i++) pixels[i] = (uint)input.Pixels[4 * i + 3] << 8;
        return pixels;
    }

    [Fact]
    public void RetiredTokensStayChargedThroughReuseAndEmittedBytesHaveIndependentOwnership()
    {
        var original = OriginalGreen();
        using var root = new TestMemoryBudget(32_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var workingBytes = original.Length * 4L;
        context.Reserve(workingBytes); var working = original.ToArray();
        Vp8lTokenBuffer? spare = null;
        try
        {
            using var first = Vp8lPreparedEncoding.Prepare(working, 513, 4, context, 32, greenOnly: true);
            using var firstOutput = new MemoryStream(); first.WriteAlphaBody(firstOutput);
            var emitted = firstOutput.ToArray(); var expected = emitted.ToArray();
            spare = first.DetachMainTokens(); Assert.NotNull(spare);
            first.Dispose();
            Assert.Equal(original.Length * 8L, spare.CapacityBytes);
            Assert.Equal(workingBytes + spare.CapacityBytes, root.Bytes);
            original.CopyTo(working, 0);
            var owned = spare; spare = null;
            using var second = Vp8lPreparedEncoding.Prepare(working, 513, 4, context, 32, greenOnly: true, matchingSeed: owned);
            Array.Clear(working); context.Release(workingBytes); workingBytes = 0;
            using var secondOutput = new MemoryStream(); second.WriteAlphaBody(secondOutput);
            Assert.Equal(expected, secondOutput.ToArray()); Assert.Equal(expected, emitted);
            second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        finally { spare?.Dispose(); context.Release(workingBytes); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefusalOrCancellationAfterHandoffRefundsTokensAndTheBorrowedRootRecovers(bool cancellation)
    {
        var original = OriginalGreen();
        using var root = new TestMemoryBudget(32_000_000);
        using var caller = root.CreateScope(); Assert.True(caller.TryReserve(1, out var held));
        using var cancel = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        var workingBytes = original.Length * 4L;
        context.Reserve(workingBytes); var working = original.ToArray();
        using var first = Vp8lPreparedEncoding.Prepare(working, 513, 4, context, 32, greenOnly: true);
        var spare = first.DetachMainTokens(); Assert.NotNull(spare); first.Dispose();
        Assert.Equal(1 + workingBytes + spare.CapacityBytes, root.Bytes);
        original.CopyTo(working, 0);
        var phase = root.GrowthAttempts + 1;
        if (cancellation) root.Admitted = _ => { if (root.GrowthAttempts == phase) cancel.Cancel(); };
        else root.RefuseAtGrowth = phase;
        if (cancellation)
        {
            var failure = Assert.Throws<OperationCanceledException>(() => Vp8lPreparedEncoding.Prepare(working, 513, 4, context, 32, greenOnly: true, matchingSeed: spare));
            Assert.Equal(cancel.Token, failure.CancellationToken);
        }
        else
        {
            var failure = Assert.Throws<ImageLimitException>(() => Vp8lPreparedEncoding.Prepare(working, 513, 4, context, 32, greenOnly: true, matchingSeed: spare));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (failure.Limit, failure.Scope));
        }
        Assert.Equal(phase, root.GrowthAttempts); Assert.Equal(cancellation ? 0 : 1, root.Refusals);
        Assert.Equal(1 + workingBytes, root.Bytes);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        original.CopyTo(working, 0);
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lPreparedEncoding.Prepare(working, 513, 4, nextContext, 32, greenOnly: true);
        using var output = new MemoryStream(); next.WriteAlphaBody(output); Assert.True(output.Length > 0);
        next.Dispose(); context.Release(workingBytes); held.Dispose();
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PaletteAndAllFamilyPreparationReleaseUnusedSeedAndKeepExactBytes(bool allFamilies)
    {
        uint[] original = [0xff000000, 0xff00ff00, 0xff000000, 0xff00ff00, 0xff000000, 0xff00ff00];
        using var root = new TestMemoryBudget(10_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var method = allFamilies ? 6 : 4; var quality = allFamilies ? 100 : 32;
        using var expected = new MemoryStream();
        using (var control = Vp8lPreparedEncoding.Prepare(original.ToArray(), 3, method, context, quality)) control.WriteAlphaBody(expected);
        Assert.Equal(0, root.Bytes);
        var seed = new Vp8lTokenBuffer(context, Vp8lReferenceMatcher.HeadCount / 2);
        using var prepared = Vp8lPreparedEncoding.Prepare(original.ToArray(), 3, method, context, quality, matchingSeed: seed);
        Assert.Throws<ObjectDisposedException>(() => _ = seed.CapacityBytes);
        using var actual = new MemoryStream(); prepared.WriteAlphaBody(actual);
        Assert.Equal(expected.ToArray(), actual.ToArray());
        Assert.Null(prepared.DetachMainTokens()); prepared.Dispose();
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ValidationFailureDisposesTransferredCapacityWithoutDisposingCallerRoot()
    {
        using var root = new TestMemoryBudget(10_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var seed = new Vp8lTokenBuffer(context, Vp8lReferenceMatcher.HeadCount / 2);
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8lPreparedEncoding.Prepare(new uint[1], 1, 4, context, -1, matchingSeed: seed));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void IncompatibleLargeCapacityIsReleasedBeforeReplacementAndKeepsThePacket()
    {
        var original = OriginalGreen();
        using var root = new TestMemoryBudget(32_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var expected = new MemoryStream();
        using (var control = Vp8lPreparedEncoding.Prepare(original.ToArray(), 513, 4, context, 32, greenOnly: true))
            control.WriteAlphaBody(expected);
        Assert.Equal(0, root.Bytes);
        var seed = new Vp8lTokenBuffer(context, original.Length + 1);
        using var prepared = Vp8lPreparedEncoding.Prepare(original.ToArray(), 513, 4, context, 32, greenOnly: true, matchingSeed: seed);
        Assert.Throws<ObjectDisposedException>(() => _ = seed.CapacityBytes);
        using var actual = new MemoryStream(); prepared.WriteAlphaBody(actual);
        Assert.Equal(expected.ToArray(), actual.ToArray()); prepared.Dispose();
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SecondAlphaFilterFailureReleasesRetiredTokensAndTheEarlierPayload(bool cancellation)
    {
        var green = OriginalGreen(); var source = green.Select(p => (byte)(p >> 8)).ToArray();
        int firstPayload;
        long preparationEntryBytes = 0;
        using (var entryRoot = new TestMemoryBudget(32_000_000))
        using (var sizing = new OperationContext(new() { MemoryBudget = entryRoot }, default))
        {
            // Learn this preparation's first admitted capacity from the actual
            // producer, without depending on private call frames or inlining.
            entryRoot.Admitted = used => { if (preparationEntryBytes == 0) preparationEntryBytes = used; };
            using var prepared = Vp8lPreparedEncoding.Prepare(green.ToArray(), 513, 4, sizing, 32, greenOnly: true);
            firstPayload = 1 + Math.Min(prepared.PayloadBytes - 5, source.Length);
            Assert.True(preparationEntryBytes > 0);
        }
        long phase = 0;
        using (var sizingRoot = new TestMemoryBudget(32_000_000))
        using (var sizing = new OperationContext(new() { MemoryBudget = sizingRoot }, default))
        {
            var payloadAdmitted = false;
            sizingRoot.Admitted = used =>
            {
                // The root callback precedes OperationContext's local increment,
                // so their difference is the currently admitted capacity.
                var requested = used - (sizing.Limits.MaxWorkingBytes - sizing.RemainingWorkingBytes);
                // Emission consumes already prepared state. The next matching
                // entry reservation starts preparation of the second filter.
                if (payloadAdmitted && phase == 0 && requested == preparationEntryBytes)
                    phase = sizingRoot.GrowthAttempts;
                if (requested == firstPayload) payloadAdmitted = true;
            };
            using var encoded = Vp8AlphaEncoding.Create(source, 513, sizing);
            Assert.Equal(24588, encoded.Bytes.Length); encoded.Dispose(); Assert.Equal(0, sizingRoot.Bytes);
        }
        Assert.True(phase > 0);
        using var root = new TestMemoryBudget(32_000_000);
        using var cancel = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        if (cancellation) root.Admitted = _ => { if (root.GrowthAttempts == phase) cancel.Cancel(); };
        else root.RefuseAtGrowth = phase;
        if (cancellation)
            Assert.Equal(cancel.Token, Assert.Throws<OperationCanceledException>(() => Vp8AlphaEncoding.Create(source, 513, context)).CancellationToken);
        else
            Assert.Equal(ImageLimitScope.SharedBudget, Assert.Throws<ImageLimitException>(() => Vp8AlphaEncoding.Create(source, 513, context)).Scope);
        Assert.Equal(phase, root.GrowthAttempts); Assert.Equal(cancellation ? 0 : 1, root.Refusals);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8AlphaEncoding.Create(source, 513, nextContext);
        Assert.Equal(24588, next.Bytes.Length);
        Assert.Equal("ca83ebb44c092e661126e81d83a3345877b1de833a2d0bde289217b19d2d25ae",
            Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(next.Bytes)));
        Assert.Equal(green, source.Select(p => (uint)p << 8));
        next.Dispose(); Assert.Equal(0, root.Bytes);
    }
}
