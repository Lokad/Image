using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpAlphaAnchorTests
{
    private const int Width = 513, Height = 259, Count = Width * Height;
    private const long MatchingBytes = 2048 + 2 * 65568 + 4L * Count + 32;
    private static byte[] Samples()
    {
        var alpha = new byte[Count];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            alpha[y * Width + x] = (byte)((((x * 12347 + y * 7919 + 3 * 23459 + x * y * 101) & 65535) + 128) / 257);
        alpha[0] = alpha[Width - 1] = 0; return alpha;
    }
    private static ImageLimits Limits(TestMemoryBudget root) => new()
    { MaxWork = 756697, MaxWorkingBytes = 1000000, MemoryBudget = root };

    [Fact]
    public void AnchorsPreserveExactAlphaAndChargeBothHistoriesBeforeMatching()
    {
        var alpha = Samples(); var unchanged = alpha.ToArray(); var admissions = new List<long>();
        using var root = new TestMemoryBudget(1000000) { Admitted = admissions.Add };
        using var context = new OperationContext(Limits(root), default);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); Assert.Equal(19245, plan.Length); Assert.Equal(MatchingBytes, admissions[1]);
            // Histories have ended; old tokens overlap new encoded capacity
            // during replacement. Only the encoded owner/tables survive.
            Assert.True(root.Peak >= 5L * Count + 64 + 2048); Assert.Equal(Count + 32 + 2048, root.Bytes);
            using var output = new MemoryStream(); output.WriteByte(plan.Header); plan.Write(alpha, Width, output);
            Assert.Equal(704388, context.ConsumedWork); Assert.Equal(19246, output.Length);
            using var decode = new OperationContext(ImageLimits.Default, default);
            Assert.Equal(alpha, WebpAlpha.Decode(output.ToArray(), Width, Height, decode));
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(unchanged, alpha);
    }

    [Theory] [InlineData(780000, 756697)] [InlineData(1000000, 693048)]
    public void IndependentLocalAllowanceDeclinesAnchorsBeforeSharedAdmission(long memory, long work)
    {
        var alpha = Samples(); var admissions = new List<long>();
        using var root = new TestMemoryBudget(1000000) { Admitted = admissions.Add };
        using var context = new OperationContext(Limits(root) with { MaxWorkingBytes = memory, MaxWork = work }, default);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); Assert.Equal(28332, plan.Length); Assert.Equal(2048 + 65568 + 4L * Count + 32, admissions[1]);
            using var output = new MemoryStream(); plan.Write(alpha, Width, output); Assert.Equal(673642, context.ConsumedWork);
        }
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void AggregateRefusalCannotRetryAnOtherwiseAdmissibleSingleHistory()
    {
        var alpha = Samples(); using var root = new TestMemoryBudget(1000000);
        Assert.True(root.TryReserve(350000, out var retained));
        // The older matching owner would fit the shared root; the selected
        // two-history owner must fail, without observing a separate allowance.
        Assert.True(350000 + 2048 + 65568 + 4L * Count + 32 < 1000000);
        using (var context = new OperationContext(Limits(root), default))
        {
            var error = Assert.Throws<ImageLimitException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context));
            Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        }
        Assert.Equal(350000, root.Bytes); Assert.Equal(1, root.Refusals); retained.Dispose();
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next))
        { Assert.NotNull(plan); Assert.Equal(19245, plan.Length); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RefusalAtEveryOwnerOrModelGrowthRefundsAllCapacity()
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
    public void CancellationAtMatchingOrReplacementAdmissionCleansUp(bool replacement)
    {
        var alpha = Samples(); var unchanged = alpha.ToArray(); using var cancellation = new CancellationTokenSource();
        var threshold = replacement ? 5L * Count + 64 + 2048 : MatchingBytes;
        using var root = new TestMemoryBudget(1000000) { Admitted = used => { if (used >= threshold) cancellation.Cancel(); } };
        using (var context = new OperationContext(Limits(root), cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(unchanged, alpha); root.Admitted = null;
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next)) Assert.NotNull(plan);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ActualMatchingWorkExhaustionIsTerminalAndSubsequentOperationSucceeds()
    {
        var alpha = Samples(); var unchanged = alpha.ToArray(); using var root = new TestMemoryBudget(1000000);
        using (var context = new OperationContext(Limits(root), default))
        {
            root.Admitted = used => { if (used == MatchingBytes) context.Work(context.RemainingWork - 32768 - 10000); };
            var error = Assert.Throws<ImageLimitException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context));
            Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(ImageLimitScope.Operation, error.Scope);
        }
        Assert.Equal(0, root.Bytes); Assert.Equal(unchanged, alpha); Assert.Equal(0, root.Refusals); root.Admitted = null;
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next)) Assert.NotNull(plan);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(64)] [InlineData(65)] [InlineData(66)] [InlineData(67)]
    public void FourResidualKeyHandlesUnsignedMaximumAndEveryTail(int count)
    {
        // Every horizontal residual is255, including the first pixel. The
        // polynomial key exceeds Int32, and the final three pixels lack a key.
        var alpha = Enumerable.Range(0, count).Select(i => (byte)(255 - i)).ToArray();
        var unchanged = alpha.ToArray(); using var root = new TestMemoryBudget(1000000);
        using (var context = new OperationContext(Limits(root), default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, count, 4, context))
        {
            Assert.NotNull(plan); using var output = new MemoryStream(); output.WriteByte(plan.Header);
            plan.Write(alpha, count, output);
            using var decode = new OperationContext(ImageLimits.Default, default);
            Assert.Equal(alpha, WebpAlpha.Decode(output.ToArray(), count, 1, decode));
        }
        Assert.Equal(unchanged, alpha); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
