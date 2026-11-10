using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosWordPairTests
{
    private static bool Supported => Avx512F.IsSupported && Avx512DQ.IsSupported && Avx512DQ.VL.IsSupported
        && Avx2.IsSupported && Fma.IsSupported && Sse41.IsSupported;

    [Theory]
    [InlineData(1, 5)] [InlineData(5, 13)] [InlineData(31, 9)]
    [InlineData(33, 15)] [InlineData(513, 256)] [InlineData(1031, 2)]
    public void TwoRowsPreserveScalarFmaBitsEnvelopeGuardsAndBorrowedOwnership(int width, int target)
    {
        if (!Supported) return;
        using var source = ImageBuffer.Create(width, 3, 16);
        for (var y = 0; y < 3; y++) for (var x = 0; x < width; x++)
        {
            source.SetSample(x, y, 0, 65535 - y);
            source.SetSample(x, y, 1, (x + y) % 2 == 0 ? 65535 : 32767);
            source.SetSample(x, y, 2, (x + y) % 3 == 0 ? 0 : 65534);
            source.SetSample(x, y, 3, ((x + y) % 5) switch { 0 => 0, 1 => 1, 2 => 16, 3 => 32768, _ => 65535 });
        }
        var original = source.Pixels.ToArray();
        using var root = new TestMemoryBudget(1 << 26);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var plan = LanczosPlan.Create(width, target, context);
        const uint productGuard = 0xdec0ded;
        const ulong outputGuard = 0xfff8abcd12345678;
        var products = Enumerable.Repeat(productGuard, width * 8 + 8).ToArray();
        var first = Enumerable.Repeat(BitConverter.UInt64BitsToDouble(outputGuard), target * 4 + 8).ToArray();
        var second = first.ToArray(); var charge = root.Bytes; var attempts = root.Attempts;
        void Apply() => LanczosFmaResize.HorizontalWordPair(source, 0, 2, plan,
            first.AsSpan(4, target * 4), second.AsSpan(4, target * 4), products.AsSpan(4, width * 8), context);
        Apply(); var allocated = GC.GetAllocatedBytesForCurrentThread(); Apply();
        Assert.Equal(allocated, GC.GetAllocatedBytesForCurrentThread());
        Assert.Equal(charge, root.Bytes); Assert.Equal(attempts, root.Attempts);
        Assert.Equal(original, source.Pixels.ToArray()); Assert.False(root.IsDisposed);
        for (var i = 0; i < 4; i++)
        {
            Assert.Equal(productGuard, products[i]); Assert.Equal(productGuard, products[width * 8 + 4 + i]);
            foreach (var output in new[] { first, second })
            {
                Assert.Equal(outputGuard, BitConverter.DoubleToUInt64Bits(output[i]));
                Assert.Equal(outputGuard, BitConverter.DoubleToUInt64Bits(output[target * 4 + 4 + i]));
            }
        }
        Check(first, 0); Check(second, 2);
        void Check(double[] output, int y)
        {
            for (var x = 0; x < target; x++) for (var c = 0; c < 4; c++)
            {
                var window = plan.Windows[x]; BigInteger exact = 0;
                double Value(int t) => (double)source.Sample(window.First + t, y, 3)
                    * (c == 3 ? 1 : source.Sample(window.First + t, y, c));
                for (var t = 0; t < window.Count; t++)
                    exact += (BigInteger)source.Sample(window.First + t, y, 3)
                        * (c == 3 ? 1 : source.Sample(window.First + t, y, c)) * plan.Weights[window.Offset + t];
                var actual = output[4 + x * 4 + c];
                Assert.True(BigInteger.Abs((BigInteger)actual - exact) <= (c == 3 ? 0 : (window.Count + 3L) * 512));
                // Four independent scalar chains describe the declared word
                // arithmetic without sharing any SIMD preparation or views.
                double a = 0, b = 0, d = 0, e = 0; var tap = 0;
                for (; tap + 3 < window.Count; tap += 4)
                {
                    a = Math.FusedMultiplyAdd(Value(tap), plan.Weights[window.Offset + tap], a);
                    b = Math.FusedMultiplyAdd(Value(tap + 1), plan.Weights[window.Offset + tap + 1], b);
                    d = Math.FusedMultiplyAdd(Value(tap + 2), plan.Weights[window.Offset + tap + 2], d);
                    e = Math.FusedMultiplyAdd(Value(tap + 3), plan.Weights[window.Offset + tap + 3], e);
                }
                var expected = (a + d) + (b + e);
                for (; tap < window.Count; tap++)
                    expected = Math.FusedMultiplyAdd(Value(tap), plan.Weights[window.Offset + tap], expected);
                Assert.Equal(BitConverter.DoubleToUInt64Bits(expected), BitConverter.DoubleToUInt64Bits(actual));
            }
        }
    }

    [Fact]
    public void ProductPairChecksCancellationBeforeBorrowedMutation()
    {
        if (!Supported) return;
        using var source = ImageBuffer.Create(13, 2, 16);
        using var geometry = new OperationContext(ImageLimits.Default, default);
        using var plan = LanczosPlan.Create(13, 5, geometry);
        var output = Enumerable.Repeat(double.NaN, 40).ToArray();
        var products = Enumerable.Repeat(0xdec0dedu, 13 * 8).ToArray();
        using var root = new TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => LanczosFmaResize.HorizontalWordPair(source, 0, 1,
            plan, output.AsSpan(0, 20), output.AsSpan(20, 20), products, context));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.All(output, value => Assert.True(double.IsNaN(value)));
        Assert.All(products, value => Assert.Equal(0xdec0dedu, value));
        Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void TwoProductRowsStayAdmittedAcrossAggregateUseRefusalAndRecovery()
    {
        if (!Supported) return;
        using var geometry = new OperationContext(ImageLimits.Default, default);
        using var h = LanczosPlan.Create(37, 19, geometry); using var v = LanczosPlan.Create(23, 11, geometry);
        long required;
        using (var probe = new LanczosWorkspace(h, v, 16, true, geometry)) required = probe.Bytes;
        using var root = new TestMemoryBudget(required * 2);
        using var owner = new OperationContext(new() { MemoryBudget = root }, default);
        using var first = new LanczosWorkspace(h, v, 16, true, owner);
        using var second = new LanczosWorkspace(h, v, 16, true, owner);
        Assert.Equal(37 * 8, first.Products.Length); Assert.Equal(required * 2, root.Bytes);
        var attempts = root.GrowthAttempts;
        var refused = Assert.Throws<ImageLimitException>(() => new LanczosWorkspace(h, v, 16, true, owner));
        Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope);
        Assert.Equal(attempts + 1, root.GrowthAttempts); Assert.Equal(required * 2, root.Bytes);
        first.Dispose(); Assert.Equal(required, root.Bytes);
        using (var recovery = new LanczosWorkspace(h, v, 16, true, owner)) Assert.Equal(required * 2, root.Bytes);
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void LocalCeilingChoosesOneProductRowBeforeSharedAdmission()
    {
        if (!Supported) return;
        using var geometry = new OperationContext(ImageLimits.Default, default);
        using var h = LanczosPlan.Create(37, 19, geometry); using var v = LanczosPlan.Create(23, 11, geometry);
        long single;
        using (var probe = new LanczosWorkspace(h, v, 16, true, geometry)) single = probe.Bytes - 37 * 16L;
        using var root = new TestMemoryBudget(single);
        using var owner = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = single }, default);
        using var workspace = new LanczosWorkspace(h, v, 16, true, owner);
        Assert.Equal(single, workspace.Bytes); Assert.Equal(37 * 4, workspace.Products.Length);
        Assert.Equal(single, root.Bytes); Assert.Equal(1, root.GrowthAttempts); Assert.Equal(0, root.Refusals);
        workspace.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void OneShotLocalLimitKeepsRoomForItsPendingOutput()
    {
        if (!Supported) return;
        using var source = OriginalPixels.Buffer(39, 9, 16);
        using var geometry = new OperationContext(ImageLimits.Default, default);
        using var h = LanczosPlan.Create(39, 19, geometry); using var v = LanczosPlan.Create(9, 4, geometry);
        long single;
        using (var probe = new LanczosWorkspace(h, v, 16, true, geometry)) single = probe.Bytes - 39 * 16L;
        const int outputBytes = 19 * 4 * 8;
        // Original fast selection includes another sum row. The extra product
        // row is sixteen bytes larger, so it must not take the result's room.
        var limit = source.ByteLength + h.StorageBytes + v.StorageBytes + single + 19 * 4 * 8L + outputBytes;
        using var expected = ImageTransforms.Resize(source, 19, 4, ResizeFilter.Lanczos3);
        using var root = new TestMemoryBudget(1 << 20);
        using (var result = ImageTransforms.Resize(source, 19, 4, ResizeFilter.Lanczos3,
            new() { MemoryBudget = root, MaxWorkingBytes = limit }))
        {
            Assert.Equal(expected.Pixels.ToArray(), result.Pixels.ToArray());
            Assert.Equal(outputBytes, root.Bytes);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationDuringAdmissionReleasesBothProductRowsBeforeReuse()
    {
        if (!Supported) return;
        using var geometry = new OperationContext(ImageLimits.Default, default);
        using var h = LanczosPlan.Create(37, 19, geometry); using var v = LanczosPlan.Create(23, 11, geometry);
        long required;
        using (var probe = new LanczosWorkspace(h, v, 16, true, geometry)) required = probe.Bytes;
        using var root = new TestMemoryBudget(required); using var cancellation = new CancellationTokenSource();
        using var owner = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        root.Admitted = _ => cancellation.Cancel();
        try
        {
            var error = Assert.Throws<OperationCanceledException>(() => new LanczosWorkspace(h, v, 16, true, owner));
            Assert.Equal(cancellation.Token, error.CancellationToken);
        }
        finally { root.Admitted = null; }
        Assert.Equal(0, root.Bytes);
        using var next = new OperationContext(new() { MemoryBudget = root }, default);
        using (var recovery = new LanczosWorkspace(h, v, 16, true, next)) Assert.Equal(required, root.Bytes);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
