using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lColorHistogramSupportTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PostCountingSupportPreservesNewMembershipAcrossEveryWordAndDensePopulations(bool red)
    {
        using var root = new TestMemoryBudget(1);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        foreach (var sparse in new[] { false, true })
        foreach (var prior in new[] { false, true })
        {
            var bins = sparse ? new[] { 0, 63, 64, 127, 128, 191, 192, 255 }
                : Enumerable.Range(0, 256).Select(i => (i * 73 + 17) & 255).ToArray();
            var pixels = bins.Select(b => (uint)(b | b << 16)).ToArray(); var original = pixels.ToArray();
            var guarded = Enumerable.Repeat(-913, 264).ToArray(); var active = new int[264];
            var expected = new int[256];
            for (var b = 0; b < 256; b++) guarded[b + 4] = expected[b] = prior && b % 3 != 0 ? b + 1 : 0;
            for (var pass = 0; pass < 2; pass++)
            {
                var before = expected.ToArray(); foreach (var bin in bins) expected[bin]++;
                Array.Fill(active, -913);
                var count = Vp8lReferenceColorTransform.Histogram(pixels, pixels.Length, 0, 0, pixels.Length, 1,
                    red, 0, 0, 0, guarded.AsSpan(4, 256), active.AsSpan(4, 256), context);
                var added = Enumerable.Range(0, 256).Where(b => before[b] == 0 && expected[b] != 0).ToArray();
                Assert.Equal(added.Length, count); Assert.Equal(added, active.AsSpan(4, count).ToArray());
                Assert.Equal(expected, guarded.AsSpan(4, 256).ToArray());
                Assert.All(guarded[..4].Concat(guarded[260..]), v => Assert.Equal(-913, v));
                Assert.All(active[..4].Concat(active[(count + 4)..]), v => Assert.Equal(-913, v));
            }
            Assert.Equal(original, pixels); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts);
        }
    }

    [Fact]
    public void EmptyRectangleAndExistingSupportDoNotPublishFalseNewBins()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var counts = Enumerable.Range(0, 256).ToArray(); var before = counts.ToArray();
        var active = Enumerable.Repeat(-3, 256).ToArray();
        Assert.Equal(0, Vp8lReferenceColorTransform.Histogram([], 0, 0, 0, 0, 0,
            false, 0, 0, 0, counts, active, context));
        Assert.Equal(before, counts); Assert.All(active, v => Assert.Equal(-3, v));
    }

    [Fact]
    public void FixedWorkspaceValidationAndCancellationPrecedeMutation()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var counts = new int[256]; var active = Enumerable.Repeat(-3, 256).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8lReferenceColorTransform.Histogram(new uint[1], 1, 0, 0, 1, 1,
            true, 0, 0, 0, counts.AsSpan(0, 255), active, context));
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8lReferenceColorTransform.Histogram(new uint[1], 1, 0, 0, 1, 1,
            false, 0, 0, 0, counts, active.AsSpan(0, 255), context));
        using var cancellation = new CancellationTokenSource();
        using var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => Vp8lReferenceColorTransform.Histogram([], 1, 0, 0, 1, 1,
            true, 0, 0, 0, [], [], canceled));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.All(counts, v => Assert.Equal(0, v)); Assert.All(active, v => Assert.Equal(-3, v));
    }
}
