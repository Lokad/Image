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
}
