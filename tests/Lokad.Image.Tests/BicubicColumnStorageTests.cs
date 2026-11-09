using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicColumnStorageTests
{
    [Theory]
    [InlineData(513, 259, 256, 129, true)]
    [InlineData(385, 259, 192, 129, true)]
    [InlineData(576, 288, 256, 128, true)]
    [InlineData(257, 65, 128, 32, false)]
    [InlineData(511, 259, 255, 129, false)]
    [InlineData(513, 17, 256, 8, false)]
    public void StridedCacheAndSuffixOverwritePoisonAndMatchIndependentCachedPixels(
        int sourceWidth, int sourceHeight, int width, int height, bool usesLoan)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
        var primary = sourceWidth == 513 && sourceHeight == 259;
        if (primary) source.GetRow8(0)[3] = source.GetRow8(0)[sourceWidth * 4 - 1] = 0;
        else for (var y = 0; y < sourceHeight; y++) for (var x = 0; x < sourceWidth; x++)
            source.GetRow8(y)[x * 4 + 3] = 255;
        var input = OriginalPixels.Bytes(source);
        // Default full-row caching is a distinct already-qualified execution
        // path, outside this low-memory column dispatch.
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(width, height, 8); result.Pixels.Fill(0xa5);
        var sx = (double)sourceWidth / width; var sy = (double)sourceHeight / height;
        var rowSlots = sy < 2.25 ? 9 : 10;
        Assert.Equal(usesLoan, ColumnBicubicResize.TryLocateResultCache(result, rowSlots, out var start, out var prefix));
        if (usesLoan)
        {
            Assert.True(prefix * 4 <= start);
            Assert.True(start + 512 <= width * 4 - 96);
            Assert.True((long)(rowSlots - 1) * result.Stride + start + 512 <= result.ByteLength);
        }
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(ColumnBicubicResize.TryApply(source, result, sx, sy,
            (long)Math.Ceiling(4 * sx) + 1, (long)Math.Ceiling(4 * sy) + 1, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        if (primary) Assert.Equal("656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8",
            Convert.ToHexStringLower(SHA256.HashData(result.Pixels)));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(4096, root.Peak); Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ColumnRefusalIsTerminalAndCancellationCleansBeforeRecovery()
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        using var result = ImageBuffer.Create(256, 129, 8); result.Pixels.Fill(0xa5);
        using var denied = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = denied }, default))
            Assert.Throws<ImageLimitException>(() => ColumnBicubicResize.TryApply(source, result, 513d / 256, 259d / 129, 10, 10, context));
        Assert.Equal(1, denied.Attempts); Assert.Equal(1, denied.Refusals); Assert.Equal(0, denied.Bytes);
        Assert.True(result.Pixels.ContainsAnyExcept((byte)0xa5) is false);
        using var root = new TestMemoryBudget(4096);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => ColumnBicubicResize.TryApply(source, result, 513d / 256, 259d / 129, 10, 10, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(ColumnBicubicResize.TryApply(source, result, 513d / 256, 259d / 129, 10, 10, context));
        Assert.Equal("656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8",
            Convert.ToHexStringLower(SHA256.HashData(result.Pixels)));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void DeclinedTinyAlphaCoreRetiresBeforePublicReplayAndReleasesTheRoot()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(513, 259, 8);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow8(y).Slice(x * 4, 4);
            pixel[0] = 19; pixel[1] = 103; pixel[2] = 241; pixel[3] = y < 130 ? (byte)1 : (byte)0;
        }
        var input = OriginalPixels.Bytes(source);
        using var result = ImageBuffer.Create(256, 129, 8); result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(result.ByteLength + 4096);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.False(ColumnBicubicResize.TryApply(source, result, 513d / 256, 259d / 129, 10, 10, context));
        Assert.Equal(0, root.Bytes);
        using (var recovered = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + result.ByteLength + 4096, MemoryBudget = root }))
        {
            Assert.Equal(result.ByteLength, root.Bytes);
            for (var y = 0; y < recovered.Height; y++) for (var x = 0; x < recovered.Width; x++)
                Assert.Equal(y <= 64 ? new byte[] { 19, 103, 241, 1 } : new byte[4], recovered.GetRow8(y).Slice(x * 4, 4).ToArray());
        }
        Assert.True(root.Peak <= result.ByteLength + 4096); Assert.Equal(0, root.Bytes);
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void StripsKeepAggregateAdmissionAndIndependentRetainedResults()
    {
        const int bytes = 256 * 129 * 4;
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        using var root = new TestMemoryBudget(2 * bytes + 4096);
        var limits = new ImageLimits { MaxWorkingBytes = source.ByteLength + bytes + 4096, MemoryBudget = root };
        using var first = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(second);
        Assert.Equal("656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8",
            Convert.ToHexStringLower(SHA256.HashData(expected)));
        Assert.Equal(2 * bytes, root.Bytes); Assert.True(root.Peak <= 2 * bytes + 4096);
        var attempts = root.Attempts;
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits));
        Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(2 * bytes, root.Bytes);
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        Assert.Equal(bytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
        using var recovered = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(recovered)); Assert.False(root.IsDisposed);
    }
}
