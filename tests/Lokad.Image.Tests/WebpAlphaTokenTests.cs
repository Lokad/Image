using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpAlphaTokenTests
{
    private const int Width = 513, Height = 259;
    private static byte[] Samples()
    {
        var alpha = new byte[Width * Height];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            alpha[y * Width + x] = (byte)((((x * 12347 + y * 7919 + 3 * 23459 + x * y * 101) & 65535) + 128) / 257);
        alpha[0] = alpha[Width - 1] = 0; return alpha;
    }
    private static ImageLimits Limits(TestMemoryBudget root) => new() { MaxWork = 693048, MaxWorkingBytes = 1000000, MemoryBudget = root };

    [Fact]
    public void TokenReplayIsExactAndAccountsOverlappingOldAndNewStorage()
    {
        var alpha = Samples(); var unchanged = alpha.ToArray(); using var root = new TestMemoryBudget(1000000);
        using var context = new OperationContext(Limits(root), default);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); Assert.Equal(28332, plan.Length);
            Assert.InRange(root.Peak, 5L * alpha.Length + 64 + 2048, 1000000);
            Assert.Equal(alpha.Length + 32 + 2048, root.Bytes); // Only encoded capacity/tables survive matching.
            using var output = new MemoryStream(); output.WriteByte(plan.Header); plan.Write(alpha, Width, output);
            Assert.Equal(673642, context.ConsumedWork); Assert.Equal(plan.Length + 1, output.Length);
            using var decode = new OperationContext(ImageLimits.Default, default);
            Assert.Equal(alpha, WebpAlpha.Decode(output.ToArray(), Width, Height, decode));
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(unchanged, alpha);
    }

    [Fact]
    public void SmallerLocalWorkspaceSelectsFixedStagingBeforeSharedAdmission()
    {
        var alpha = Samples(); using var root = new TestMemoryBudget(300000);
        using var context = new OperationContext(Limits(root) with { MaxWorkingBytes = 250000 }, default);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); Assert.Equal(54551, plan.Length); Assert.InRange(root.Peak, 1, 250000);
            using var output = new MemoryStream(); plan.Write(alpha, Width, output);
        }
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
    }

    [Fact]
    public void AggregateRefusalCannotRetrySmallerStagingAndReleasePermitsSuccess()
    {
        var alpha = Samples(); using var root = new TestMemoryBudget(1000000);
        Assert.True(root.TryReserve(800000, out var retained));
        using (var context = new OperationContext(Limits(root), default))
        {
            var error = Assert.Throws<ImageLimitException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context));
            Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        }
        Assert.Equal(800000, root.Bytes); Assert.Equal(1, root.Refusals); retained.Dispose();
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next)) Assert.NotNull(plan);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void EveryRefusedTokenModelHeaderAndStageGrowthIsTerminalAndRefunded()
    {
        var alpha = Samples(); long attempts;
        using (var trace = new TestMemoryBudget(1000000))
        {
            using (var context = new OperationContext(Limits(trace), default))
            using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context)) Assert.NotNull(plan);
            attempts = trace.GrowthAttempts; Assert.InRange(attempts, 5, 30);
        }
        for (var phase = 1; phase <= attempts; phase++)
        {
            using var root = new TestMemoryBudget(1000000) { RefuseAtGrowth = phase };
            using (var context = new OperationContext(Limits(root), default))
            {
                var error = Assert.Throws<ImageLimitException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context));
                Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
            }
            Assert.Equal(0, root.Bytes); Assert.Equal(1, root.Refusals); root.RefuseAtGrowth = 0;
            using (var next = new OperationContext(Limits(root), default))
            using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next)) Assert.NotNull(plan);
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void CancellationAtTokenOrEncodedStageAdmissionCleansUpAllOwners(bool encodedStage)
    {
        var alpha = Samples(); var unchanged = alpha.ToArray(); using var cancellation = new CancellationTokenSource();
        var threshold = encodedStage ? 5L * alpha.Length + 64 + 2048 : 4L * alpha.Length + 32 + 2048;
        using var root = new TestMemoryBudget(1000000) { Admitted = bytes => { if (bytes >= threshold) cancellation.Cancel(); } };
        using (var context = new OperationContext(Limits(root), cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(unchanged, alpha); root.Admitted = null;
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next)) Assert.NotNull(plan);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void ActualOutputKeepsEncodedCapacityChargedUntilItsWriterFinishes(bool cancel)
    {
        var alpha = Samples(); using var root = new TestMemoryBudget(1000000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(Limits(root), cancellation.Token);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); var held = root.Bytes;
            using var output = new InterruptedOutput(() =>
            { Assert.Equal(held, root.Bytes); if (cancel) cancellation.Cancel(); else throw new IOException(); });
            if (cancel) Assert.Throws<OperationCanceledException>(() => plan.Write(alpha, Width, output));
            else Assert.Throws<IOException>(() => plan.Write(alpha, Width, output));
            Assert.Equal(held, root.Bytes); Assert.True(output.CanWrite);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void EveryActualCopyChargeRemainsCappedWithoutRetry()
    {
        var alpha = Samples(); using var root = new TestMemoryBudget(1000000);
        using (var context = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); context.Work(context.RemainingWork - plan.Length + 1);
            using var output = new MemoryStream(); var error = Assert.Throws<ImageLimitException>(() => plan.Write(alpha, Width, output));
            Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(ImageLimitScope.Operation, error.Scope);
            Assert.Equal(0, output.Length); Assert.True(root.Bytes > alpha.Length);
        }
        Assert.Equal(0, root.Bytes);
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next))
        { Assert.NotNull(plan); using var output = new MemoryStream(); plan.Write(alpha, Width, output); }
        Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void AnUneconomicalCompletedTokenCandidateReturnsRawWithoutKeepingTemporaryStorage()
    {
        var alpha = new byte[8192]; new Random(23).NextBytes(alpha); var unchanged = alpha.ToArray();
        using var root = new TestMemoryBudget(1000000);
        using var context = new OperationContext(Limits(root) with { MaxWork = 200000 }, default);
        Assert.Null(WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 128, 4, context));
        Assert.True(root.Peak > 4L * alpha.Length + 32); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
        Assert.Equal(unchanged, alpha);
    }

    private sealed class InterruptedOutput(Action interrupt) : MemoryStream
    { public override void Write(ReadOnlySpan<byte> buffer) { base.Write(buffer); interrupt(); } }
}
