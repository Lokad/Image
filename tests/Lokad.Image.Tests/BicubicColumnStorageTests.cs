using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicColumnStorageTests
{
    [Theory]
    [InlineData(513, 259, 256, 129, 8, true)]
    [InlineData(385, 259, 192, 129, 8, true)]
    [InlineData(576, 288, 256, 128, 8, true)]
    [InlineData(257, 65, 128, 32, 8, false)]
    [InlineData(511, 259, 255, 129, 8, false)]
    [InlineData(513, 17, 256, 8, 8, false)]
    [InlineData(513, 259, 256, 129, 16, true)]
    [InlineData(385, 259, 192, 129, 16, true)]
    [InlineData(576, 288, 256, 128, 16, true)]
    [InlineData(257, 65, 128, 32, 16, true)]
    [InlineData(511, 259, 255, 129, 16, false)]
    [InlineData(513, 17, 256, 8, 16, false)]
    public void StridedCacheAndSuffixOverwritePoisonAndMatchIndependentCachedPixels(
        int sourceWidth, int sourceHeight, int width, int height, int depth, bool usesLoan)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        var primary = sourceWidth == 513 && sourceHeight == 259;
        if (primary) ClearEdgeAlpha(source);
        else for (var y = 0; y < sourceHeight; y++) for (var x = 0; x < sourceWidth; x++)
            if (depth == 8) source.GetRow8(y)[x * 4 + 3] = 255;
            else source.GetRow16(y)[x * 4 + 3] = 65535;
        var input = OriginalPixels.Bytes(source);
        // Default full-row caching is a distinct already-qualified execution
        // path, outside this low-memory column dispatch.
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(width, height, depth); result.Pixels.Fill(0xa5);
        var sx = (double)sourceWidth / width; var sy = (double)sourceHeight / height;
        var rowSlots = sy < 2.25 && (depth == 8 || sx < 2.25) ? 9 : 10;
        var pixelBytes = depth / 8 * 4;
        Assert.Equal(usesLoan, ColumnBicubicResize.TryLocateResultCache(result, rowSlots, out var start, out var prefix));
        if (usesLoan)
        {
            Assert.True(prefix * pixelBytes <= start);
            Assert.True(start + 512 <= width * pixelBytes - 96);
            Assert.True((long)(rowSlots - 1) * result.Stride + start + 512 <= result.ByteLength);
        }
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(ColumnBicubicResize.TryApply(source, result, sx, sy,
            (long)Math.Ceiling(4 * sx) + 1, (long)Math.Ceiling(4 * sy) + 1, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        if (primary) Assert.Equal(FrozenHash(depth),
            Convert.ToHexStringLower(SHA256.HashData(result.Pixels)));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(4096, root.Peak); Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void ColumnRefusalIsTerminalAndCancellationCleansBeforeRecovery(int depth)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, depth); ClearEdgeAlpha(source);
        using var result = ImageBuffer.Create(256, 129, depth); result.Pixels.Fill(0xa5);
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
        Assert.Equal(FrozenHash(depth),
            Convert.ToHexStringLower(SHA256.HashData(result.Pixels)));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void DeclinedTinyAlphaCoreRetiresBeforePublicReplayAndReleasesTheRoot(int depth)
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(513, 259, depth);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
        {
            if (depth == 8)
            {
                var pixel = source.GetRow8(y).Slice(x * 4, 4);
                pixel[0] = 19; pixel[1] = 103; pixel[2] = 241; pixel[3] = y < 130 ? (byte)1 : (byte)0;
            }
            else
            {
                var pixel = source.GetRow16(y).Slice(x * 4, 4);
                pixel[0] = 19; pixel[1] = 103; pixel[2] = 241; pixel[3] = y < 130 ? (ushort)1 : (ushort)0;
            }
        }
        var input = OriginalPixels.Bytes(source);
        using var result = ImageBuffer.Create(256, 129, depth); result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(result.ByteLength + 4096);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.False(ColumnBicubicResize.TryApply(source, result, 513d / 256, 259d / 129, 10, 10, context));
        Assert.Equal(0, root.Bytes);
        using (var recovered = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + result.ByteLength + 4096, MemoryBudget = root }))
        {
            Assert.Equal(result.ByteLength, root.Bytes);
            for (var y = 0; y < recovered.Height; y++) for (var x = 0; x < recovered.Width; x++)
                if (depth == 8)
                    Assert.Equal(y <= 64 ? new byte[] { 19, 103, 241, 1 } : new byte[4], recovered.GetRow8(y).Slice(x * 4, 4).ToArray());
                else
                    Assert.Equal(y <= 64 ? new ushort[] { 19, 103, 241, 1 } : new ushort[4], recovered.GetRow16(y).Slice(x * 4, 4).ToArray());
        }
        Assert.True(root.Peak <= result.ByteLength + 4096); Assert.Equal(0, root.Bytes);
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void StripsKeepAggregateAdmissionAndIndependentRetainedResults(int depth)
    {
        var bytes = 256 * 129 * 4 * (depth / 8);
        using var source = OriginalPixels.Buffer(513, 259, depth); ClearEdgeAlpha(source);
        using var root = new TestMemoryBudget(2 * bytes + 4096);
        var limits = new ImageLimits { MaxWorkingBytes = source.ByteLength + bytes + 4096, MemoryBudget = root };
        using var first = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(second);
        Assert.Equal(FrozenHash(depth),
            Convert.ToHexStringLower(SHA256.HashData(expected)));
        Assert.Equal(2 * bytes, root.Bytes); Assert.True(root.Peak <= 2 * bytes + 4096);
        var attempts = root.Attempts;
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits));
        Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(2 * bytes, root.Bytes);
        first.Pixels[0] ^= 255; first.Dispose();
        Assert.Equal(bytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
        using var recovered = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(recovered)); Assert.False(root.IsDisposed);
    }

    private static void ClearEdgeAlpha(ImageBuffer source)
    {
        if (source.BitDepth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[source.Width * 4 - 1] = 0;
        else source.GetRow16(0)[3] = source.GetRow16(0)[source.Width * 4 - 1] = 0;
    }

    private static string FrozenHash(int depth) => depth == 8
        ? "656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8"
        : "2676207be4d1a7c9109061fc3c8eb5a0d6b8d10a97bbbc611fbebda3330ffd9f";
}
