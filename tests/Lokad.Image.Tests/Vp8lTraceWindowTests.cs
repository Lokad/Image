using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferenceLzTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lTraceWindowTests
{
    [Theory]
    [InlineData(1, 8191, 0, 0, 4, 4607705087L, "a1704bb475b3d45a138d05ffafb1625f90db008f7501d32b671123f604c1eba6", "73a65a4d6ed15236e6053733eb74d8b40ef42f0f916096d39416c5b022c59c56")]
    [InlineData(1, 8192, 0, 0, 4, 4607705087L, "d7be1cefd576e1778419b42ebc4652d0c59f95c79d04917b66ce4e9753fee038", "d7be1cefd576e1778419b42ebc4652d0c59f95c79d04917b66ce4e9753fee038")]
    [InlineData(1, 8193, 0, 10, 5, 6706135150L, "5a89c73c4ada96de07a4c17a87952fbd46b512512bfc2cbc13863a0d8ef28d56", "bb34518c5babfae364826b78a2f9f23afa90dd31f016e2f17748ec91e6a1b469")]
    [InlineData(65, 513, 1, 10, 34, 9614597994L, "78b677850b103b0c3b81107133305e91f93a794010b5324aaf079d0a3e7a09d3", "c8632e262ce586b69be7cc193871d5274052d325c43cfc87d288710a940f8fd2")]
    [InlineData(65, 513, 2, 0, 11345, 3061719652022L, "d0498f9635e6a89cfcdcffc817dc97b67154cf92aab5a265d9681b655918200e", "d0498f9635e6a89cfcdcffc817dc97b67154cf92aab5a265d9681b655918200e")]
    public void LookaheadAndRepeatedWrapsPreserveFrozenTraceChoices(int width, int height, int pattern, int bits, int count, long expectedCost, string expectedHash, string expectedSeedHash)
    {
        var pixels = Pixels(width * height, pattern); var original = pixels.ToArray();
        using var root = new TestMemoryBudget(64_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var matches = Vp8lReferenceMatcher.Build(pixels, width, 75, false, context);
        var indices = matches.Values.ToArray();
        using var seed = Vp8lReferenceLz.Greedy(pixels, matches.Values, context);
        Vp8lReferenceCache.Apply(pixels, seed, bits, context);
        var seedHash = SemanticHash(seed.Tokens, width); var baseline = root.Bytes;
        using var result = Vp8lReferenceLz.Trace(pixels, matches.Values, width, seed.Tokens, context, bits);
        var hash = SemanticHash(result.Tokens, width);
        var cost = Vp8lReferenceLz.Estimate(result.Tokens, width, context, bits);
        Assert.Equal(count, result.Count); Assert.Equal(expectedCost, cost);
        Assert.Equal(expectedHash, hash); Assert.Equal(expectedSeedHash, seedHash);
        Assert.Equal(pixels, Reconstruct(result.Tokens, pixels.Length, bits));
        Assert.Equal(original, pixels); Assert.True(matches.Values.SequenceEqual(indices));
        Assert.Equal(seedHash, SemanticHash(seed.Tokens, width));
        Assert.Equal(baseline + pixels.Length * 8L, root.Bytes);
        seed.Dispose(); matches.Dispose(); Assert.Equal(pixels.Length * 8L, root.Bytes);
        Assert.Equal(pixels, Reconstruct(result.Tokens, pixels.Length, bits));
        result.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BoundedStorageAdmissionFailureReleasesOutputAndAllowsSubsequentTracing(bool cancel)
    {
        var pixels = Pixels(65 * 513, 1);
        using var root = new TestMemoryBudget(16_000_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var matches = Vp8lReferenceMatcher.Build(pixels, 65, 75, false, context);
        using var seed = Vp8lReferenceLz.Greedy(pixels, matches.Values, context);
        Vp8lReferenceCache.Apply(pixels, seed, 10, context);
        var baseline = root.Bytes; var stateAdmission = root.GrowthAttempts + 2;
        if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == stateAdmission) cancellation.Cancel(); };
        else root.RefuseAtGrowth = stateAdmission;
        if (cancel)
            Assert.Throws<OperationCanceledException>(() => Vp8lReferenceLz.Trace(pixels, matches.Values, 65, seed.Tokens, context, 10));
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceLz.Trace(pixels, matches.Values, 65, seed.Tokens, context, 10));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(stateAdmission, root.GrowthAttempts); Assert.Equal(cancel ? 0 : 1, root.Refusals);
        Assert.Equal(baseline, root.Bytes); root.Admitted = null; root.RefuseAtGrowth = 0;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferenceLz.Trace(pixels, matches.Values, 65, seed.Tokens, nextContext, 10);
        Assert.Equal("78b677850b103b0c3b81107133305e91f93a794010b5324aaf079d0a3e7a09d3", SemanticHash(next.Tokens, 65));
        Assert.Equal(pixels, Reconstruct(next.Tokens, pixels.Length, 10));
        next.Dispose(); Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static uint[] Pixels(int count, int pattern)
    {
        var pixels = new uint[count]; uint random = 0x12345678;
        for (var at = 0; at < count; at++)
        {
            random ^= random << 13; random ^= random >> 17; random ^= random << 5;
            var period = 3 + (at / 4096 % 3) * 2;
            pixels[at] = pattern switch
            {
                0 => 0xff4280e1,
                1 => unchecked(0x01010101u * (uint)(at % period + 1)),
                _ => at / 257 % 3 == 0 ? random : unchecked(0x01020305u * (uint)(at % 17))
            };
        }
        return pixels;
    }

    private static uint[] Reconstruct(ReadOnlySpan<Vp8lToken> tokens, int count, int bits)
    {
        var pixels = new uint[count]; var cache = new uint[bits == 0 ? 0 : 1 << bits]; var at = 0;
        foreach (var token in tokens)
        {
            for (var i = 0; i < token.Length; i++)
            {
                var pixel = token.Code > 0 ? pixels[at - checked((int)token.Value)]
                    : token.Code < 0 ? cache[~token.Code] : token.Value;
                pixels[at++] = pixel;
                if (bits != 0) cache[Vp8lReferenceCache.Key(pixel, bits)] = pixel;
            }
        }
        Assert.Equal(count, at); return pixels;
    }
}
