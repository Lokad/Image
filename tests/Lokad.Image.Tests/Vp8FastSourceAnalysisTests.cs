using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8FastSourceAnalysisTests
{
    [Theory]
    [InlineData(0, 244, 5, "4bb54aa7c6acd73bd3a905683e49f099091f40888410bf6030fd4a07ff4fcf19", "2ba36f3b1ef7f2bed22c2d1ee22c9f41e6e0919ce1dba5095c6ae96774893166")]
    [InlineData(1, 244, 5, "4bb54aa7c6acd73bd3a905683e49f099091f40888410bf6030fd4a07ff4fcf19", "2ba36f3b1ef7f2bed22c2d1ee22c9f41e6e0919ce1dba5095c6ae96774893166")]
    [InlineData(4, 213, 0, "97025ac1bb5876852386d00a0568744e60583d97b47a03496a526bd1c249fc95", "17535e17d4998767b7c4ec8c67f2b0d6db34d2baeff5d3d0d4f3355f62aa670d")]
    public void AllSourceRecordsMatchNativeBeforeSegmentation(int method, int mean, int intra4, string expected, string histogram)
    {
        using var root = new TestMemoryBudget(16_000_000); using var source = OriginalVp8lControls.Buffer("diagram", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var input = Vp8PreparedInput.Create(source, context);
        var original = Vp8PreparedInputTests.Hash(input); var before = root.Bytes;
        using var plan = Vp8SourceAnalysis.Create(input, context, method, 75);
        Assert.Equal((561, mean, 42), (plan.Count, plan.MeanAlpha, plan.MeanChromaScore));
        using var records = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); Span<byte> bytes = stackalloc byte[4]; var small = 0;
        for (var i = 0; i < plan.Count; i++)
        {
            var mode = plan.LumaModes[i]; bytes[0] = plan.Alpha[i]; bytes[1] = mode == 2 ? (byte)0 : (byte)1;
            bytes[2] = mode == 2 ? (byte)0 : mode; bytes[3] = plan.ChromaModes[i]; small += mode == 2 ? 1 : 0; records.AppendData(bytes);
        }
        Assert.Equal(intra4, small); Assert.Equal(expected, Convert.ToHexStringLower(records.GetHashAndReset()));
        using var counts = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var n in plan.Histogram) { BinaryPrimitives.WriteInt32LittleEndian(bytes, n); counts.AppendData(bytes); }
        Assert.Equal(histogram, Convert.ToHexStringLower(counts.GetHashAndReset())); Assert.Equal(original, Vp8PreparedInputTests.Hash(input));
        Assert.Equal(before + 2835, root.Bytes); source.Dispose(); input.Dispose(); Assert.Equal(plan.CapacityBytes, root.Bytes);
        plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void FastOwnedAndScratchRefusalAreTerminalAndRecoverAfterCallerRelease(int phase)
    {
        using var root = new TestMemoryBudget(100_000); using var source = OriginalVp8lControls.Buffer("hidden", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var input = Vp8PreparedInput.Create(source, context);
        var before = root.Bytes; root.RefuseAtGrowth = root.GrowthAttempts + phase;
        var error = Assert.Throws<ImageLimitException>(() => Vp8SourceAnalysis.Create(input, context, 0));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope)); Assert.Equal(1, root.Refusals);
        Assert.Equal(before, root.Bytes); root.RefuseAtGrowth = 0; using var next = Vp8SourceAnalysis.Create(input, context, 1);
        next.Dispose(); Assert.Equal(before, root.Bytes);
    }

    [Fact]
    public void FastCancellationAndLateWorkFailureRefundEveryNewOwner()
    {
        using var root = new TestMemoryBudget(100_000); using var source = OriginalVp8lControls.Buffer("hidden", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var input = Vp8PreparedInput.Create(source, context); var before = root.Bytes;
        using var cancel = new CancellationTokenSource(); using var stopped = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        root.Admitted = _ => cancel.Cancel(); Assert.Throws<OperationCanceledException>(() => Vp8SourceAnalysis.Create(input, stopped, 0));
        Assert.Equal(before, root.Bytes); root.Admitted = null; long work;
        using (var sizing = new OperationContext(new() { MemoryBudget = root }, default))
        { using var plan = Vp8SourceAnalysis.Create(input, sizing, 0); work = sizing.ConsumedWork; }
        using var limited = new OperationContext(new() { MemoryBudget = root, MaxWork = work - 1 }, default);
        Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => Vp8SourceAnalysis.Create(input, limited, 0)).Limit);
        Assert.Equal(before, root.Bytes); using var recovered = Vp8SourceAnalysis.Create(input, context, 1); recovered.Dispose(); Assert.Equal(before, root.Bytes);
    }

    [Theory]
    [InlineData(-1, 75)]
    [InlineData(7, 75)]
    [InlineData(0, 0)]
    [InlineData(1, 101)]
    public void InvalidOptionsAcquireNoNewCapacity(int method, int quality)
    {
        using var root = new TestMemoryBudget(100_000); using var source = OriginalVp8lControls.Buffer("singleton", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var input = Vp8PreparedInput.Create(source, context);
        var before = root.Bytes; var attempts = root.Attempts;
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8SourceAnalysis.Create(input, context, method, quality));
        Assert.Equal(before, root.Bytes); Assert.Equal(attempts, root.Attempts);
    }
}
