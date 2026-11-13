using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicHorizontalQuartetTests
{
    [Fact]
    public void QuartetPreservesOrderedScalarBitsInputsAndCacheGuardsWithoutAllocation()
    {
        if (!Avx512F.IsSupported) return;
        var random = new Random(921);
        var decoded = new Vector256<double>[30]; var weights = new double[72];
        var result = new Vector256<double>[6];
        double[] values = [-0d, 0, 1e-200, Math.BitDecrement(.5), Math.BitIncrement(.5), -65025, 65025, 4294836225d];
        for (var trial = 0; trial < 128; trial++)
        {
            decoded[0] = decoded[29] = Vector256.Create(double.NaN);
            for (var p = 1; p <= 28; p++)
                decoded[p] = Vector256.Create(values[random.Next(values.Length)], values[random.Next(values.Length)],
                    values[random.Next(values.Length)], values[random.Next(values.Length)]);
            for (var pixel = 0; pixel < 4; pixel++)
            {
                weights[pixel * 18] = weights[pixel * 18 + 17] = double.NaN;
                for (var t = 0; t < 16; t++)
                    weights[pixel * 18 + t + 1] = (trial + pixel + t) % 3 == 0 ? -(t + 1) * .0625 : (t + 1) * .1;
            }
            var sourceCopy = decoded.ToArray(); var weightCopy = weights.ToArray();
            Array.Fill(result, Vector256.Create(double.NaN));
            Run(decoded, weights, result, default);
            for (var pixel = 0; pixel < 4; pixel++)
            for (var channel = 0; channel < 4; channel++)
            {
                double expected = 0;
                for (var t = 0; t < 16; t++)
                    expected += Product(decoded[1 + pixel * 4 + t].GetElement(channel), weights[pixel * 18 + t + 1]);
                Assert.Equal(BitConverter.DoubleToUInt64Bits(expected), BitConverter.DoubleToUInt64Bits(result[pixel + 1].GetElement(channel)));
            }
            Assert.Equal(Vector256.Create(double.NaN).AsUInt64(), result[0].AsUInt64());
            Assert.Equal(Vector256.Create(double.NaN).AsUInt64(), result[5].AsUInt64());
            for (var i = 0; i < decoded.Length; i++) Assert.Equal(sourceCopy[i].AsUInt64(), decoded[i].AsUInt64());
            Assert.Equal(weightCopy, weights);
        }
        Run(decoded, weights, result, default);
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 8; i++) Run(decoded, weights, result, default);
        Assert.Equal(allocated, GC.GetAllocatedBytesForCurrentThread());
    }

    private static void Run(Vector256<double>[] decoded, double[] weights, Vector256<double>[] result, CancellationToken cancellation)
        => BicubicMath.FilterOverlappingHorizontalQuartet(decoded.AsSpan(1, 28),
            weights.AsSpan(1, 16), weights.AsSpan(19, 16), weights.AsSpan(37, 16), weights.AsSpan(55, 16),
            result.AsSpan(1, 4), cancellation);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double Product(double a, double b) => a * b;

    [Fact]
    public void QuartetChecksEveryBoundedWindowBeforeWritingCache()
    {
        if (!Avx512F.IsSupported) return;
        var source = new Vector256<double>[28]; var weights = new double[16];
        var output = Enumerable.Repeat(Vector256.Create(double.NaN), 4).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => BicubicMath.FilterOverlappingHorizontalQuartet(
            source.AsSpan(0, 27), weights, weights, weights, weights, output, default));
        for (var shortWeight = 0; shortWeight < 4; shortWeight++)
        {
            var at = shortWeight;
            Assert.Throws<ArgumentOutOfRangeException>(() => BicubicMath.FilterOverlappingHorizontalQuartet(source,
                weights.AsSpan(0, at == 0 ? 15 : 16), weights.AsSpan(0, at == 1 ? 15 : 16),
                weights.AsSpan(0, at == 2 ? 15 : 16), weights.AsSpan(0, at == 3 ? 15 : 16), output, default));
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => BicubicMath.FilterOverlappingHorizontalQuartet(
            source, weights, weights, weights, weights, output.AsSpan(0, 3), default));
        Assert.All(output, v => Assert.Equal(Vector256.Create(double.NaN).AsUInt64(), v.AsUInt64()));
    }

    [Fact]
    public void CanceledQuartetLeavesPrivateCacheUntouchedAndNextCallSucceeds()
    {
        if (!Avx512F.IsSupported) return;
        var source = Enumerable.Repeat(Vector256.Create(12d), 30).ToArray();
        var weights = Enumerable.Repeat(1d / 16, 72).ToArray();
        var output = Enumerable.Repeat(Vector256.Create(double.NaN), 6).ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => Run(source, weights, output, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.All(output, v => Assert.Equal(Vector256.Create(double.NaN).AsUInt64(), v.AsUInt64()));
        Run(source, weights, output, default);
        for (var i = 1; i <= 4; i++) Assert.Equal(Vector256.Create(12d), output[i]);
    }

    [Theory]
    [InlineData(513, 259, 128, 64)]
    [InlineData(307, 400, 75, 100)]
    public void TinyAlphaFramesPreserveExactPixelsAdmissionAndSurvivingOwners(int width, int height, int outputWidth, int outputHeight)
    {
        using var source = OriginalPixels.Buffer(width, height, 8);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++) source.GetRow8(y)[x * 4 + 3] = (byte)((x + 3 * y) % 3);
        var original = OriginalPixels.Bytes(source); var bytes = outputWidth * outputHeight * 4;
        var golden = ExactResizeReference.Resize(original, width, height, 8, outputWidth, outputHeight, ResizeFilter.Bicubic);
        using var root = new TestMemoryBudget(bytes + 4096);
        using var actual = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + (long)bytes + 4096, MemoryBudget = root });
        Assert.Equal(golden, OriginalPixels.Bytes(actual)); Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(bytes, root.Bytes); Assert.InRange(root.Peak, bytes, bytes + 4096);
        source.Dispose(); Assert.Equal(golden, OriginalPixels.Bytes(actual));
        actual.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
