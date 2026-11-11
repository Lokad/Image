using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PackedHorizontalQuartetTests
{
    [Theory]
    [InlineData(255, false)] [InlineData(65535, false)]
    [InlineData(255, true)] [InlineData(65535, true)]
    public void EveryOrderedDoubleAndBorrowedGuardStaysExact(int maximum, bool tiny)
    {
        if (!Avx.IsSupported) return;
        var random = new Random(828);
        var decoded = Enumerable.Repeat(Vector256.Create(double.NaN), 58).ToArray();
        for (var p = 0; p < 56; p++)
        {
            var alpha = tiny ? p % 2 : random.Next(maximum + 1);
            decoded[p + 1] = Vector256.Create((double)alpha * random.Next(maximum + 1),
                (double)alpha * random.Next(maximum + 1), (double)alpha * random.Next(maximum + 1), alpha);
        }
        var weights = Enumerable.Repeat(double.NaN, 134).ToArray();
        for (var c = 0; c < 4; c++) for (var t = 0; t < 32; t++)
            weights[1 + c * 33 + t] = (t % 4) switch { 0 => .1, 1 => -1d / 3, 2 => .75, _ => -0d };
        var originalDecoded = MemoryMarshal.AsBytes(decoded.AsSpan()).ToArray();
        var originalWeights = MemoryMarshal.AsBytes(weights.AsSpan()).ToArray();
        var output = Enumerable.Repeat(Vector256.Create(double.NaN), 6).ToArray();
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        PackedColumnBicubicResize.FilterContiguousQuartet(decoded.AsSpan(1, 56), weights.AsSpan(1, 132), output.AsSpan(1, 4), context);
        for (var c = 0; c < 4; c++) for (var channel = 0; channel < 4; channel++)
        {
            double expected = 0;
            for (var t = 0; t < 32; t++)
                expected += RoundedProduct(decoded[1 + c * 8 + t].GetElement(channel), weights[1 + c * 33 + t]);
            Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(output[1 + c].GetElement(channel)));
        }
        for (var channel = 0; channel < 4; channel++)
        {
            Assert.True(double.IsNaN(output[0].GetElement(channel)));
            Assert.True(double.IsNaN(output[5].GetElement(channel)));
        }
        Assert.Equal(originalDecoded, MemoryMarshal.AsBytes(decoded.AsSpan()).ToArray());
        Assert.Equal(originalWeights, MemoryMarshal.AsBytes(weights.AsSpan()).ToArray());
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double RoundedProduct(double sample, double weight) => sample * weight;

    private static ushort[] Offsets()
    {
        var result = new ushort[132];
        for (var c = 0; c < 4; c++) for (var t = 0; t < 33; t++) result[c * 33 + t] = (ushort)(c * 8 + t);
        return result;
    }

    [Fact]
    public void FourContiguousThirtyTwoTapWindowsAcquireProofWithoutChangingGeometry()
    {
        int[] counts = [32, 32, 32, 32]; var offsets = Offsets(); var original = offsets.ToArray();
        var pairs = PackedColumnBicubicResize.PrepareContiguousPairs(counts, offsets, 33);
        Assert.True(PackedColumnBicubicResize.IsContiguousQuartet(counts, offsets, 33, 56, pairs));
        Assert.Equal(original, offsets); Assert.Equal(new[] { 32, 32, 32, 32 }, counts);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void UnequalClampedShiftedShortAndPartialWindowsKeepTheirFallback(int variant)
    {
        int[] counts = [32, 32, 32, 32]; var offsets = Offsets(); var taps = 33; var sourceCount = 56;
        switch (variant)
        {
            case 0: counts[3] = 33; break;
            case 1: offsets[1] = 0; break;
            case 2: for (var c = 1; c < 4; c++) for (var t = 0; t < 33; t++) offsets[c * 33 + t]++; break;
            case 3: sourceCount = 55; break;
            case 4: counts = counts[..3]; break;
            case 5: taps = 32; break;
            case 6: for (var t = 0; t < offsets.Length; t++) offsets[t]++; break;
            case 7: for (var c = 2; c < 4; c++) for (var t = 0; t < 33; t++) offsets[c * 33 + t]++; break;
        }
        var original = offsets.ToArray(); var oldCounts = counts.ToArray();
        var pairs = PackedColumnBicubicResize.PrepareContiguousPairs(counts, offsets, taps);
        Assert.False(PackedColumnBicubicResize.IsContiguousQuartet(counts, offsets, taps, sourceCount, pairs));
        Assert.Equal(original, offsets); Assert.Equal(oldCounts, counts);
    }

    [Fact]
    public void PreCanceledExecutionLeavesBorrowedCacheUntouched()
    {
        var decoded = new Vector256<double>[56]; var weights = new double[132];
        var cache = Enumerable.Repeat(Vector256.Create(double.NaN), 4).ToArray();
        using var root = new TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => PackedColumnBicubicResize.FilterContiguousQuartet(decoded, weights, cache, context));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        foreach (var pixel in cache) for (var c = 0; c < 4; c++) Assert.True(double.IsNaN(pixel.GetElement(c)));
        Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void NumericExecutionAllocatesNothingAndDoesNotAdmitBorrowedStorage()
    {
        if (!Avx.IsSupported) return;
        var decoded = Enumerable.Repeat(Vector256.Create(65535d, 4294836225d, 1d, 65535d), 56).ToArray();
        var weights = Enumerable.Repeat(.1, 132).ToArray(); var cache = new Vector256<double>[4];
        using var root = new TestMemoryBudget(0); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        PackedColumnBicubicResize.FilterContiguousQuartet(decoded, weights, cache, context);
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        for (var repeat = 0; repeat < 10; repeat++)
            PackedColumnBicubicResize.FilterContiguousQuartet(decoded, weights, cache, context);
        Assert.Equal(allocated, GC.GetAllocatedBytesForCurrentThread());
        Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void WordDeclineRetiresQuartetLoanBeforePublicExactReplay()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(513, 259, 16);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow16(y).Slice(x * 4, 4);
            pixel[0] = 19; pixel[1] = 103; pixel[2] = 241; pixel[3] = x is >= 24 and < 96 ? (ushort)1 : ushort.MaxValue;
        }
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic);
        using var abandoned = ImageBuffer.Create(64, 32, 16); abandoned.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(abandoned.ByteLength + 4096);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.False(PackedColumnBicubicResize.TryApply(source, abandoned, 513d / 64, 259d / 32, 34, 34, context));
        Assert.Equal(0, root.Bytes);
        using (var recovered = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + abandoned.ByteLength + 4096, MemoryBudget = root }))
        {
            Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(recovered));
            Assert.Equal(abandoned.ByteLength, root.Bytes);
        }
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.Equal(0, root.Bytes);
        Assert.True(root.Peak <= abandoned.ByteLength + 4096); Assert.False(root.IsDisposed);
    }
}
