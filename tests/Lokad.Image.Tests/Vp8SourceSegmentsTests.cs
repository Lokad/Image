using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8SourceSegmentsTests
{
    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    public static IEnumerable<object[]> Controls()
    {
        yield return ["opaque", "872fd9e831d98ef6ac1363a90c431141a9bc8a63b8f2ecdf27e104fefc4cedcc", new[] { 0, 35, 108, 108 }, new[] { -18, 39, 127, 127 }, new[] { 0, 57, 178, 178 }, new[] { 27, 22, 15, 15 }, new[] { 8, 5, 2, 2 }, 3, 11, 4];
        yield return ["photograph", "ccf2215ba2875d875a3a844250221ad05df6d184f8a0a8a019fece337786b528", new[] { 69, 124, 174, 219 }, new[] { -127, -81, 3, 79 }, new[] { 0, 93, 178, 255 }, new[] { 36, 32, 25, 19 }, new[] { 11, 7, 4, 2 }, 4, 172, 6];
        yield return ["diagram", "d67c66d25d507e79371f3d9b66e2aefa1d911501dca93813ab0482773574d2f3", new[] { 17, 124, 178, 252 }, new[] { -127, -97, -39, 41 }, new[] { 0, 116, 174, 255 }, new[] { 36, 33, 29, 22 }, new[] { 11, 7, 5, 3 }, 4, 214, 4];
        yield return ["asymmetric", "5ae95290d4376ce3333e9ce4424c2944be2520455c0189167fd39b2ae6c8741a", new[] { 0, 53, 103, 103 }, new[] { -18, 70, 127, 127 }, new[] { 0, 88, 171, 171 }, new[] { 27, 20, 15, 15 }, new[] { 8, 5, 2, 2 }, 3, 11, 3];
    }

    [Theory]
    [MemberData(nameof(Controls))]
    public void AllFinalNativeMapsAndQuantizersMatchWithIndependentRetainedOwnership(string name, string expectedMap,
        int[] centers, int[] alpha, int[] beta, int[] quantizers, int[] initialFilters, int count, int mean, int iterations)
    {
        using var root = new TestMemoryBudget(16_000_000); using var image = OriginalVp8lControls.Buffer(name, 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var borrowed = image.Pixels.Length; context.ReserveBorrowed(borrowed);
        using var input = Vp8PreparedInput.Create(image, context); input.CleanupTransparentArea();
        using var analysis = Vp8SourceAnalysis.Create(input, context); var sourceAlpha = analysis.Alpha.ToArray(); var baseline = root.Bytes;
        using var result = Vp8SourceSegments.Create(analysis, 75, false, context);
        Assert.Equal(expectedMap, Hash(result.Map)); Assert.Equal((33, 17, count), (result.Width, result.Height, result.SegmentCount));
        Assert.Equal(new(mean, iterations), result.Statistics);
        var parameters = result.Parameters.ToArray();
        Assert.Equal(centers, parameters.Select(p => p.Center)); Assert.Equal(alpha, parameters.Select(p => p.Alpha));
        Assert.Equal(beta, parameters.Select(p => p.Beta)); Assert.Equal(quantizers, parameters.Select(p => p.Quantizer));
        Assert.Equal(initialFilters, parameters.Select(p => p.InitialFilter));
        Assert.Equal(sourceAlpha, analysis.Alpha.ToArray()); Assert.Equal(769, result.CapacityBytes);
        Assert.Equal(baseline + result.CapacityBytes, root.Bytes); Assert.Equal(root.Bytes + 1024, root.Peak);
        analysis.Dispose(); input.Dispose(); image.Dispose(); context.ReleaseBorrowed(borrowed);
        Assert.Equal(result.CapacityBytes, root.Bytes); Assert.Equal(expectedMap, Hash(result.Map));
        result.Dispose(); result.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => result.Map.Length); Assert.Throws<ObjectDisposedException>(() => result.Parameters.Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptyCentroidsAndCoincidentLowerTiesKeepTheirSpecifiedAssignments(bool same)
    {
        Span<int> histogram = stackalloc int[256]; histogram.Clear();
        if (same) histogram[128] = 9; else { histogram[0] = 1; histogram[255] = 1; }
        Span<byte> assignment = stackalloc byte[256]; Span<Vp8SourceSegments.Segment> segments = stackalloc Vp8SourceSegments.Segment[4];
        using var context = new OperationContext(ImageLimits.Default, default);
        var stats = Vp8SourceSegments.Cluster(histogram, assignment, segments, context);
        Assert.Equal(128, stats.Mean); Assert.Equal(same ? 1 : 2, stats.Iterations);
        Assert.Equal(same ? new[] { 128, 128, 128, 128 } : new[] { 0, 95, 159, 255 }, segments.ToArray().Select(p => p.Center));
        if (same) { Assert.Equal(0, assignment[128]); Assert.All(segments.ToArray(), p => Assert.Equal((0, 0), (p.Alpha, p.Beta))); }
        else { Assert.Equal((0, 3), ((int)assignment[0], (int)assignment[255])); Assert.Equal(new[] { -127, -33, 31, 127 }, segments.ToArray().Select(p => p.Alpha)); }
    }

    [Fact]
    public void HalfIntegralCentroidRoundsUp()
    {
        Span<int> histogram = stackalloc int[256]; histogram.Clear(); histogram[0] = histogram[1] = 1;
        Span<byte> assignment = stackalloc byte[256]; Span<Vp8SourceSegments.Segment> segments = stackalloc Vp8SourceSegments.Segment[1];
        using var context = new OperationContext(ImageLimits.Default, default);
        var stats = Vp8SourceSegments.Cluster(histogram, assignment, segments, context);
        Assert.Equal(new(1, 1), stats); Assert.Equal(1, segments[0].Center);
    }

    [Fact]
    public void MajoritySmoothingReadsOriginalNeighborsAndPreservesEdgesAndTails()
    {
        byte[] map = [1,1,1,1,1, 1,0,0,0,1, 1,1,0,0,0, 99];
        var replacement = new byte[16]; replacement[^1] = 98;
        using var context = new OperationContext(ImageLimits.Default, default);
        Vp8SourceSegments.SmoothMap(map.AsSpan(0, 15), 5, 3, replacement.AsSpan(0, 15), context);
        Assert.Equal(new byte[] { 1,1,1,1,1, 1,1,0,0,1, 1,1,0,0,0, 99 }, map);
        Assert.Equal(98, replacement[^1]); // In-place voting would incorrectly flip the next two interior cells.
    }

    [Theory]
    [InlineData(1, false)] [InlineData(2, false)] [InlineData(3, false)]
    [InlineData(1, true)] [InlineData(2, true)] [InlineData(3, true)]
    public void RefusalOrCancellationAtEachOwnedBoundaryCleansUpAndAllowsRecovery(int growth, bool cancel)
    {
        using var image = OriginalVp8lControls.Buffer("opaque", 8);
        using var root = new TestMemoryBudget(16_000_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var input = Vp8PreparedInput.Create(image, context); using var analysis = Vp8SourceAnalysis.Create(input, context);
        var baseline = root.Bytes; var attempts = root.GrowthAttempts;
        if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == attempts + growth) cancellation.Cancel(); };
        else root.RefuseAtGrowth = attempts + growth;
        if (cancel) Assert.Throws<OperationCanceledException>(() => Vp8SourceSegments.Create(analysis, 75, true, context));
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8SourceSegments.Create(analysis, 75, true, context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(1, root.Refusals);
        }
        Assert.Equal(baseline, root.Bytes); root.Admitted = null; root.RefuseAtGrowth = 0;
        using var later = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8SourceSegments.Create(analysis, 75, true, later); next.Dispose();
        Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RetainedResultChargesTheSharedReplacementPeak()
    {
        using var image = OriginalVp8lControls.Buffer("singleton", 8);
        using var root = new TestMemoryBudget(16_000_000); using var caller = root.CreateScope();
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(image, context); using var analysis = Vp8SourceAnalysis.Create(input, context);
        using var first = Vp8SourceSegments.Create(analysis, 75, false, context); var hash = Hash(first.Map);
        Assert.True(caller.TryReserve(16_000_000 - root.Bytes - first.CapacityBytes - 1023, out var held));
        var baseline = root.Bytes;
        Assert.Throws<ImageLimitException>(() => Vp8SourceSegments.Create(analysis, 75, false, context));
        Assert.Equal(baseline, root.Bytes); Assert.Equal(hash, Hash(first.Map)); held.Dispose();
        using var second = Vp8SourceSegments.Create(analysis, 75, false, context);
        Assert.Equal(input.CapacityBytes + analysis.CapacityBytes + first.CapacityBytes + second.CapacityBytes, root.Bytes);
        first.Dispose(); Assert.Equal(hash, Hash(second.Map));
    }
}
