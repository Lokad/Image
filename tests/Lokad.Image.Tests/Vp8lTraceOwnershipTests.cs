using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lTraceOwnershipTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TraceScratchRefusalOrCancellationPreservesBorrowedInputsAndAllowsIndependentRecovery(bool cancel)
    {
        using var root = new TestMemoryBudget(8_000_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        var pixels = Enumerable.Range(0, 129).Select(i => 0xff000000u | (uint)((i * 17 + i / 7) % 19) * 0x030509u).ToArray();
        var original = pixels.ToArray();
        using var matches = Vp8lReferenceMatcher.Build(pixels, 43, 75, false, context);
        using var seed = Vp8lReferenceLz.Greedy(pixels, matches.Values, context);
        var originalMatches = matches.Values.ToArray(); var originalSeed = seed.Tokens.ToArray();
        var baseline = root.Bytes; var first = root.GrowthAttempts + 1; var scratch = first + 1;
        root.Admitted = _ =>
        {
            if (root.GrowthAttempts == first) Assert.Equal(baseline + pixels.Length * 8L, root.Bytes);
            if (root.GrowthAttempts == scratch)
            {
                Assert.True(root.Bytes > baseline + pixels.Length * 8L);
                if (cancel) cancellation.Cancel();
            }
        };
        if (!cancel) root.RefuseAtGrowth = scratch;
        if (cancel) Assert.Throws<OperationCanceledException>(() => Vp8lReferenceLz.Trace(pixels, matches.Values, 43, seed.Tokens, context));
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceLz.Trace(pixels, matches.Values, 43, seed.Tokens, context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(scratch, root.GrowthAttempts); Assert.Equal(cancel ? 0 : 1, root.Refusals);
        Assert.Equal(baseline, root.Bytes); Assert.Equal(original, pixels);
        Assert.Equal(originalMatches, matches.Values.ToArray()); Assert.Equal(originalSeed, seed.Tokens.ToArray());
        root.Admitted = null; root.RefuseAtGrowth = 0;
        using var later = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferenceLz.Trace(pixels, matches.Values, 43, seed.Tokens, later);
        Assert.Equal(baseline + pixels.Length * 8L, root.Bytes);
        var decoded = new List<uint>();
        foreach (var token in next.Tokens)
        {
            Assert.True(token.Code >= 0);
            if (token.Code == 0) decoded.Add(token.Value);
            else for (var i = 0; i < token.Code; i++) decoded.Add(decoded[decoded.Count - (int)token.Value]);
        }
        Assert.Equal(original, decoded.ToArray()); next.Dispose(); next.Dispose();
        Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(originalMatches, matches.Values.ToArray()); Assert.Equal(originalSeed, seed.Tokens.ToArray());
    }

    private static uint[] Pixels() => Enumerable.Range(0, 64)
        .Select(i => 0xff000000u | (uint)((i % 8) * 17) << 8 | (uint)(i / 16)).ToArray();

    [Fact]
    public void RejectedCandidateCapacityCanBeReusedWithoutChangingSeedOrPixels()
    {
        var pixels = Pixels(); using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var matches = Vp8lReferenceMatcher.Build(pixels, 8, 75, false, context);
        using var seed = Vp8lReferenceLz.Greedy(pixels, matches.Values, context);
        var originalSeed = seed.Tokens.ToArray();
        var scratch = Vp8lReferenceLz.Repeats(pixels, 8, context); var scratchBytes = scratch.CapacityBytes;
        var before = root.Bytes;
        using (var trace = Vp8lReferenceLz.Trace(pixels, matches.Values, 8, seed.Tokens, context, scratch: scratch))
        {
            var decoded = new List<uint>();
            foreach (var token in trace.Tokens)
            {
                if (token.Code == 0) decoded.Add(token.Value);
                else
                {
                    Assert.True(token.Code > 0);
                    for (var i = 0; i < token.Length; i++) decoded.Add(decoded[decoded.Count - (int)token.Value]);
                }
            }
            Assert.Equal(pixels, decoded); Assert.True(seed.Tokens.SequenceEqual(originalSeed));
        }
        Assert.Equal(before - scratchBytes, root.Bytes); Assert.True(seed.Tokens.SequenceEqual(originalSeed));
        seed.Dispose(); matches.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ReusedTraceFailureReleasesScratchAndLeavesSeedAndRootUsable(bool cancel)
    {
        var pixels = Pixels(); using var root = new TestMemoryBudget(1_000_000);
        using (var cancellation = new CancellationTokenSource())
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
        using (var matches = Vp8lReferenceMatcher.Build(pixels, 8, 75, false, context))
        using (var seed = Vp8lReferenceLz.Greedy(pixels, matches.Values, context))
        {
            var originalSeed = seed.Tokens.ToArray();
            var scratch = Vp8lReferenceLz.Repeats(pixels, 8, context);
            var remaining = root.Bytes - scratch.CapacityBytes;
            if (cancel) cancellation.Cancel(); else root.RefuseAtGrowth = root.GrowthAttempts + 1;
            if (cancel) Assert.Throws<OperationCanceledException>(() => Vp8lReferenceLz.Trace(pixels, matches.Values, 8, seed.Tokens, context, scratch: scratch));
            else Assert.Throws<ImageLimitException>(() => Vp8lReferenceLz.Trace(pixels, matches.Values, 8, seed.Tokens, context, scratch: scratch));
            Assert.Equal(remaining, root.Bytes); Assert.True(seed.Tokens.SequenceEqual(originalSeed));
        }
        Assert.Equal(0, root.Bytes); root.RefuseAtGrowth = 0;
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        using (var matches = Vp8lReferenceMatcher.Build(pixels, 8, 75, false, context))
        using (var parsed = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 8, false, 10, context))
            Assert.True(parsed.Count > 0);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
