using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lHeadTokenHandoffTests
{
    [Theory]
    [InlineData(131071, false)] [InlineData(131072, true)] [InlineData(131073, true)]
    public void ScratchRequiresActualEmptyCapacityAndTheSameOperation(int capacity, bool fits)
    {
        using var root = new TestMemoryBudget(capacity * 8L);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var foreign = new OperationContext(new() { MemoryBudget = root }, default);
        using var seed = new Vp8lTokenBuffer(context, capacity);
        if (fits)
        {
            seed.BorrowScratchWords(context, Vp8lReferenceMatcher.HeadCount).Fill(-1);
            Assert.Equal(0, seed.Count); Assert.Empty(seed.Tokens.ToArray());
        }
        else Assert.Throws<ArgumentException>(() => { seed.BorrowScratchWords(context, Vp8lReferenceMatcher.HeadCount); });
        Assert.Throws<ArgumentException>(() => { seed.BorrowScratchWords(foreign, 1); });
        seed.CacheBits = 1;
        Assert.Throws<ArgumentException>(() => { seed.BorrowScratchWords(context, 1); });
        seed.CacheBits = 0; seed.Add(Vp8lToken.Literal(0x10203040));
        Assert.Throws<ArgumentException>(() => { seed.BorrowScratchWords(context, 1); });
        Assert.Equal(0x10203040u, seed.Tokens[0].Value);
        Assert.Equal(capacity * 8L, root.Bytes); Assert.Equal(0, root.Refusals);
        seed.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(131072, 512)] [InlineData(131328, 513)]
    public void MatchingAndGreedyUseOneOwnerWithoutChangingIndicesOrSurvivingTokens(int length, int width)
    {
        var pixels = Pixels(length); var original = pixels.ToArray();
        using var independentContext = new OperationContext(ImageLimits.Default, default);
        using var expected = Vp8lReferenceMatcher.Build(pixels, width, 32, false, independentContext);
        using var root = new TestMemoryBudget(length * 12L);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Vp8lTokenBuffer? seed = new(context, length);
        try
        {
            using var matches = Vp8lReferenceMatcher.Build(pixels, width, 32, false, context, seed);
            Assert.True(expected.Values.SequenceEqual(matches.Values));
            Assert.Equal(0, seed.Count); Assert.Equal(length * 12L, root.Bytes);
            Assert.Equal(length * 12L, root.Peak);
            var transferred = seed; seed = null;
            using var tokens = Vp8lReferenceLz.Greedy(pixels, matches.Values, context, transferred);
            Assert.Same(transferred, tokens);
            matches.Dispose(); Assert.Equal(length * 8L, root.Bytes);
            Assert.Equal(pixels, Replay(tokens, length)); Assert.Equal(original, pixels);
            tokens.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        finally { seed?.Dispose(); }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void RefusedOrCancelledMatchingCleansTheHandoffAndANewOperationRecovers(bool cancel)
    {
        const int length = 131072, width = 512;
        var pixels = Pixels(length);
        using var root = new TestMemoryBudget(length * 12L);
        IMemoryReservation? held = null;
        if (!cancel) Assert.True(root.TryReserve(1, out held));
        using var cancellation = new CancellationTokenSource();
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
        {
            using var seed = new Vp8lTokenBuffer(context, length);
            if (cancel) root.Admitted = _ => cancellation.Cancel();
            if (cancel)
                Assert.Throws<OperationCanceledException>(() => Vp8lReferenceMatcher.Build(pixels, width, 32, false, context, seed));
            else
            {
                var failure = Assert.Throws<ImageLimitException>(() => Vp8lReferenceMatcher.Build(pixels, width, 32, false, context, seed));
                Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope); Assert.Equal(1, root.Refusals);
            }
            root.Admitted = null;
            Assert.Equal(length * 8L + (held?.Bytes ?? 0), root.Bytes);
        }
        Assert.Equal(held?.Bytes ?? 0, root.Bytes); held?.Dispose(); Assert.Equal(0, root.Bytes);
        using (var next = new OperationContext(new() { MemoryBudget = root }, default))
        using (var seed = new Vp8lTokenBuffer(next, length))
        using (var matches = Vp8lReferenceMatcher.Build(pixels, width, 32, false, next, seed))
        {
            Assert.Equal(length * 12L, root.Bytes); Assert.Equal(0, seed.Count);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void OwnedSeedIsReleasedWhenGreedyOrMainParsingRejectsInputs()
    {
        using var root = new TestMemoryBudget(1024);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var greedySeed = new Vp8lTokenBuffer(context, 16);
        Assert.Throws<ArgumentException>(() => Vp8lReferenceLz.Greedy([], [], context, greedySeed));
        Assert.Equal(0, root.Bytes); Assert.Throws<ObjectDisposedException>(() => greedySeed.Count + greedySeed.Tokens.Length);
        var parsingSeed = new Vp8lTokenBuffer(context, 16);
        Assert.Throws<ArgumentException>(() => Vp8lReferenceMainParsing.Encode([], [], 1, false, 10, context, scratch: parsingSeed));
        Assert.Equal(0, root.Bytes); Assert.Throws<ObjectDisposedException>(() => parsingSeed.Tokens.Length);
        using var subsequent = new Vp8lTokenBuffer(context, 16); Assert.Equal(128, root.Bytes);
    }

    private static uint[] Pixels(int length)
        => Enumerable.Range(0, length).Select(i => 0xff000000u | (uint)(i % 37) * 0x00050307u).ToArray();

    private static uint[] Replay(Vp8lTokenBuffer tokens, int length)
    {
        var result = new uint[length]; var at = 0;
        foreach (var token in tokens.Tokens)
        {
            Assert.InRange(token.Code, 0, 4095);
            if (token.Code == 0) { Assert.True(at < length); result[at++] = token.Value; }
            else
            {
                Assert.InRange(token.Value, 1u, (uint)at); Assert.True(token.Code <= length - at);
                for (var i = 0; i < token.Code; i++, at++) result[at] = result[at - (int)token.Value];
            }
        }
        Assert.Equal(length, at); return result;
    }
}
