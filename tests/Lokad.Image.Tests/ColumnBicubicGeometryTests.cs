using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class ColumnBicubicGeometryTests
{
    [Fact]
    public void DifferentOddWidthReductionMatchesThePublicDirectPath()
    {
        if (!System.Runtime.Intrinsics.X86.Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(151, 83, 8);
        for (var y = 0; y < source.Height; y++)
        {
            var row = source.GetRow8(y);
            for (var x = 0; x < source.Width; x++) row[x * 4 + 3] = 255;
        }
        var original = OriginalPixels.Bytes(source);
        // No optional scratch fits the independent public direct execution.
        using var expected = ImageTransforms.Resize(source, 75, 41, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + 75 * 41 * 4 });
        using var result = ImageBuffer.Create(75, 41, 8);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(ColumnBicubicResize.TryApply(source, result, 151d / 75, 83d / 41, 10, 10, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8, 8, 256)]
    [InlineData(8, 8, 257)]
    [InlineData(16, 6, 256)]
    public void UnwrittenTailsSurvivePrefixPublicationAndRetireBeforeTailWrites(int depth, int strip, int width)
    {
        using var root = new TestMemoryBudget(300_000);
        using var result = ImageBuffer.Create(width, 129, depth, new() { MemoryBudget = root });
        result.Pixels.Fill(0xa5); var charged = root.Bytes; var admissions = root.GrowthAttempts;
        var geometry = ColumnBicubicGeometry.Prepare(result, 259, 259d / 129, 10, strip, default);
        Assert.True(geometry.PrefixColumns >= 2 * strip);
        Assert.Equal(0, geometry.PrefixColumns % strip);
        var prefixBytes = geometry.PrefixColumns * 4 * (depth / 8);
        Span<double> expected = stackalloc double[10]; Span<double> actual = stackalloc double[10];
        for (var y = 0; y < result.Height; y++)
        {
            var row = result.Pixels.Slice(y * result.Stride, result.Stride);
            Assert.Equal(-1, row[..prefixBytes].IndexOfAnyExcept((byte)0xa5));
            geometry.Read(y, true, expected, out var first, out var count, out var normalizer);
            row[..prefixBytes].Fill(0x37);
            geometry.Read(y, true, actual, out var actualFirst, out var actualCount, out var actualNormalizer);
            Assert.Equal((first, count, BitConverter.DoubleToInt64Bits(normalizer)),
                (actualFirst, actualCount, BitConverter.DoubleToInt64Bits(actualNormalizer)));
            Assert.Equal(MemoryMarshal.Cast<double, long>(expected[..count]).ToArray(),
                MemoryMarshal.Cast<double, long>(actual[..count]).ToArray());
            // Once every prefix strip is done, tail publication retires these
            // bytes. Direct construction must not read the overwritten record.
            row[prefixBytes..].Fill(0xcc);
            geometry.Read(y, false, actual, out actualFirst, out actualCount, out actualNormalizer);
            Assert.Equal((first, count, BitConverter.DoubleToInt64Bits(normalizer)),
                (actualFirst, actualCount, BitConverter.DoubleToInt64Bits(actualNormalizer)));
            Assert.Equal(MemoryMarshal.Cast<double, long>(expected[..count]).ToArray(),
                MemoryMarshal.Cast<double, long>(actual[..count]).ToArray());
        }
        Assert.Equal(charged, root.Bytes); Assert.Equal(admissions, root.GrowthAttempts);
        result.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SmallResultsKeepDirectConstructionWithoutTouchingPixels()
    {
        using var result = ImageBuffer.Create(9, 4, 8); result.Pixels.Fill(0xa5);
        var geometry = ColumnBicubicGeometry.Prepare(result, 9, 9d / 4, 10, 8, default);
        Assert.Equal(0, geometry.PrefixColumns);
        Span<double> weights = stackalloc double[10];
        geometry.Read(1, false, weights, out _, out var count, out var normalizer);
        Assert.InRange(count, 1, 10); Assert.True(normalizer > 0);
        Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
    }

    [Fact]
    public void CancellationPreservesTheRetainedOwnerAndSameResultAllowsRecovery()
    {
        using var root = new TestMemoryBudget(200_000);
        using var result = ImageBuffer.Create(256, 129, 8, new() { MemoryBudget = root });
        result.Pixels.Fill(0xa5); var charged = root.Bytes;
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() =>
        { ColumnBicubicGeometry.Prepare(result, 259, 259d / 129, 10, 8, cancellation.Token); });
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(charged, root.Bytes); Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        var recovered = ColumnBicubicGeometry.Prepare(result, 259, 259d / 129, 10, 8, default);
        Assert.True(recovered.PrefixColumns > 0); Assert.Equal(charged, root.Bytes);
        result.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
