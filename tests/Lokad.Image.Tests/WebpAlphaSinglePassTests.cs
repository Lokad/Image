using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpAlphaSinglePassTests
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
    private static ImageLimits Limits(TestMemoryBudget root, long work = 693048)
        => new() { MaxWork = work, MemoryBudget = root };

    [Fact]
    public void StagedBackwardReferencesAreExactAndOnlyRetainedCapacityRemainsCharged()
    {
        var alpha = Samples(); var unchanged = alpha.ToArray(); using var root = new TestMemoryBudget(300000);
        using var context = new OperationContext(Limits(root), default);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); Assert.Equal(54551, plan.Length);
            Assert.Equal(2048 + alpha.Length + 32, root.Bytes);
            Assert.Equal(2048 + alpha.Length + 32 + 65568, root.Peak);
            using var output = new MemoryStream(); output.WriteByte(plan.Header); plan.Write(alpha, Width, output);
            Assert.Equal(plan.Length + 1, output.Length); Assert.Equal(690559, context.ConsumedWork);
            using var decode = new OperationContext(ImageLimits.Default, default);
            Assert.Equal(alpha, WebpAlpha.Decode(output.ToArray(), Width, Height, decode));
        }
        Assert.Equal(unchanged, alpha); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ACompletedLargerCandidateReleasesItsStageAndUsesTheExactRunOnlyPacket()
    {
        var alpha = new byte[8195]; using var root = new TestMemoryBudget(100000);
        using var context = new OperationContext(Limits(root, 70000), default);
        using var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 1, 4, context); Assert.NotNull(plan);
        Assert.Equal(2048, root.Bytes); Assert.True(root.Peak > 65568);
        using var fixedContext = new OperationContext(new ImageLimits { MaxWorkingBytes = 2048 }, default);
        using var fixedPlan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 1, 4, fixedContext); Assert.NotNull(fixedPlan);
        using var actual = new MemoryStream(); using var expected = new MemoryStream();
        plan.Write(alpha, 1, actual); fixedPlan.Write(alpha, 1, expected);
        Assert.Equal(expected.ToArray(), actual.ToArray()); Assert.Equal(plan.Length, actual.Length);
        using var decode = new OperationContext(ImageLimits.Default, default);
        Assert.Equal(alpha, WebpAlpha.Decode(new[] { plan.Header }.Concat(actual.ToArray()).ToArray(), 1, alpha.Length, decode));
    }

    [Fact]
    public void ByteCapacityOverflowIsAnOrdinaryNoGainResultAndReleasesEveryTemporary()
    {
        var alpha = new byte[8192]; new Random(23).NextBytes(alpha); var unchanged = alpha.ToArray();
        using var root = new TestMemoryBudget(100000); using var context = new OperationContext(Limits(root, 100000), default);
        Assert.Null(WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 128, 4, context));
        Assert.Equal(0, root.Bytes); Assert.True(root.Peak > 65568); Assert.Equal(0, root.Refusals); Assert.Equal(unchanged, alpha);
    }

    [Fact]
    public void ActualWorkExhaustionCannotRetryTheRunOnlyEncoding()
    {
        var alpha = Samples(); using var root = new TestMemoryBudget(300000);
        using (var context = new OperationContext(Limits(root, 686000), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); using var output = new MemoryStream();
            var error = Assert.Throws<ImageLimitException>(() => plan.Write(alpha, Width, output));
            Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(ImageLimitScope.Operation, error.Scope);
            Assert.Equal(0, output.Length); Assert.True(root.Bytes > alpha.Length);
        }
        Assert.Equal(0, root.Bytes);
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next))
        { Assert.NotNull(plan); using var output = new MemoryStream(); plan.Write(alpha, Width, output); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(1)] [InlineData(2)]
    public void EveryAdmissionRefusalIsTerminalAndReleasingCapacityPermitsSuccess(int phase)
    {
        var alpha = Samples(); using var root = new TestMemoryBudget(300000) { RefuseAtGrowth = phase };
        using (var context = new OperationContext(Limits(root), default))
        {
            var error = Assert.Throws<ImageLimitException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context));
            Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        }
        Assert.Equal(0, root.Bytes); Assert.Equal(1, root.Refusals); root.RefuseAtGrowth = 0;
        using (var context = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context)) Assert.NotNull(plan);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAtStagingAdmissionCleansUpAndBorrowsInputAndRoot()
    {
        var alpha = Samples(); var unchanged = alpha.ToArray(); using var cancellation = new CancellationTokenSource();
        using var root = new TestMemoryBudget(300000) { Admitted = bytes => { if (bytes > 2048) cancellation.Cancel(); } };
        using (var context = new OperationContext(Limits(root), cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(unchanged, alpha); root.Admitted = null;
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next)) Assert.NotNull(plan);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void InterruptedOutputKeepsStagingChargedUntilItsActualWriterEnds(bool cancel)
    {
        var alpha = Samples(); using var root = new TestMemoryBudget(300000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(Limits(root), cancellation.Token);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); var retained = root.Bytes;
            using var output = new InterruptOutput(() =>
            { Assert.Equal(retained, root.Bytes); if (cancel) cancellation.Cancel(); else throw new IOException(); });
            if (cancel) Assert.Throws<OperationCanceledException>(() => plan.Write(alpha, Width, output));
            else Assert.Throws<IOException>(() => plan.Write(alpha, Width, output));
            Assert.Equal(retained, root.Bytes); Assert.True(output.CanWrite);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private sealed class InterruptOutput(Action interrupt) : MemoryStream
    { public override void Write(ReadOnlySpan<byte> buffer) { base.Write(buffer); interrupt(); } }
}
