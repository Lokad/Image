using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Tests;

public sealed class LanczosPairedOutputTests
{
    [Theory]
    [InlineData(17, 35, 9, 17, false, 8)]
    [InlineData(17, 69, 9, 33, true, 8)]
    [InlineData(7, 41, 3, 20, true, 8)]
    [InlineData(1, 21, 1, 10, false, 8)]
    [InlineData(8, 73, 5, 11, false, 8)]
    [InlineData(9, 1031, 5, 11, false, 8)]
    [InlineData(3, 19, 5, 9, true, 8)]
    [InlineData(3, 35, 5, 17, true, 8)]
    [InlineData(17, 35, 9, 17, false, 16)]
    [InlineData(17, 69, 9, 33, true, 16)]
    [InlineData(7, 41, 3, 20, true, 16)]
    [InlineData(1, 21, 1, 10, false, 16)]
    [InlineData(8, 73, 5, 11, false, 16)]
    [InlineData(9, 1031, 5, 11, false, 16)]
    [InlineData(3, 19, 5, 9, true, 16)]
    [InlineData(3, 35, 5, 17, true, 16)]
    [InlineData(7, 13, 3, 6, true, 16)]
    [InlineData(7, 11, 3, 5, true, 16)]
    [InlineData(37, 29, 19, 11, true, 16)]
    [InlineData(37, 29, 19, 12, true, 16)]
    [InlineData(37, 29, 19, 13, true, 16)]
    [InlineData(37, 29, 19, 14, true, 16)]
    public void SharedOutputPrefixesAndOrdinaryTailsMatchExactIntegerExecution(
        int sourceWidth, int sourceHeight, int width, int height, bool tinyAlpha, int depth)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        if (tinyAlpha)
            for (var y = 0; y < sourceHeight; y++)
            for (var x = 0; x < sourceWidth; x++)
                if (depth == 8)
                    source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 7 == 0 ? 255 : (x + y) % 3 == 0 ? 1 : 0);
                else
                    source.GetRow16(y)[x * 4 + 3] = (ushort)((x + y) % 7 == 0 ? 65535 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var exact = new OperationContext(ImageLimits.Default, default);
        using var horizontal = LanczosPlan.Create(sourceWidth, width, exact);
        using var vertical = LanczosPlan.Create(sourceHeight, height, exact);
        using var rows = new LanczosWorkspace(horizontal, vertical, depth, false, exact);
        using var expected = LanczosResize.Execute(source, width, height, horizontal, vertical, rows, exact);
        using var root = new TestMemoryBudget(1_000_000);
        using var plan = Lanczos3ResizePlan.Create(sourceWidth, sourceHeight, width, height, depth,
            new() { MemoryBudget = root });
        using var first = plan.Resize(source);
        using var second = plan.Resize(source);
        var frozen = OriginalPixels.Bytes(expected);
        Assert.Equal(frozen, OriginalPixels.Bytes(first));
        Assert.Equal(frozen, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        var bytesPerPixel = depth / 2;
        Assert.Equal(plan.StorageBytes + 2L * width * height * bytesPerPixel, root.Bytes);
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(frozen, OriginalPixels.Bytes(second));
        first.Dispose(); source.Dispose(); plan.Dispose();
        Assert.Equal((long)width * height * bytesPerPixel, root.Bytes);
        Assert.Equal(frozen, OriginalPixels.Bytes(second));
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void OutputRefusalAndCancellationReleaseOnlyTheFailedResultBeforeReuse(int depth)
    {
        using var source = OriginalPixels.Buffer(17, 69, depth);
        using var root = new TestMemoryBudget(1_000_000);
        using var plan = Lanczos3ResizePlan.Create(17, 69, 9, 33, depth, new() { MemoryBudget = root });
        var retained = plan.StorageBytes;
        using var survivor = plan.Resize(source);
        var live = root.Bytes;
        var refusals = root.Refusals;
        root.RefuseAtGrowth = root.GrowthAttempts + 1;
        try
        {
            var error = Assert.Throws<ImageLimitException>(() => plan.Resize(source));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
            Assert.Equal(refusals + 1, root.Refusals);
            Assert.Equal(live, root.Bytes);
        }
        finally { root.RefuseAtGrowth = 0; }
        using var canceled = new CancellationTokenSource();
        root.Admitted = _ => canceled.Cancel();
        try
        {
            Assert.Throws<OperationCanceledException>(() => plan.Resize(source, canceled.Token));
            Assert.Equal(live, root.Bytes);
        }
        finally { root.Admitted = null; }
        using (var next = plan.Resize(source))
            Assert.Equal(OriginalPixels.Bytes(survivor), OriginalPixels.Bytes(next));
        Assert.Equal(live, root.Bytes);
        survivor.Dispose(); Assert.Equal(retained, root.Bytes);
        plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(4)] [InlineData(8)] [InlineData(12)] [InlineData(16)]
    public void SharedRowLoadsKeepEachFmaSequenceBitExactAndSpanGuardsUntouched(int channels)
    {
        if (!Avx512F.IsSupported || !Fma.IsSupported) return;
        using var context = new OperationContext(ImageLimits.Default, default);
        var first = Enumerable.Repeat(double.MaxValue, channels + 4).ToArray();
        var second = (double[])first.Clone();
        Array.Clear(first, 2, channels); Array.Clear(second, 2, channels);
        var expected0 = new double[channels]; var expected1 = new double[channels];
        var row = new double[channels]; var random = new Random(1947);
        for (var tap = 0; tap < 17; tap++)
        {
            var weight0 = tap % 2 == 0 ? 31_000_003 : -17_000_009;
            var weight1 = tap % 3 == 0 ? -23_000_011 : 19_000_013;
            for (var i = 0; i < channels; i++)
            {
                row[i] = random.NextInt64(-(1L << 46), 1L << 46);
                expected0[i] = Math.FusedMultiplyAdd(row[i], weight0, expected0[i]);
                expected1[i] = Math.FusedMultiplyAdd(row[i], weight1, expected1[i]);
            }
            LanczosFmaResize.AccumulatePair(row, first.AsSpan(2, channels), second.AsSpan(2, channels),
                weight0, weight1, context);
        }
        for (var i = 0; i < channels; i++)
        {
            Assert.Equal(BitConverter.DoubleToInt64Bits(expected0[i]), BitConverter.DoubleToInt64Bits(first[i + 2]));
            Assert.Equal(BitConverter.DoubleToInt64Bits(expected1[i]), BitConverter.DoubleToInt64Bits(second[i + 2]));
        }
        foreach (var at in new[] { 0, 1, channels + 2, channels + 3 })
        { Assert.Equal(double.MaxValue, first[at]); Assert.Equal(double.MaxValue, second[at]); }
    }
}
