using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PackedColumnStorageTests
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double RoundedProduct(double sample, double weight) => sample * weight;

    [Fact]
    public void IndexedPairPreservesRoundedBitsAndBorrowedViewsWithoutAllocation()
    {
        if (!Avx512F.IsSupported || !Avx2.IsSupported) return;
        foreach (var word in new[] { false, true }) foreach (var prepared in new[] { false, true })
        foreach (var count in new[] { 0, 1, 7, 33 })
        {
            const int rows = 9, guard = 32;
            var maximum = word ? 65535u : 255u;
            var samples = new double[rows * 4];
            var rowBytes = prepared ? 32 : word ? 16 : 8;
            var data = Enumerable.Repeat((byte)0xa5, guard * 2 + rows * rowBytes).ToArray();
            for (var x = 0; x < rows; x++) for (var c = 0; c < 4; c++)
            {
                var alpha = x % 4 == 0 ? 0u : x % 4 == 1 ? 1u : maximum;
                var color = c == 0 ? maximum : c == 1 ? maximum - (uint)(x % 2) : (uint)(x * 7919) & maximum;
                var product = c == 3 ? alpha : alpha * color;
                samples[x * 4 + c] = product;
                var at = guard + x * rowBytes;
                if (prepared) BinaryPrimitives.WriteInt64LittleEndian(data.AsSpan(at + c * 8, 8), BitConverter.DoubleToInt64Bits(product));
                else if (word) BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(at + c * 4, 4), product);
                else BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(at + c * 2, 2), (ushort)product);
            }
            var original = data.ToArray();
            var offsets0 = Enumerable.Repeat(ushort.MaxValue, count + 3).ToArray();
            var offsets1 = offsets0.ToArray();
            var weights0 = Enumerable.Repeat(double.NaN, count + 3).ToArray();
            var weights1 = weights0.ToArray();
            for (var t = 0; t < count; t++)
            {
                offsets0[t + 1] = (ushort)(t % 3 == 0 ? 0 : t % rows);
                offsets1[t + 1] = (ushort)(t % 3 == 0 ? rows - 1 : (rows - 1 - t % rows));
                weights0[t + 1] = (t % 3) switch { 0 => .1, 1 => -1d / 3, _ => .75 };
                weights1[t + 1] = (t % 3) switch { 0 => -.2, 1 => 1d / 7, _ => .5 };
            }
            Vector512<double> Apply()
            {
                var decoded = data.AsSpan(guard, rows * rowBytes);
                var first = offsets0.AsSpan(1); var second = offsets1.AsSpan(1);
                var w0 = weights0.AsSpan(1); var w1 = weights1.AsSpan(1);
                return prepared
                    ? word
                        ? PackedColumnBicubicResize.FilterIndexedPair<PackedColumnBicubicResize.DecodedPrefix, PackedColumnBicubicResize.WordSamples>(decoded, first, second, w0, w1, count)
                        : PackedColumnBicubicResize.FilterIndexedPair<PackedColumnBicubicResize.DecodedPrefix, PackedColumnBicubicResize.ByteSamples>(decoded, first, second, w0, w1, count)
                    : word
                        ? PackedColumnBicubicResize.FilterIndexedPair<PackedColumnBicubicResize.PackedSuffix, PackedColumnBicubicResize.WordSamples>(decoded, first, second, w0, w1, count)
                        : PackedColumnBicubicResize.FilterIndexedPair<PackedColumnBicubicResize.PackedSuffix, PackedColumnBicubicResize.ByteSamples>(decoded, first, second, w0, w1, count);
            }
            Apply(); var allocated = GC.GetAllocatedBytesForCurrentThread(); var actual = Apply();
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
            for (var c = 0; c < 8; c++)
            {
                var expected = 0d;
                for (var t = 0; t < count; t++)
                    expected += RoundedProduct(samples[(c < 4 ? offsets0[t + 1] : offsets1[t + 1]) * 4 + c % 4],
                        c < 4 ? weights0[t + 1] : weights1[t + 1]);
                Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual.GetElement(c)));
            }
            Assert.Equal(original, data);
            Assert.Equal(ushort.MaxValue, offsets0[0]); Assert.Equal(ushort.MaxValue, offsets1[^1]);
            Assert.True(double.IsNaN(weights0[0])); Assert.True(double.IsNaN(weights1[^1]));
        }
    }

    [Fact]
    public void ContiguousPairProofRetainsClippedAndUnequalCountFallbacks()
    {
        int[] counts = [3, 3, 3, 3];
        ushort[] offsets = [0, 1, 2, 2, 3, 4, 1, 2, 3, 3, 4, 5];
        Assert.Equal(5, PackedColumnBicubicResize.PrepareContiguousPairs(counts, offsets, 3));
        offsets[1] = 0; // Repeated source sample at a clipped edge.
        Assert.Equal(4, PackedColumnBicubicResize.PrepareContiguousPairs(counts, offsets, 3));
        counts[3] = 2;
        Assert.Equal(0, PackedColumnBicubicResize.PrepareContiguousPairs(counts, offsets, 3));
        counts[3] = 3; offsets[10] = 5; // An interior discontinuity is also rejected.
        Assert.Equal(0, PackedColumnBicubicResize.PrepareContiguousPairs(counts, offsets, 3));
    }

    [Theory]
    [InlineData(513, 259, 64, 32, true)]
    [InlineData(769, 389, 96, 48, true)]
    [InlineData(321, 259, 40, 32, true)]
    [InlineData(553, 277, 64, 32, false)]
    [InlineData(513, 251, 64, 31, false)]
    [InlineData(505, 259, 63, 32, false)]
    [InlineData(193, 259, 24, 32, false)]
    public void DecodedPrefixAndPackedSuffixOverwritePoisonAndMatchIndependentCache(
        int sourceWidth, int sourceHeight, int width, int height, bool usesLoan)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
        var primary = sourceWidth == 513 && sourceHeight == 259;
        if (primary) source.GetRow8(0)[3] = source.GetRow8(0)[sourceWidth * 4 - 1] = 0;
        else for (var y = 0; y < sourceHeight; y++) for (var x = 0; x < sourceWidth; x++)
            source.GetRow8(y)[x * 4 + 3] = 255;
        var input = OriginalPixels.Bytes(source);
        // Default full-row caching is a distinct, unchanged numeric path.
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(width, height, 8); result.Pixels.Fill(0xa5);
        var sx = (double)sourceWidth / width; var sy = (double)sourceHeight / height;
        Assert.Equal(usesLoan, PackedColumnBicubicResize.TryLocateResultCache(result, sx, sy, out var start, out var prefix));
        if (usesLoan)
        {
            Assert.Equal(result.Stride - 128, start); Assert.True(prefix * 4 <= start);
            Assert.True(31L * result.Stride + start + 128 <= result.ByteLength);
        }
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(PackedColumnBicubicResize.TryApply(source, result, sx, sy,
            (long)Math.Ceiling(4 * sx) + 1, (long)Math.Ceiling(4 * sy) + 1, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        if (primary)
        {
            Assert.Equal("70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80",
                Convert.ToHexStringLower(SHA256.HashData(input)));
            Assert.Equal("e2340f662ce94418bed4e7be5c65ac4c73fb7a705b047af06784e42b37c267c6",
                Convert.ToHexStringLower(SHA256.HashData(result.Pixels)));
        }
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(4096, root.Peak); Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RefusalAndCancellationReleaseWorkspaceBeforeSuccessfulRecovery()
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        using var result = ImageBuffer.Create(64, 32, 8); result.Pixels.Fill(0xa5);
        using var denied = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = denied }, default))
            Assert.Throws<ImageLimitException>(() => PackedColumnBicubicResize.TryApply(source, result, 513d / 64, 259d / 32, 34, 34, context));
        Assert.Equal(1, denied.Attempts); Assert.Equal(1, denied.Refusals); Assert.Equal(0, denied.Bytes);
        Assert.False(result.Pixels.ContainsAnyExcept((byte)0xa5));
        using var root = new TestMemoryBudget(4096);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => PackedColumnBicubicResize.TryApply(source, result, 513d / 64, 259d / 32, 34, 34, context));
        Assert.Equal(0, root.Bytes); Assert.False(result.Pixels.ContainsAnyExcept((byte)0xa5));
        root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(PackedColumnBicubicResize.TryApply(source, result, 513d / 64, 259d / 32, 34, 34, context));
        Assert.Equal("e2340f662ce94418bed4e7be5c65ac4c73fb7a705b047af06784e42b37c267c6",
            Convert.ToHexStringLower(SHA256.HashData(result.Pixels)));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void TinyAlphaDeclineRetiresBorrowedCacheBeforeExactPublicReplay()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(513, 259, 8);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow8(y).Slice(x * 4, 4);
            pixel[0] = 19; pixel[1] = 103; pixel[2] = 241; pixel[3] = y < 130 ? (byte)1 : (byte)0;
        }
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(64, 32, 8); result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(result.ByteLength + 4096);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.False(PackedColumnBicubicResize.TryApply(source, result, 513d / 64, 259d / 32, 34, 34, context));
        Assert.Equal(0, root.Bytes);
        using (var recovered = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + result.ByteLength + 4096, MemoryBudget = root }))
        {
            Assert.Equal(result.ByteLength, root.Bytes);
            Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(recovered));
            Assert.Equal(new byte[] { 19, 103, 241, 1 }, recovered.GetRow8(0)[..4].ToArray());
            Assert.Equal(new byte[4], recovered.GetRow8(31)[..4].ToArray());
        }
        Assert.True(root.Peak <= result.ByteLength + 4096); Assert.Equal(0, root.Bytes);
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SharedRootKeepsResultsIndependentAcrossRefusalDisposalAndRecovery()
    {
        const int bytes = 64 * 32 * 4;
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        using var root = new TestMemoryBudget(2 * bytes + 4096);
        var limits = new ImageLimits { MaxWorkingBytes = source.ByteLength + bytes + 4096, MemoryBudget = root };
        using var first = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(second);
        Assert.Equal("e2340f662ce94418bed4e7be5c65ac4c73fb7a705b047af06784e42b37c267c6",
            Convert.ToHexStringLower(SHA256.HashData(expected)));
        Assert.Equal(2 * bytes, root.Bytes); Assert.True(root.Peak <= 2 * bytes + 4096);
        var attempts = root.Attempts;
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits));
        Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(2 * bytes, root.Bytes);
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        Assert.Equal(bytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
        using var recovered = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(recovered)); Assert.False(root.IsDisposed);
    }
}
