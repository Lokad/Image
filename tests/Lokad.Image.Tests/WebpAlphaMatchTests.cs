using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpAlphaMatchTests
{
    private const int Width = 129, Height = 67;
    private static byte[] Samples()
    {
        var alpha = new byte[Width * Height];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            alpha[y * Width + x] = (byte)((((x * 12347 + y * 7919 + 3 * 23459 + x * y * 101) & 65535) + 128) / 257);
        alpha[0] = alpha[^1] = 0; return alpha;
    }

    [Fact]
    public void ExactBackwardReferencesBeatTheFixedCandidateWithinTheirDeclaredWorkAndMemory()
    {
        var alpha = Samples(); var unchanged = alpha.ToArray();
        using var fixedContext = new OperationContext(new ImageLimits { MaxWorkingBytes = 2048 }, default);
        using var fixedPlan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, fixedContext);
        Assert.NotNull(fixedPlan);
        using var root = new TestMemoryBudget(300000);
        var bound = alpha.Length * 96L + 5_200_000;
        using var context = new OperationContext(new ImageLimits { MaxWork = bound, MaxWorkingBytes = 300000, MemoryBudget = root }, default);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); Assert.True(plan.Length < fixedPlan.Length);
            Assert.InRange(root.Peak, 262176, 300000); Assert.True(root.Bytes > 262176);
            using var output = new MemoryStream(); output.WriteByte(plan.Header); plan.Write(alpha, Width, output);
            Assert.Equal(plan.Length + 1, output.Length); Assert.InRange(context.ConsumedWork, 1, bound);
            using var decode = new OperationContext(ImageLimits.Default, default);
            Assert.Equal(alpha, WebpAlpha.Decode(output.ToArray(), Width, Height, decode));
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(unchanged, alpha);
    }

    [Fact]
    public void EveryRefusedGrowthIsTerminalAndAllOperationChargesAreReleased()
    {
        var alpha = Samples(); long attempts;
        using (var baseline = new TestMemoryBudget(300000))
        {
            using (var context = new OperationContext(new ImageLimits { MemoryBudget = baseline }, default))
            using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
            { Assert.NotNull(plan); using var output = new MemoryStream(); plan.Write(alpha, Width, output); }
            attempts = baseline.GrowthAttempts; Assert.InRange(attempts, 3, 40);
        }
        for (var phase = 1; phase <= attempts; phase++)
        {
            using var root = new TestMemoryBudget(300000) { RefuseAtGrowth = phase };
            using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
            {
                var failure = Assert.Throws<ImageLimitException>(() =>
                {
                    using var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context);
                    Assert.NotNull(plan); using var output = new MemoryStream(); plan.Write(alpha, Width, output);
                });
                Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
            }
            Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            root.RefuseAtGrowth = 0;
            using (var next = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
            using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, next)) Assert.NotNull(plan);
            Assert.Equal(0, root.Bytes);
        }
    }

    [Fact]
    public void CancellationAfterHistoryAdmissionCleansUpAndPreservesBorrowedStorage()
    {
        var alpha = Samples(); var unchanged = alpha.ToArray();
        using var cancellation = new CancellationTokenSource();
        using var root = new TestMemoryBudget(300000) { Admitted = used => { if (used > 2048) cancellation.Cancel(); } };
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(unchanged, alpha);
        root.Admitted = null;
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context)) Assert.NotNull(plan);
        Assert.Equal(0, root.Bytes);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void InterruptedAdaptiveOutputKeepsHistoryChargedUntilItsOwnerEnds(bool cancel)
    {
        var alpha = Samples(); using var root = new TestMemoryBudget(300000);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, cancellation.Token);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
        {
            Assert.NotNull(plan); var owned = root.Bytes; Assert.True(owned > 262176);
            using var output = new InterruptOutput(() =>
            { Assert.True(root.Bytes >= owned); if (cancel) cancellation.Cancel(); else throw new IOException(); });
            if (cancel) Assert.Throws<OperationCanceledException>(() => plan.Write(alpha, Width, output));
            else Assert.Throws<IOException>(() => plan.Write(alpha, Width, output));
            Assert.Equal(owned, root.Bytes); Assert.True(output.CanWrite);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void TightLocalAllowancesSelectTheSameFixedPacketWithoutSharedRefusal()
    {
        var alpha = Samples(); byte[]? expected = null;
        foreach (var limits in new[] { new ImageLimits { MaxWorkingBytes = 2048 },
            new ImageLimits { MaxWork = alpha.Length * 4L + 2048 } })
        {
            using var root = new TestMemoryBudget(2048);
            using (var context = new OperationContext(limits with { MemoryBudget = root }, default))
            using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, Width, 4, context))
            {
                Assert.NotNull(plan); Assert.Equal(2048, root.Peak);
                using var output = new MemoryStream(); plan.Write(alpha, Width, output);
                if (expected is null) expected = output.ToArray(); else Assert.Equal(expected, output.ToArray());
            }
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
        }
    }

    private delegate void DistanceCode(int distance, int width, out int code, out int extra, out int bits);
    [Theory] [InlineData(1)] [InlineData(129)] [InlineData(128)] [InlineData(65535)] [InlineData(1048456)]
    public void DistancePrefixesReconstructTheDeclaredBackwardOffset(int distance)
    {
        var encode = typeof(WebpLosslessEncoder.AlphaEncoding).GetMethod("Distance", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<DistanceCode>();
        encode(distance, Width, out var code, out var extra, out var bits);
        Assert.InRange(code, 0, 39); Assert.InRange(extra, 0, (1 << bits) - 1);
        var value = code < 4 ? code + 1 : ((2 + (code & 1)) << ((code - 2) / 2)) + extra + 1;
        Assert.Equal(distance, value == 1 ? Width : value == 2 ? 1 : value - 120);
    }

    private sealed class InterruptOutput(Action interrupt) : MemoryStream
    { public override void WriteByte(byte value) { base.WriteByte(value); interrupt(); } }
}
