using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lBorrowedMatchIndexTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LongUniformLowEffortMatchesKeepExactIndicesWithPoisonedReusedStorage(bool cancelable)
    {
        const int count = 8194, width = 4097;
        using var root = new TestMemoryBudget(4_000_000);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancelable ? cancellation.Token : default);
        var pixels = Enumerable.Repeat(0xff204060u, count).ToArray(); pixels[^1] ^= 0x100;
        var original = pixels.ToArray();
        using var expected = Vp8lReferenceMatcher.Build(pixels, width, 75, true, context);
        using var reused = new Vp8lMatchIndex(context, count);
        reused.Writable.Fill(int.MaxValue);
        var storage = reused.Storage;
        Vp8lReferenceMatcher.Fill(pixels, width, 75, true, context, reused);
        Assert.Same(storage, reused.Storage);
        Assert.True(expected.Values.SequenceEqual(reused.Values));
        Assert.Equal(0u, reused.Values[0]); Assert.Equal(0u, reused.Values[^1]);
        Assert.Equal(original, pixels); Assert.Equal(count * 8L, root.Bytes);
        reused.Dispose(); Assert.Equal(count * 4L, root.Bytes);
        Assert.Equal(0u, expected.Values[0]);
        expected.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void PoisonedReusableStorageMatchesFreshIndicesAcrossShapesAndSmallInputs()
    {
        using var root = new TestMemoryBudget(32_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var reused = new Vp8lMatchIndex(context, 0);
        foreach (var (count, width) in new[] { (1, 1), (2, 2), (35, 7), (35, 7), (16384, 256), (35, 7), (2, 1), (1, 1) })
        {
            var pixels = new uint[count];
            for (var i = 0; i < count; i++) pixels[i] = i % 23 < 8 ? (uint)(i % 5) : (uint)(i * 0x10203);
            var original = pixels.ToArray(); var previous = reused.Storage;
            previous.AsSpan().Fill(-1234567);
            using var fresh = Vp8lReferenceMatcher.Build(pixels, width, 32, false, context);
            Vp8lReferenceMatcher.Fill(pixels, width, 32, false, context, reused);
            Assert.True(fresh.Values.SequenceEqual(reused.Values)); Assert.Equal(original, pixels);
            if (previous.Length == count) Assert.Same(previous, reused.Storage);
            Assert.Equal(count * 8L, root.Bytes); // The two independent owners.
        }
        reused.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ForeignContextsAndDisposedStorageCannotBeBorrowed()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var first = new OperationContext(new() { MemoryBudget = root }, default);
        using var second = new OperationContext(new() { MemoryBudget = root }, default);
        using var index = new Vp8lMatchIndex(first, 3); index.Writable.Fill(123);
        uint[] pixels = [1, 2, 1];
        Assert.Throws<ArgumentException>(() => Vp8lReferenceMatcher.Fill(pixels, 3, 32, false, second, index));
        Assert.Equal(new uint[] { 123, 123, 123 }, index.Values.ToArray()); Assert.Equal(12, root.Bytes);
        index.Dispose();
        Assert.Throws<ObjectDisposedException>(() => Vp8lReferenceMatcher.Fill(pixels, 3, 32, false, first, index));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DiscardedScratchDoesNotOverlapRefusedOrCanceledReplacement(bool cancellation)
    {
        using var root = new TestMemoryBudget(1_000_000); using var cancel = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        using var index = new Vp8lMatchIndex(context, 7); var phase = root.GrowthAttempts + 1;
        if (cancellation) root.Admitted = _ => cancel.Cancel(); else root.RefuseAtGrowth = phase;
        var pixels = new uint[100];
        if (cancellation)
            Assert.Equal(cancel.Token, Assert.Throws<OperationCanceledException>(() => Vp8lReferenceMatcher.Fill(pixels, 10, 32, false, context, index)).CancellationToken);
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceMatcher.Fill(pixels, 10, 32, false, context, index));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(phase, root.GrowthAttempts); Assert.Empty(index.Storage); Assert.Equal(0, root.Bytes);
        Assert.Equal(cancellation ? 400 : 28, root.Peak); // Admission, not physical allocation.
        index.Dispose(); root.Admitted = null; root.RefuseAtGrowth = 0;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferenceMatcher.Build(pixels, 10, 32, false, nextContext);
        Assert.Equal(400, root.Bytes); next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void HeadPreparationRefusalKeepsBorrowedIndexChargedUntilItsOwnerDisposes()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var index = new Vp8lMatchIndex(context, 35); root.RefuseAtGrowth = root.GrowthAttempts + 1;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceMatcher.Fill(new uint[35], 7, 32, false, context, index));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(140, root.Bytes); Assert.Equal(35, index.Values.Length);
        index.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void PreparedCandidatesSurviveRebuiltAndDisposedMatchStorage()
    {
        var input = OriginalVp8lControls.Input("asymmetric"); var original = new uint[input.Width * input.Height];
        for (var i = 0; i < original.Length; i++) original[i] = (uint)input.Pixels[4 * i + 3] << 8;
        using var root = new TestMemoryBudget(32_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var index = new Vp8lMatchIndex(context, 0);
        using var first = Vp8lPreparedEncoding.Prepare(original.ToArray(), input.Width, 4, context, 32, greenOnly: true, matchStorage: index);
        using var before = new MemoryStream(); first.WriteAlphaBody(before);
        var changed = original.Select(value => (value ^ 0x5500u) & 0xff00u).ToArray();
        using var second = Vp8lPreparedEncoding.Prepare(changed, input.Width, 4, context, 32, greenOnly: true, matchStorage: index);
        var charged = root.Bytes; var capacity = index.Storage.Length * 4L;
        index.Dispose(); Assert.Equal(charged - capacity, root.Bytes);
        using var after = new MemoryStream(); first.WriteAlphaBody(after); Assert.Equal(before.ToArray(), after.ToArray());
        using var actualSecond = new MemoryStream(); second.WriteAlphaBody(actualSecond);
        using var expectedSecond = new MemoryStream();
        using (var independentlyPrepared = Vp8lPreparedEncoding.Prepare(original.Select(value => (value ^ 0x5500u) & 0xff00u).ToArray(), input.Width, 4, context, 32, greenOnly: true))
            independentlyPrepared.WriteAlphaBody(expectedSecond);
        Assert.Equal(expectedSecond.ToArray(), actualSecond.ToArray());
        first.Dispose(); second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
