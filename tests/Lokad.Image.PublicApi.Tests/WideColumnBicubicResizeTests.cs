using System.Security.Cryptography;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WideColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 128L * 64 * 18 * 18 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 128L * 64 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("5067190b92fe644a75b0001848c4a555feb638c72592239dbf0ea0d63c518e71",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(65, 37, 15, 9, false)] [InlineData(81, 39, 18, 9, false)]
    [InlineData(67, 39, 15, 9, false)] [InlineData(71, 43, 15, 10, false)]
    [InlineData(65, 37, 15, 9, true)] [InlineData(81, 39, 18, 9, true)]
    [InlineData(51, 37, 13, 9, false)] [InlineData(55, 37, 14, 9, false)]
    [InlineData(59, 37, 15, 9, false)] [InlineData(66, 33, 16, 8, false)]
    [InlineData(59, 37, 15, 9, true)]
    public void WiderSupportKeepsStripTailsNegativeLobesAndTinyAlphaExact(
        int sourceWidth, int sourceHeight, int width, int height, bool tiny)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
        if (tiny)
            for (var y = 0; y < sourceHeight; y++)
            for (var x = 0; x < sourceWidth; x++)
                source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 7 == 0 ? 255 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var normal = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void FourColumnWorkspaceKeepsRetainedOwnersAndTerminalAdmissionCleanup()
    {
        const int inputBytes = 513 * 259 * 4, outputBytes = 128 * 64 * 4;
        using var root = new TestMemoryBudget(inputBytes + outputBytes + 4096);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = inputBytes + outputBytes + 4096 };
        using var source = ImageBuffer.Create(513, 259, 8, limits);
        using (var authored = OriginalPixels.Buffer(513, 259, 8))
            for (var y = 0; y < 259; y++) authored.GetRow8(y).CopyTo(source.GetRow8(y));
        var original = OriginalPixels.Bytes(source); var start = root.GrowthAttempts;
        var stripScratch = Avx512F.IsSupported && Avx2.IsSupported && Avx.IsSupported && Sse41.IsSupported ? 4096 : 0;
        var first = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits);
        var growths = root.GrowthAttempts - start;
        Assert.Equal(inputBytes + outputBytes, root.Bytes);
        Assert.Equal(inputBytes + outputBytes + stripScratch, root.Peak);
        var refusal = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope);
        Assert.Equal(inputBytes + outputBytes, root.Bytes); first.Dispose();
        for (var i = 1; i <= growths; i++)
        {
            root.RefuseAtGrowth = root.GrowthAttempts + i;
            var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
            Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts); Assert.Equal(inputBytes, root.Bytes);
        }
        root.RefuseAtGrowth = 0;
        using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes == inputBytes + outputBytes + stripScratch) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits, cancellation.Token));
        Assert.Equal(inputBytes, root.Bytes); root.Admitted = null;
        using var next = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits);
        Assert.Equal(original, OriginalPixels.Bytes(source)); source.Dispose();
        Assert.Equal(outputBytes, root.Bytes); Assert.NotEmpty(OriginalPixels.Bytes(next));
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
